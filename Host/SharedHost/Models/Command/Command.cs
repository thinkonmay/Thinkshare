using MersenneTwister;

namespace SharedHost.Models.Shell
{
    public class ShellScript
    {
        public ShellScript() { }

        public ShellScript(ScriptModel model, int slaveID)
        {
            SlaveID = slaveID;
            Script = model.Script;
            ID = Randoms.Next();
        }

        public int SlaveID { get; set; }

        public int ModelID { get; set; }

        public int ID { get; set; }

        public string Script { get; set; }
    }

    public class ShellOutput
    {
        public int SlaveID {get;set;}

        public string Script { get; set;}

        public string Output { get; set; }

        public int ID { get; set; }

        public int ModelID { get; set; }
    }
}
