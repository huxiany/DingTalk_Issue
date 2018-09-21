namespace EaseSource.Dingtalk.Entity
{
    using Newtonsoft.Json;

    public class DTMarkDownMessageContent
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; } = string.Empty;
    }
}
