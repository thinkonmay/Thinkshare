using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Host.Interfaces;
using Host.Models;
using Host.Services;

namespace Host.Slave
{
    public class Slave
    {
        public Slave(SlaveState _state)
        {
            this.State = _state;
        }


        public SlaveState State { get; set; }

        public Device Device;

        public int Id { get; set; }

        public AgentHub Hub { get; set; }

        public Session session;

        public void ChangeState(SlaveState newstate)
        {
            State = newstate;
        }














        /*state dependent method*/
        public void SessionInitialize(Session session)
        {
            State.SessionInitialize(this, session);
        }

        public void SessionTerminate()
        {
            State.SessionTerminate(this);
        }

        public void RemoteControlDisconnect()
        {
            State.RemoteControlDisconnect(this);
        }

        public void RemoteControlReconnect()
        {
            State.RemoteControlReconnect(this);
        }

        public void SendCommand(string command)
        {
            State.SendCommand(this, command);
        }

        public void UpdateState(DeviceInformation information, DeviceState devicestate)
        {
            State.UpdateState(this,information,devicestate);
        }
    }
}
