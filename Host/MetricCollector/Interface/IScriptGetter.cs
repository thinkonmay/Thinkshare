using MetricCollector.Model;
using System.Collections.Generic;

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
