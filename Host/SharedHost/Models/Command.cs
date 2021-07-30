namespace SharedHost.Models
{
    public class Command
    {
        public int SlaveID { get; set; }
        public int ProcessID { get; set; }
        public string CommandLine { get; set; }
    }

    public class CommandResult : Command
    {
        public string Output { get; set; }
    }
}
