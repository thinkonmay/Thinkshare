using SharedHost.Models;
using SlaveManager.SlaveStates;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlaveManager.Services
{
    public interface ISlavePool
    {

        public bool AddSlaveId(int slaveid);

        public List<Tuple<int, string>> GetSystemSlaveState();

        public int AddSlaveDevice(SlaveDevice slave);

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


    public class SlavePool : ISlavePool
    {
        ConcurrentDictionary<int, SlaveDevice> SlaveList;

        private readonly SlaveSession defaultSession;

        public SlavePool ()
        {
            SlaveList = new ConcurrentDictionary<int, SlaveDevice>();


            defaultSession = new SlaveSession();
            defaultSession.SessionSlaveID = 2002;
            defaultSession.SignallingUrl = "";
            defaultSession.ClientOffer = false;
            defaultSession.StunServer = "https://stun.l.google.com:19302";

            defaultSession.QoE = new QoE();
            defaultSession.QoE.AudioCodec = Codec.CODEC_NVH264;
            defaultSession.QoE.VideoCodec = Codec.CODEC_OPUS_ENC;
            defaultSession.QoE.ScreenHeight = 1440;
            defaultSession.QoE.ScreenWidth = 2560;
            defaultSession.QoE.Framerate = 60;
            defaultSession.QoE.Bitrate = 1000000;
            defaultSession.QoE.QoEMode = QoEMode.DEFAULT;
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

        public List<Tuple<int, string>> GetSystemSlaveState()
        {
            var list = new List<Tuple<int, string>>();

            var pair = SlaveList.Where(o => o.Value != null);
            foreach(var i in SlaveList)
            {
                if (i.Value != null)
                {
                    list.Add(new Tuple<int, string>(i.Key, i.Value.GetSlaveState()));
                }
            }
            list.Add(new Tuple<int, string>(0, "mock"));
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

            slave.RemoteControlDisconnect();
            return true;
        }

        public bool RemoteControlReconnect(int slaveid)
        {
            SlaveDevice slave;
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }

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

            if(session == null)
            {
                if(slave == null)
                {
                    return false;
                }
                slave.SessionInitialize(defaultSession);
            }
            else
            {
                slave.SessionInitialize(session);
            }
            return true;
        }

        public bool SessionTerminate(int slaveid)
        {
            SlaveDevice slave;
            if (!SlaveList.TryGetValue(slaveid, out slave)) { return false; }

            slave.SessionTerminate();
            return true;
        }

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
        }

        public bool AddSlaveDeviceWithKey(int key,SlaveDevice slave)
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
