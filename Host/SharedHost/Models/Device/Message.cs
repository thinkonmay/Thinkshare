namespace SharedHost.Models.Device
{
    public class Message
    {
        public Module From { get; set; }

        public Module To { get; set; }

        public Opcode Opcode { get; set; }

        public string? Data { get; set; }

        public int? WorkerID { get; set; }

        public string? token {get;set;}
    }

    public enum Opcode
    {
        SESSION_INITIALIZE,
        SESSION_TERMINATE,
        RECONNECT_REMOTE_CONTROL,
        DISCONNECT_REMOTE_CONTROL,

        ERROR_REPORT,
        SHELL_SESSION,
        STATE_SYNCING,
        REGISTER_NEW_WORKER,
        ID_GRANT
    }

    public enum Module
    {        
        CORE_MODULE,
        CLIENT_MODULE,
        LOADER_MODULE,
        AGENT_MODULE,
        HOST_MODULE,
        CLUSTER_MODULE,
    }
}
