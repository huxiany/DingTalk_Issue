namespace EaseSource.Dingtalk.Entity
{
    using Newtonsoft.Json;

    public class DTLinkMessage : DTMessageBase
    {
        public override string MessageType
        {
            get { return "link"; }
        }

        public DTLinkMessageContent Link { get; set; }
    }
}
