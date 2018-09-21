namespace EaseSource.Dingtalk.Entity
{
    using Newtonsoft.Json;

    public class DTLinkMessageContent
    {
        public string MessageUrl { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "picUrl")]
        public string PictureUrl { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;
    }
}
