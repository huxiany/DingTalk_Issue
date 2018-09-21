namespace EaseSource.Dingtalk.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using EaseSource.Dingtalk.Entity;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class DingtalkServices : Interfaces.IDingtalkServices
    {
#pragma warning disable SA1310 // Field names must not contain underscore
        private const string DD_ACCESS_TOKEN_CACHE_KEY = "DD_Access_Token";
        private const string DD_JSAPI_TICKET_CACHE_KEY = "DD_JSAPI_Ticket";
#pragma warning restore SA1310 // Field names must not contain underscore

        private readonly ILogger<DingtalkServices> logger;
        private DingtalkConfig dingtalkConfig;
        private IMemoryCache cache;

        private TimeSpan cacheLifeTime = new TimeSpan(1, 50, 0);

        public DingtalkServices(IOptions<DingtalkConfig> dingtalkConfig, IMemoryCache cache, ILogger<DingtalkServices> logger)
        {
            this.dingtalkConfig = dingtalkConfig.Value;
            this.cache = cache;
            this.logger = logger;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            string accessToken = null;
            if (!cache.TryGetValue<string>(DD_ACCESS_TOKEN_CACHE_KEY, out accessToken))
            {
                var uri = string.Format(CultureInfo.InvariantCulture, "{0}/gettoken?corpid={1}&corpsecret={2}", dingtalkConfig.DingtalkAPIBaseUrl, dingtalkConfig.CorpID, dingtalkConfig.Secret);

                JObject jsonObj = await GetDTContentAsync(uri);
                if (jsonObj != null)
                {
                    accessToken = jsonObj["access_token"].ToString();
                }

                if (!string.IsNullOrEmpty(accessToken))
                {
                    cache.Set<string>(DD_ACCESS_TOKEN_CACHE_KEY, accessToken, cacheLifeTime);
                }
            }

            return accessToken;
        }

        public async Task<string> GetTicketAsync(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            string ticket = null;

            if (!cache.TryGetValue<string>(DD_JSAPI_TICKET_CACHE_KEY, out ticket))
            {
                var uri = string.Format(CultureInfo.InvariantCulture, "{0}/get_jsapi_ticket?type=jsapi&access_token={1}", dingtalkConfig.DingtalkAPIBaseUrl, accessToken);

                JObject jsonObj = await GetDTContentAsync(uri);
                if (jsonObj != null)
                {
                    ticket = jsonObj["ticket"].ToString();
                }

                if (!string.IsNullOrEmpty(ticket))
                {
                    cache.Set<string>(DD_JSAPI_TICKET_CACHE_KEY, ticket, cacheLifeTime);
                }
            }

            return ticket;
        }

        public async Task<Dictionary<string, string>> GetMobileConfigAsync(string clientUrl)
        {
            return await GetConfigAsync(clientUrl);
        }

        public async Task<Dictionary<string, string>> GetPCConfigAsync(string clientUrl)
        {
            return await GetConfigAsync(clientUrl);
        }

        public async Task<List<string>> GetUserIdsInRoleAsync(long roleId)
        {
            string method = "dingtalk.corp.role.simplelist";
            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("role_id", roleId.ToString(CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>("size", "100")
            };
            List<string> userIds = new List<string>();
            JObject jsonObj = await InvokeTOPAPI(method, postData);
            if (jsonObj["error_response"] != null)
            {
                LogTOPError(jsonObj);
                return userIds;
            }

            JToken result = jsonObj["dingtalk_corp_role_simplelist_response"];
            if (result == null)
            {
                logger.LogError("The response does not contain dingtalk_corp_role_simplelist_response element");
                return userIds;
            }

            result = result["result"];
            if (result == null)
            {
                logger.LogError("The response does not contain result element");
                return userIds;
            }

            if (result["has_more"] != null && result["has_more"].ToString() == "true")
            {
                logger.LogWarning("Received only partial of users, user list is not complete.");
            }

            result = result["list"];
            JArray userArr = (JArray)result.SelectToken("emp_simple_list");
            if (userArr == null || userArr.Count == 0)
            {
                return userIds;
            }

            userArr.ToList().ForEach(u =>
            {
                string userId = u["userid"].ToString();
                userIds.Add(userId);
            });

            return userIds;
        }

        public async Task<List<DTRole>> GetDTRolesAsync()
        {
            string method = "dingtalk.corp.role.list";
            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("size", "100")
            };
            List<DTRole> roles = new List<DTRole>();
            JObject jsonObj = await InvokeTOPAPI(method, postData);
            if (jsonObj["error_response"] != null)
            {
                LogTOPError(jsonObj);
                return roles;
            }

            JToken result = jsonObj["dingtalk_corp_role_list_response"];
            if (result == null)
            {
                logger.LogError("The response does not contain dingtalk_corp_role_list_response element");
                return roles;
            }

            result = result["result"];
            if (result == null)
            {
                logger.LogError("The response does not contain result element");
                return roles;
            }

            if (result["has_more"].ToString() != "false")
            {
                logger.LogWarning("Received only partial of roles, role list is not complete.");
            }

            result = result["list"];
            JArray rgs = (JArray)result.SelectToken("role_groups");
            rgs.ToList().ForEach(rg =>
            {
                string rgName = rg["group_name"].ToString();

                // 不获取默认组的角色
                if (rgName.Equals("默认", StringComparison.InvariantCulture))
                {
                    return;
                }

                JArray rs = (JArray)rg.SelectToken("roles").SelectToken("roles");
                rs.ToList().ForEach(r =>
                {
                    roles.Add(new DTRole { GroupName = rgName, Id = r["id"].ToObject<long>(), Name = r["role_name"].ToString() });
                });
            });

            return roles;
        }

        public async Task<DTUser> GetDTUserAsync(string ddUserID)
        {
            if (string.IsNullOrEmpty(ddUserID))
            {
                return null;
            }

            DTUser user = null;

            string accessToken = await GetAccessTokenAsync();
            var uri = string.Format(CultureInfo.InvariantCulture, "{0}/user/get?access_token={1}&userid={2}", dingtalkConfig.DingtalkAPIBaseUrl, accessToken, ddUserID);

            JObject jsonObj = await GetDTContentAsync(uri);
            if (jsonObj != null)
            {
                user = JsonConvert.DeserializeObject<DTUser>(jsonObj.ToString());
            }

            return user;
        }

        public async Task<string> GetDTUserIDByAuthCodeAsync(string authCode)
        {
            if (string.IsNullOrEmpty(authCode))
            {
                return null;
            }

            string ddUserID = null;

            string accessToken = await GetAccessTokenAsync();
            var uri = string.Format(CultureInfo.InvariantCulture, "{0}/user/getuserinfo?access_token={1}&code={2}", dingtalkConfig.DingtalkAPIBaseUrl, accessToken, authCode);

            JObject jsonObj = await GetDTContentAsync(uri);
            if (jsonObj != null)
            {
                ddUserID = jsonObj["userid"].ToString();
            }

            return ddUserID;
        }

        public async Task<List<DTUser>> GetAllDTUserFullInfoAsync()
        {
            string accessToken = await GetAccessTokenAsync();
            var uri = string.Format(CultureInfo.InvariantCulture, "{0}/user/list?access_token={1}&department_id=", dingtalkConfig.DingtalkAPIBaseUrl, accessToken);
            return await GetAllDDUserInfoAsync(uri);
        }

        public async Task<List<DTUser>> GetAllDTUserSimpleInfoAsync()
        {
            string accessToken = await GetAccessTokenAsync();
            var uri = string.Format(CultureInfo.InvariantCulture, "{0}/user/simplelist?access_token={1}&department_id=", dingtalkConfig.DingtalkAPIBaseUrl, accessToken);
            return await GetAllDDUserInfoAsync(uri);
        }

        public async Task<List<DTDepartment>> GetDTDepartmentsAsync()
        {
            string accessToken = await GetAccessTokenAsync();

            List<DTDepartment> children = new List<DTDepartment>();

            var uri = string.Format(CultureInfo.InvariantCulture, "{0}/department/list?access_token={1}", dingtalkConfig.DingtalkAPIBaseUrl, accessToken);
            JObject jsonObj = await GetDTContentAsync(uri);
            if (jsonObj != null)
            {
                children = JsonConvert.DeserializeObject<List<DTDepartment>>(jsonObj["department"].ToString());
            }

            return children;
        }

        public async Task<DTDepartment> GetDTDepartmentInfoAsync(int id)
        {
            string accessToken = await GetAccessTokenAsync();

            DTDepartment dept = null;

            var uri = string.Format(CultureInfo.InvariantCulture, "{0}/department/get?access_token={1}&id={2}", dingtalkConfig.DingtalkAPIBaseUrl, accessToken, id);
            JObject jsonObj = await GetDTContentAsync(uri);
            if (jsonObj != null)
            {
                dept = JsonConvert.DeserializeObject<DTDepartment>(jsonObj.ToString());
            }

            return dept;
        }

        public async Task<List<DTAdmin>> GetDTAdminsAsync()
        {
            string accessToken = await GetAccessTokenAsync();

            List<DTAdmin> admins = null;

            var uri = string.Format(CultureInfo.InvariantCulture, "{0}/user/get_admin?access_token={1}", dingtalkConfig.DingtalkAPIBaseUrl, accessToken);
            JObject jsonObj = await GetDTContentAsync(uri);
            if (jsonObj != null)
            {
                admins = JsonConvert.DeserializeObject<List<DTAdmin>>(jsonObj["adminList"].ToString());
            }

            return admins;
        }

        public async Task<DTSendMessageResponse> SendDTMessageAsync(DTMessageBase msg, string uri)
        {
            if (msg == null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new ArgumentNullException(nameof(uri));
            }

            string response = await PostDTContentAsync(uri, msg);
            if (response == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<DTSendMessageResponse>(response);
        }

        public async Task<DTSendMessageResponse> SendDTMessageWithAuthCodeAsync(DTMessageBase msg)
        {
            if (msg == null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            string accessToken = await GetAccessTokenAsync();
            msg.AgentId = dingtalkConfig.AgentID;

            var uri = string.Format(CultureInfo.InvariantCulture, "{0}/message/sendByCode?access_token={1}", dingtalkConfig.DingtalkAPIBaseUrl, accessToken);

            return await SendDTMessageAsync(msg, uri);
        }

        public async Task<DTSendMessageResponse> SendDingtalkLinkMessageAsync(DTLinkMessage msg)
        {
            if (msg == null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            if (!msg.Link.MessageUrl.StartsWith("http", StringComparison.InvariantCulture))
            {
                msg.Link.MessageUrl = new Uri(new Uri(dingtalkConfig.AnDaSMTMobileAppUrl), msg.Link.MessageUrl).AbsoluteUri;
            }

            return await SendDTMessageWithAuthCodeAsync(msg);
        }

        public async Task<DTSendMessageResponse> SendDingtalkMarkDownMessageAsync(DTMarkDownMessage msg)
        {
            if (msg == null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            return await SendDTMessageWithoutCodeAsync(msg);
        }

        public async Task<DTSendMessageResponse> SendDTMessageWithoutCodeAsync(DTMessageBase msg)
        {
            if (msg == null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            string accessToken = await GetAccessTokenAsync();
            msg.AgentId = dingtalkConfig.AgentID;

            var uri = string.Format(CultureInfo.InvariantCulture, "{0}/message/send?access_token={1}", dingtalkConfig.DingtalkAPIBaseUrl, accessToken);

            return await SendDTMessageAsync(msg, uri);
        }

        private async Task<Dictionary<string, string>> GetConfigAsync(string url)
        {
            string signStrPattern = "jsapi_ticket={0}&noncestr={1}&timestamp={2}&url={3}";
            var corpId = dingtalkConfig.CorpID;
            var agentId = dingtalkConfig.AgentID;
            var nonceStr = dingtalkConfig.NonceString;
            int timeStamp = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            string corpAccessToken = await GetAccessTokenAsync();

            string ticket = await GetTicketAsync(corpAccessToken);
            var strToBeSigned = string.Format(CultureInfo.InvariantCulture, signStrPattern, ticket, nonceStr, timeStamp.ToString(CultureInfo.InvariantCulture), url);
            var signature = Sha1Hash(strToBeSigned);

            Dictionary<string, string> config = new Dictionary<string, string>
            {
                { "url", url },
                { "nonceStr", nonceStr },
                { "agentId", agentId },
                { "timeStamp", timeStamp.ToString(CultureInfo.InvariantCulture) },
                { "corpId", corpId },
                { "signature", signature }
            };

            return config;
        }

        private async Task<JObject> GetDTContentAsync(string uri)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                client.BaseAddress = new Uri(uri);
                client.DefaultRequestHeaders.Accept.Clear();
                var response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    JObject o = JObject.Parse(responseJson);
                    var errCode = o["errcode"].ToString();
                    if (errCode == "0")
                    {
                        return o;
                    }
                }

                return null;
            }
        }

        private async Task<string> PostDTContentAsync(string uri, object postData)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                client.BaseAddress = new Uri(uri);
                client.DefaultRequestHeaders.Accept.Clear();
                var content = new DTJsonHttpContent(postData);
                var response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return responseJson;
                }

                return null;
            }
        }

        private async Task<string> PostTOPContentAsync(IEnumerable<KeyValuePair<string, string>> postData)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                client.BaseAddress = new Uri(dingtalkConfig.TOPAPIUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                logger.LogTrace("Sending HTTP POST to {url} with data {postData}", dingtalkConfig.TOPAPIUrl, JsonConvert.SerializeObject(postData));
                var response = await client.PostAsync(dingtalkConfig.TOPAPIUrl, new FormUrlEncodedContent(postData));
                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    logger.LogTrace("The HTTP POST response content is {res}", responseJson);
                    return responseJson;
                }
                else
                {
                    logger.LogWarning("HTTP POST returned {code} status code with data {data}", response.StatusCode, response.Content.ReadAsStringAsync());
                }

                return null;
            }
        }

        /// <summary>
        /// 调用阿里开放平台API
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="postData">需要传递给服务端的数据，此方法会添加上session, timestamp, format, v四个公共请求参数</param>
        /// <returns>服务端返回的JSON结果，如果返回为null，表示调用没有成功</returns>
        private async Task<JObject> InvokeTOPAPI(string method, List<KeyValuePair<string, string>> postData)
        {
            AddFormContent(postData, "method", method);
            AddFormContent(postData, "session", await GetAccessTokenAsync());
            AddFormContent(postData, "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            AddFormContent(postData, "format", "json");
            AddFormContent(postData, "v", "2.0");
            var response = await PostTOPContentAsync(postData);
            if (response != null)
            {
                return JObject.Parse(response);
            }

            return null;
        }

        private void AddFormContent(List<KeyValuePair<string, string>> postData, string key, string value)
        {
            if (!postData.Any(kvp => kvp.Key == key))
            {
                postData.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        private void LogTOPError(JObject jsonObj)
        {
            JToken errObj = jsonObj["error_response"];
            string code = errObj["code"].ToString();
            string msg = errObj["msg"].ToString();
            string subCode = errObj["sub_code"].ToString();
            string subMsg = errObj["sub_msg"].ToString();
            logger.LogError(
                "Error getting Dingtalk roles. code: {code}. msg: {msg}. sub_code: {subCode}. sub_msg: {subMsg}.",
                code,
                msg,
                subCode,
                subMsg);
        }

        private string Sha1Hash(string input)
        {
#pragma warning disable CA5350 // Do not use insecure cryptographic algorithm SHA1.
            SHA1 s = SHA1.Create();
#pragma warning restore CA5350 // Do not use insecure cryptographic algorithm SHA1.
            byte[] b = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] c = s.ComputeHash(b);
#pragma warning disable CA1308 // Normalize strings to uppercase
            return BitConverter.ToString(c).Replace("-", string.Empty).ToLower(CultureInfo.InvariantCulture);
#pragma warning restore CA1308 // Normalize strings to uppercase
        }

        private async Task<List<DTUser>> GetAllDDUserInfoAsync(string uri)
        {
            List<DTUser> userInfoList = new List<DTUser>();
            List<DTDepartment> depts = await GetDTDepartmentsAsync();
            depts.ForEach((dept) =>
            {
                Task<List<DTUser>> taskDeptUsers = GetDepartmentDDUserInfo(uri, dept);
                taskDeptUsers.Wait();
                List<DTUser> deptUsers = taskDeptUsers.Result;
                deptUsers.ForEach(du =>
                {
                    if (!userInfoList.Exists(u => u.UserID == du.UserID))
                    {
                        userInfoList.Add(du);
                    }
                });
            });
            return userInfoList;
        }

        private async Task<List<DTUser>> GetDepartmentDDUserInfo(string apiUri, DTDepartment dept)
        {
            List<DTUser> users = new List<DTUser>();

            string uriWithDeptId = string.Format(CultureInfo.InvariantCulture, "{0}{1}", apiUri, dept.Id);
            JObject jsonObj = await GetDTContentAsync(uriWithDeptId);
            if (jsonObj != null)
            {
                users = JsonConvert.DeserializeObject<List<DTUser>>(jsonObj["userlist"].ToString());
            }

            return users;
        }
    }
}