namespace EaseSource.Dingtalk.Entity
{
    public class DTFileMessage : DTMessageBase
    {
        public override string MessageType
        {
            get { return "file"; }
        }

        public DTMediaMessageContent Voice { get; set; }
    }
}
