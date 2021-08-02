using SharedHost.Models;
using SlaveManager.SlaveDevices;
using SlaveManager.SlaveDevices.SlaveStates;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SlaveManager.Interfaces;

namespace SlaveManager.Services
{



    public class SlavePool : ISlavePool
    {
        ConcurrentDictionary<int, SlaveDevice> SlaveList;

        public SlavePool()
        {
            SlaveList = new ConcurrentDictionary<int, SlaveDevice>();
        }


        public bool AddSlaveId(int slaveid)
        {
            SlaveDevice slave = null;
            var ret = SlaveList.TryAdd(slaveid, slave);
            return ret;
        }

        public bool DisconnectSlave(int slaveid)
        {
            SlaveDevice slave;
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }

            slave.RejectSlave();
            var disconnected = new DeviceDisconnected();
            slave.ChangeState(disconnected);
            return true;
        }

        public string GetSlaveState(int ID)
        {
            return SlaveList.Where(o => o.Key == ID).FirstOrDefault().Value.GetSlaveState();
        }

        public List<Tuple<int, string>> GetSystemSlaveState()
        {
            var list = new List<Tuple<int, string>>();

            var pair = SlaveList.Where(o => o.Value != null);
            foreach (var i in SlaveList)
            {
                if (i.Value != null)
                {
                    list.Add(new Tuple<int, string>(i.Key, i.Value.GetSlaveState()));
                }
            }
            return list;
        }

        public bool RejectSlave(int slaveid)
        {
            SlaveDevice slave;
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }

            slave.RejectSlave();
            SlaveList.TryRemove(SlaveList.First(item => item.Key == slaveid));

            var disconnected = new DeviceDisconnected();
            slave.ChangeState(disconnected);
            return true;
        }

        public bool RemoteControlDisconnect(int slaveid)
        {
            SlaveDevice slave;
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }
            if (this.GetSlaveState(slaveid) != "On Session") { return false; }

            slave.RemoteControlDisconnect();
            return true;
        }

        public bool RemoteControlReconnect(int slaveid)
        {
            SlaveDevice slave;
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }
            if (this.GetSlaveState(slaveid) != "Off Remote") { return false; }
            slave.RemoteControlReconnect();
            return true;
        }

        public bool SendCommand(int slaveid, int order, string command)
        {
            SlaveDevice slave;
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }

            slave.SendCommand(order, command);
            return true;
        }

        public bool SessionInitialize(int slaveid, SlaveSession session)
        {
            SlaveDevice slave;
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }

            if (slave.GetSlaveState() != "Device Open") { return false; }

            slave.SessionInitialize(session);
            return true;
        }

        public bool SessionTerminate(int slaveid)
        {
            SlaveDevice slave;
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }

            slave.SessionTerminate();
            return true;
        }

        /*
        public int AddSlaveDevice(SlaveDevice slave)
        {
            int slave_id;
            do
            {
                var generator = new Random();
                slave_id = generator.Next(1,1000);
            } while (SlaveList.Where(o => o.Key == slave_id) == null);
            SlaveList.TryAdd(slave_id, slave);
            return slave_id;
        }*/

        public bool AddSlaveDeviceWithKey(int key, SlaveDevice slave)
        {
            return SlaveList.TryUpdate(key, slave, null);
        }

        public bool SearchForSlaveID(int slave_id)
        {
            SlaveDevice slave;

            if (SlaveList.TryGetValue(slave_id, out slave))
            {
                if (slave == null)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
