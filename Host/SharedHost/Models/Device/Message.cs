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
        SESSION_DISCONNECT,
        SESSION_RECONNECT,


        SHELL_SESSION,
        ERROR_REPORT,


        STATE_SYNCING,
        REGISTER_WORKER_NODE,
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
