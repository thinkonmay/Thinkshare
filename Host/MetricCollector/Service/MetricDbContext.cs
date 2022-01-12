using System;
using System.Threading.Tasks;
using InfluxDB.Collector;
using MetricCollector.Model;

namespace MetricCollector.Service
{
    public class MetricDbContext
    {
        public MetricDbContext()
        {
            Metrics.Collector = new CollectorConfiguration()
                .Tag.With("host", Environment.GetEnvironmentVariable("COMPUTERNAME"))
                .Batch.AtInterval(TimeSpan.FromSeconds(2))
                .WriteTo.InfluxDB("http://192.168.99.100:8086", "data")
                .CreateCollector();

        }


        public async Task AddCPUMetric(CPUDataModel model)
        {
            // var cpuTime = new LineProtocolPoint(
            //     "working_set",
            //     new Dictionary<string, object>
            //     {
            //         { "value", process.WorkingSet64 },
            //     },
            //     new Dictionary<string, string>
            //     {
            //         { "host", Environment.GetEnvironmentVariable("COMPUTERNAME") }
            //     },
            //     DateTime.UtcNow);

            // var payload = new LineProtocolPayload();
            // payload.Add(cpuTime);
        }
    }
}
