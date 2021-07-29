namespace SharedHost.Models
{
    public class Command
    {
        public int Order { get; set; }
        public string CommandLine { get; set; }
    }

    public class CommandResult : Command
    {
        public string Output { get; set; }
    }
}
