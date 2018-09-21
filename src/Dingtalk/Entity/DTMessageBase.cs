namespace EaseSource.Dingtalk.Entity
{
    using Newtonsoft.Json;

    public abstract class DTMessageBase
    {
        [JsonProperty(PropertyName = "touser")]
        public string ToUsers { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "toparty")]
        public string ToParties { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "agentid")]
        public string AgentId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "code")]
        public string AuthCode { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "msgtype")]
        public virtual string MessageType { get; } = string.Empty;
    }
}
