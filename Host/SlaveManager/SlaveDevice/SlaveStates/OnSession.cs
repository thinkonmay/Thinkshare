﻿using Newtonsoft.Json;
using SharedHost.Models.Command;
using SharedHost.Models;
using SharedHost.Models.Device;
using SharedHost.Models.Session;
using SlaveManager.Interfaces;
using System;
using System.Threading.Tasks;

namespace SlaveManager.SlaveDevices.SlaveStates
{
    public class OnSession : ISlaveState
    {
        public async Task SessionInitialize(ISlaveDevice slave, SlaveSession session)
        {
            Console.WriteLine("Already In Session");
            return;
        }

        public async Task SessionTerminate(ISlaveDevice slave)
        {
            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.SESSION_TERMINATE;
            message.Data = " ";

            await slave.SendMessage(message);

            var _state = new DeviceOpen();
            slave.ChangeState(_state);


            return;
        }

        public async Task RemoteControlDisconnect(ISlaveDevice slave)
        {
            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.DISCONNECT_REMOTE_CONTROL;
            message.Data = " ";

            slave.ChangeState(new OnSessionOffRemote());
            await slave.SendMessage(message);


            return;
        }

        public async Task RemoteControlReconnect(ISlaveDevice slave)
        {
            return;
        }



        public async Task InitializeShellSession(ISlaveDevice slave, int order)
        {
            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.NEW_SHELL_SESSION;

            ForwardScript forward_command = new ForwardScript();
            forward_command.ProcessID = order;
            forward_command.CommandLine = " ";

            message.Data = JsonConvert.SerializeObject(forward_command);
            await slave.SendMessage(message);
            return;
        }

        public async Task RejectSlave(ISlaveDevice slave)
        {
            Message msg = new Message();
            msg.From = Module.HOST_MODULE;
            msg.To = Module.AGENT_MODULE;
            msg.Opcode = Opcode.REJECT_SLAVE;
            msg.Data = " ";

            await slave.SendMessage(msg);
            ISlaveState _state = new DeviceDisconnected();
            slave.ChangeState(_state);

            return;
        }

        public string GetSlaveState()
        {
            return SlaveServiceState.OnSession;
        }

        public async Task OnSessionCoreExit(ISlaveDevice slave, int SlaveID)
        {
            await slave.OnRemoteControlDisconnected(SlaveID);
        }
    }
}
