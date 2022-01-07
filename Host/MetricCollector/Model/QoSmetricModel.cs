namespace MetricCollector.Model
{
    public class QoSMetric
    {
        public int Framerate{get;set;}

        public int VideoLatency {get;set;}

        public int AudioLatency {get;set;}

        public int AudioBitrate {get;set;}

        public int VideoBitrate {get;set;}

        public int SessionSlaveID {get;set;}
    }
}
