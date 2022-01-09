using System.Threading.Tasks;


namespace WorkerManager.Interfaces 
{
    public interface IClusterInfor
    {
        Task<bool> IsPrivate();

        Task<bool> IsSelfHost();
    }
}