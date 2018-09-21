namespace EaseSource.Dingtalk.Entity
{
    public class DTImageMessage : DTMessageBase
    {
        public override string MessageType
        {
            get { return "image"; }
        }

        public DTMediaMessageContent Image { get; set; }
    }
}
