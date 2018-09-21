namespace EaseSource.Dingtalk.Entity
{
    using System.Net.Http;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class DTJsonHttpContent : StringContent
    {
        public DTJsonHttpContent(object contentObj)
            : base(
            JsonConvert.SerializeObject(
                contentObj,
                new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, ContractResolver = new CamelCasePropertyNamesContractResolver() }),
            Encoding.UTF8,
            "application/json")
        {
        }
    }
}
