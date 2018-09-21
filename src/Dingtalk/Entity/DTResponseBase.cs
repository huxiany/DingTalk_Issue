namespace EaseSource.Dingtalk.Entity
{
    using Newtonsoft.Json;

    public abstract class DTResponseBase
    {
        [JsonProperty(PropertyName = "errcode")]
        public int ErrorCode { get; set; }

        [JsonProperty(PropertyName = "errmsg")]
        public string ErrorMessage { get; set; }
    }
}
