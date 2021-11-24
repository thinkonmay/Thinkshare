namespace SharedHost.Models.Session
{
    public class SessionMetric
    {
        public int FrameRate {get;set;}
        public int AudioLatency {get;set;}
        public int VideoLatency {get;set;}
        public int AudioBitrate {get;set;}
        public int VideoBitrate {get;set;}
        public int TotalBandwidth {get;set;}
        public int PacketsLost {get;set;}
    }
    
}