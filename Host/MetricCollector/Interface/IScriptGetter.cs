using MetricCollector.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetricCollector.Interface
{
    public interface IScriptGetter
    {
        Task<List<CPUDataModel>> GetCPU(int DeviceID);

        Task<List<GPUDataModel>> GetGPU(int DeviceID);

        Task<List<RAMDataModel>> GetRAM(int DeviceID);

        Task<List<StorageDataModel>> GetStorage(int DeviceID);
    }
}
