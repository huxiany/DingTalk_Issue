namespace EaseSource.Dingtalk.Entity
{
    public class DTTextMessage : DTMessageBase
    {
        public override string MessageType
        {
            get { return "text";  }
        }

        public DTTextMessageContent Text { get; set; }
    }
}
