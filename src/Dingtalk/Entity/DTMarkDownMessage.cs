namespace EaseSource.Dingtalk.Entity
{
    using Newtonsoft.Json;

    public class DTMarkDownMessage : DTMessageBase
    {
        public override string MessageType
        {
            get { return "markdown"; }
        }

        public DTMarkDownMessageContent Markdown { get; set; }
    }
}
