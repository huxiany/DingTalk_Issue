namespace EaseSource.Dingtalk.Entity
{
    using Newtonsoft.Json;

    public class DTMediaMessageContent
    {
        [JsonProperty(PropertyName = "media_id")]
        public string MediaId { get; set; } = string.Empty;
    }
}
