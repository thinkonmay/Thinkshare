using System;
using System.Collections.Generic;

namespace MetricCollector.Model
{
    public class CPUmetricModel
    {
        public string InstanceName { get; set; }

        public int CPUpercentage { get; set; }
    }

    public class CPUDataModel
    {
        public DateTime Time { get; set; }

        public List<CPUmetricModel> ProcessList { get; set; }
    }
}
