using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShellExtractor.Model
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
