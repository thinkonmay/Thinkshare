using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Host.Models;


namespace Host.Interfaces
{
    public interface IAgentHub
    {
        Task Handle();

        Task SendMessage(Message message);
    }
}
 