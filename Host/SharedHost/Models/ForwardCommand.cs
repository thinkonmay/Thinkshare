namespace SharedHost.Models
{
    public class ForwardCommand
    {
        public int SlaveID { get; set; }
        public int ProcessID { get; set; }
        public string CommandLine { get; set; }
    }

    public class ReceiveCommand
    {
        public int Time { get; set; }

        public int ProcessID { get; set; }

        public string Command { get; set; }
    }
}
