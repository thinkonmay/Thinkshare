using System;
using System.Collections.Generic;

namespace MetricCollector.Model
{
    public class RAMmetricModel
    {
        public string Name { get; set; }

        public string WorkingSet { get; set; }
    }

    public class RAMDataModel
    {
        public DateTime Time { get; set; }

        public List<RAMmetricModel> ProcessList { get; set; }
    }
}
