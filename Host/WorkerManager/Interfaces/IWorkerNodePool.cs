namespace WorkerManager.Interfaces
{
    public interface IWorkerNodePool
    {
        public void Start();

        bool Initialized { get; set; }
    }
}
