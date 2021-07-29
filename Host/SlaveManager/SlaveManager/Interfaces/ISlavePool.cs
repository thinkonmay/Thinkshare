using SharedHost.Models;
using SlaveManager.SlaveDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlaveManager.Interfaces
{
    public interface ISlavePool
    {

        public bool AddSlaveId(int slaveid);


        public string GetSlaveState(int ID);

        public List<Tuple<int, string>> GetSystemSlaveState();

        //public int AddSlaveDevice(SlaveDevice slave);

        public bool AddSlaveDeviceWithKey(int key, SlaveDevice slave);

        public bool SearchForSlaveID(int slave_id);

        public bool DisconnectSlave(int slaveid);

        public bool RejectSlave(int slaveid);

        public bool SendCommand(int slaveid, int order, string command);

        public bool RemoteControlReconnect(int slaveid);

        public bool RemoteControlDisconnect(int slaveid);

        public bool SessionInitialize(int slaveid, SlaveSession session);

        public bool SessionTerminate(int slaveid);
    }
}
