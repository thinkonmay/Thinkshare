using Newtonsoft.Json;
using SharedHost.Models;
using SlaveManager.Interfaces;
using System.Net.WebSockets;
using SlaveManager.Services;
using System.Threading.Tasks;
using SlaveManager.SlaveDevices.SlaveStates;

namespace SlaveManager.SlaveDevices
{
    public interface ISlaveDevice
    {
        public void ChangeState(ISlaveState newstate);

        public string GetSlaveState();

        public Task SendMessage(Message message);

        public Task SessionInitialize(SlaveSession session);

        public Task RemoteControlDisconnect();

        public Task RemoteControlReconnect();

        public Task SendCommand(int order, string command);

        public Task RejectSlave();
    }
    /// <summary>
    /// 
    /// </summary>
    public class SlaveDevice : ISlaveDevice
    {
        public ISlaveState State { get; set; }
        public IConnection connection { get; set; }
        public WebSocket ws { get; set; }
        public SlaveDevice(ISlaveConnection _connection)
        {
            ws = null;
            State = new DeviceDisconnected();
            connection = _connection;
        }


        public void ChangeState(ISlaveState newstate)
        {
            State = newstate;
        }


        /*state dependent method*/
        public async Task SessionInitialize(SlaveSession session)
        {
            await State.SessionInitialize(this, session);
        }

        public async Task SessionTerminate()
        {
            await State.SessionTerminate(this);
        }

        public async Task RemoteControlDisconnect()
        {
            await State.RemoteControlDisconnect(this);
        }

        public async Task RemoteControlReconnect()
        {
            await State.RemoteControlReconnect(this);
        }

        public async Task SendCommand(int order, string command)
        {
            await State.SendCommand(this, order, command);
        }

        public async Task RejectSlave()
        {
            await State.RejectSlave(this);
        }

        public Task SendMessage(Message message)
        {
            return connection.Send(ws, JsonConvert.SerializeObject(message));
        }

        public string GetSlaveState()
        {
            return State.GetSlaveState();
        }
    }
}
