namespace EaseSource.Dingtalk.Entity
{
    public class DTVoiceMessage : DTMessageBase
    {
        public override string MessageType
        {
            get { return "voice"; }
        }

        public DTVoiceMessageContent Voice { get; set; }
    }
}
