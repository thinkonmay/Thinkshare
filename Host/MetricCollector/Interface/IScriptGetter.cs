using MetricCollector.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetricCollector.Interface
{
    public interface IScriptGetter
    {
        public List<CPUDataModel> GetCPU(int DeviceID);

        public List<GPUDataModel> GetGPU(int DeviceID);

        public List<RAMDataModel> GetRAM(int DeviceID);

        public List<StorageDataModel> GetStorage(int DeviceID);
        
    }
}
