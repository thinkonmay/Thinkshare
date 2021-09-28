using SharedHost.Models;
using SlaveManager.SlaveDevices;
using SlaveManager.SlaveDevices.SlaveStates;
using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SlaveManager.Interfaces;
using SharedHost.Models.Session;
using SharedHost.Models.Shell;
using SharedHost.Models.Device;
using System.Threading.Tasks;
using SharedHost;

namespace SlaveManager.Services
{
    public class SlavePool : ISlavePool
    {
        ConcurrentDictionary<int, SlaveDevice> SlaveList;

        private readonly SystemConfig _config;

        private readonly IConductorSocket _socket;

        public int SamplePeriod {get;set;}

        public SlavePool(SystemConfig config, IConductorSocket socket)
        {
            SamplePeriod = 60000;
            _config = config;
            _socket = socket;
            SlaveList = new ConcurrentDictionary<int, SlaveDevice>();
            Task.Run(() => SystemHeartBeat());
        }

        public async Task SystemHeartBeat()
        {
            var model_list = await _socket.GetDefaultModel();
            while(true)
            {
                foreach(var i in model_list)
                {
                    BroadcastShellScript(new ShellScript(i,0));
                }
                Thread.Sleep(SamplePeriod);
            }
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
        public bool InitShellSession(ShellScript script)
        {
            SlaveDevice slave;
            if (!SearchForSlaveID(script.SlaveID)) { return false; }
            if (!SlaveList.TryGetValue(script.SlaveID, out slave)) { return false; }

            Task.Run(() => slave.InitializeShellSession(script));
            return true;
        }

        public bool BroadcastShellScript(ShellScript script)
        {
            var slave = GetSystemSlaveState();
            foreach(var item in slave)
            {
                if(item.SlaveServiceState != SlaveServiceState.Disconnected)
                {
                    script.SlaveID = item.SlaveID;
                    InitShellSession(script);
                }
            }
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
