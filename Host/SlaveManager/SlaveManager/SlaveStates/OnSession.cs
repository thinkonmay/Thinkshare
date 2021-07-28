﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedHost.Models;
using SlaveManager.Interfaces;
using SlaveManager.Models;

namespace SlaveManager.SlaveStates
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
            message.Opcode = Opcode.SESSION_TERMINATION;
            message.Data = null;

            await slave.SendMessage(message);

            ISlaveState _state = new DeviceOpen();
            slave.ChangeState(_state);


            return ;
        }

        public async Task RemoteControlDisconnect(ISlaveDevice slave)
        {
            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.DISCONNECT_REMTOE_CONTROL;
            message.Data = null;

            await slave.SendMessage(message);


            return;
        }

        public async Task RemoteControlReconnect(ISlaveDevice slave)
        {
            Console.WriteLine("Already In Remote Control");
            return;
        }

        public async Task SendCommand(ISlaveDevice slave, int order, string command)
        {

            Message message = new Message();

            message.From = Module.HOST_MODULE;
            message.To = Module.AGENT_MODULE;
            message.Opcode = Opcode.COMMAND_LINE_FOWARD;

            Command forward_command = new Command();
            forward_command.Order = order;
            forward_command.CommandLine = command;

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

            await slave.SendMessage(msg);


            return;
        }

        public string GetSlaveState()
        {
            return "On Session";
        }
    }
}
