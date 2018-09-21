namespace EaseSource.Dingtalk.Entity
{
    using Newtonsoft.Json;

    public class DTSendMessageResponse : DTResponseBase
    {
        [JsonProperty(PropertyName = "invaliduser")]
        public string InvalidUsers { get; set; }

        [JsonProperty(PropertyName = "invalidparty")]
        public string InvalidParties { get; set; }

        [JsonProperty(PropertyName = "forbiddenUserId")]
        public string ForbiddenUserIds { get; set; }

        [JsonProperty(PropertyName = "messageId")]
        public string MessageId { get; set; }
    }
}
