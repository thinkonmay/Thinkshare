namespace SharedHost.Models.Command
{
    public class ForwardScript
    {
        public int SlaveID { get; set; }

        public int ProcessID { get; set; }

        public string Script { get; set; }
    }

    public class ShellOutput
    {
        public int SlaveID { get; set; }

        public int ProcessID { get; set; }

        public string Output { get; set; }
    }
}
