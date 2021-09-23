using SharedHost.Models;
using SlaveManager.SlaveDevices;
using SlaveManager.SlaveDevices.SlaveStates;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SlaveManager.Interfaces;
using SharedHost.Models.Session;
using SharedHost.Models.Command;
using SharedHost.Models.Device;
using System.Threading.Tasks;
using SharedHost;

namespace SlaveManager.Services
{
    public class SlavePool : ISlavePool
    {
        ConcurrentDictionary<int, SlaveDevice> SlaveList;

        private readonly SystemConfig _config;


        public SlavePool(SystemConfig config)
        {
            _config = config;
            SlaveList = new ConcurrentDictionary<int, SlaveDevice>();
        }



        public SlaveDevice GetSlaveDevice(int ID)
        {
            if (!SearchForSlaveID(ID)) { return null; }
            return SlaveList.Where(o => o.Key == ID).FirstOrDefault().Value;
        }

        public string GetSlaveState(int ID)
        {
            if (!SearchForSlaveID(ID)) { return "SlaveNotFound"; }
            return SlaveList.Where(o => o.Key == ID).FirstOrDefault().Value.GetSlaveState();
        }



        public bool SearchForSlaveID(int slave_id)
        {
            SlaveDevice slave;
            return SlaveList.TryGetValue(slave_id, out slave);
        }

        public List<SlaveQueryResult> GetSystemSlaveState()
        {
            var list = new List<SlaveQueryResult>();

            foreach (var i in SlaveList)
            { 
                list.Add(new SlaveQueryResult() { 
                    SlaveID = i.Key,
                    SlaveServiceState = i.Value.GetSlaveState()
                });                
            }
            return list;
        }

        public bool RejectSlave(int slaveid)
        {
            SlaveDevice slave;
            if (!SearchForSlaveID(slaveid)) { return false; }
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }

            Task.Run(() => slave.RejectSlave());
            SlaveList.TryRemove(SlaveList.First(item => item.Key == slaveid));
            return true;
        }







        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool InitializeCommand(ShellScript script)
        {
            SlaveDevice slave;
            if (!SearchForSlaveID(script.SlaveID)) { return false; }
            if (!SlaveList.TryGetValue(script.SlaveID, out slave)) { return false; }

            Task.Run(() => slave.InitializeShellSession(script));
            return true;
        }












        public bool SessionInitialize(int slaveid, SlaveSession session)
        {
            SlaveDevice slave;
            if (!SearchForSlaveID(slaveid)) { return false; }
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }
            if (!String.Equals(slave.GetSlaveState(),SlaveServiceState.Open)) { return false; }

            Task.Run(() => slave.SessionInitialize(session));
            return true;
        }

        public bool SessionTerminate(int slaveid)
        {
            SlaveDevice slave;
            if (!SearchForSlaveID(slaveid)) { return false; }
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }

            Task.Run(() => slave.SessionTerminate());
            return true;
        }

        public bool RemoteControlDisconnect(int slaveid)
        {
            SlaveDevice slave;
            if (!SearchForSlaveID(slaveid)) { return false; }
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }
            if (GetSlaveState(slaveid) != SlaveServiceState.OnSession) { return false; }

            Task.Run(() => slave.RemoteControlDisconnect());
            return true;
        }

        public bool RemoteControlReconnect(int slaveid)
        {
            SlaveDevice slave;
            if (!SearchForSlaveID(slaveid)) { return false; }
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }
            if (GetSlaveState(slaveid) != SlaveServiceState.OffRemote) { return false; }

            Task.Run(() => slave.RemoteControlReconnect());
            return true;
        }



        public bool AddSlaveId(int SlaveID)
        {
            var slave = new SlaveDevice(_config);
            var ret = SlaveList.TryAdd(SlaveID, slave);
            return ret;
        }

        /// <summary>
        /// Add slaveid, typically called after slaveid are registered to slave pool. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="slave"></param>
        /// <returns></returns>
        public bool AddSlaveDeviceWithKey(int key, SlaveDevice slave)
        {
            return SlaveList.TryUpdate(key, slave, SlaveList.Where(o => o.Key==key).FirstOrDefault().Value);
        }
    }
}
