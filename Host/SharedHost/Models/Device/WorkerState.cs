namespace SharedHost.Models.Device
{
   public class WorkerState
    {
        public const string OnSession = "ON_SESSION";

        public const string OffRemote = "OFF_REMOTE";

        public const string Open = "DEVICE_OPEN";

        public const string Disconnected = "DEVICE_DISCONNECTED";

        // registering state, only available in worker manager db
        public const string Registering = "DEVICE_REGISTERING";

        // unregister state, only available in worker manager db
        public const string Unregister = "DEVICE_REGISTERING";
    }
}
