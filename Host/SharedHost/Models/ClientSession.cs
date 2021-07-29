namespace SharedHost.Models
{
    public class ClientSession
    {
        public int SessionClientID { get; set; }

        public string SignallingUrl { get; set; }

        public string StunServer { get; set; }

        public bool ClientOffer { get; set; }

        public QoE QoE { get; set; }
    }
}