namespace EaseSource.Dingtalk.Entity
{
    public class DTAdmin
    {
        public string UserID { get; set; }

#pragma warning disable CA1707 // Identifiers should not contain underscores
        public ushort Sys_Level { get; set; }
#pragma warning restore CA1707 // Identifiers should not contain underscores
    }
}
