using SharedHost.Models;
using SlaveManager.SlaveDevices;
using SlaveManager.SlaveDevices.SlaveStates;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SlaveManager.Interfaces;
using SlaveManager.Data;

namespace SlaveManager.Services
{
    public class SlavePool : ISlavePool
    {
        ConcurrentDictionary<int, SlaveDevice> SlaveList;

        /// <summary>
        /// use to inject connection to slave device
        /// </summary>
        private readonly ISlaveConnection _connection;

        public SlavePool(ISlaveConnection connection)
        {
            SlaveList = new ConcurrentDictionary<int, SlaveDevice>();

            _connection = connection;
        }



        public bool DisconnectSlave(int slaveid)
        {
            SlaveDevice slave;
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }

            slave.RejectSlave();
            var disconnected = new DeviceDisconnected();
            slave.ChangeState(disconnected);
            AddSlaveDeviceWithKey(slaveid,slave); 
            return true;
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

        public List<Tuple<int, string>> GetSystemSlaveState()
        {
            var list = new List<Tuple<int, string>>();

            var pair = SlaveList.Where(o => o.Value != null);
            foreach (var i in pair)
            { 
                list.Add(new Tuple<int, string>(i.Key, i.Value.GetSlaveState()));                
            }
            return list;
        }

        public bool RejectSlave(int slaveid)
        {
            SlaveDevice slave;
            if (!SearchForSlaveID(slaveid)) { return false; }
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }

            slave.RejectSlave();
            SlaveList.TryRemove(SlaveList.First(item => item.Key == slaveid));
            return true;
        }




        public bool RemoteControlDisconnect(int slaveid)
        {
            SlaveDevice slave;
            if (!SearchForSlaveID(slaveid)) { return false; }
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }
            if (this.GetSlaveState(slaveid) != SlaveServiceState.OnSession) { return false; }

            slave.RemoteControlDisconnect();
            return true;
        }

        public bool RemoteControlReconnect(int slaveid)
        {
            SlaveDevice slave;
            if (!SearchForSlaveID(slaveid)) { return false; }
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }
            if (this.GetSlaveState(slaveid) != SlaveServiceState.OffRemote) { return false; }
            slave.RemoteControlReconnect();
            return true;
        }

        public bool SendCommand(int slaveid, int order, string command)
        {
            SlaveDevice slave;
            if (!SearchForSlaveID(slaveid)) { return false; }
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }

            slave.SendCommand(order, command);
            return true;
        }

        public bool SessionInitialize(int slaveid, SlaveSession session)
        {
            SlaveDevice slave;
            if (!SearchForSlaveID(slaveid)) { return false; }
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }
            if (!String.Equals(slave.GetSlaveState(),SlaveServiceState.Open)) { return false; }

            slave.SessionInitialize(session);
            return true;
        }

        public bool SessionTerminate(int slaveid)
        {
            SlaveDevice slave;
            if (!SearchForSlaveID(slaveid)) { return false; }
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }

            slave.SessionTerminate();
            return true;
        }



        public bool AddSlaveId(int slaveid)
        {
            SlaveDevice slave = new SlaveDevice(_connection);
            var ret = SlaveList.TryAdd(slaveid, slave);
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
