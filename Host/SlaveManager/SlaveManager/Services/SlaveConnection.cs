using Newtonsoft.Json;
using SharedHost.Models;
using SlaveManager.Administration;
using SlaveManager.Interfaces;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlaveManager.Services
{
    public class SlaveConnection : ISlaveConnection
    {
        private readonly IAdmin _admin;

        public SlaveConnection(IAdmin admin)
        {
            _admin = admin;
        }

        public async Task<bool> KeepReceiving(WebSocket ws)
        {
            WebSocketReceiveResult message;
            do
            {
                using (var memoryStream = new MemoryStream())
                {
                    message = await ReceiveMessage(ws, memoryStream);
                    if (message.Count > 0)
                    {
                        var receivedMessage = Encoding.UTF8.GetString(memoryStream.ToArray());
                        var json_message = JsonConvert.DeserializeObject<MessageWithID>(receivedMessage);

                        if (json_message != null)
                        {
                            if (json_message.To != Module.HOST_MODULE)
                            {
                                if (json_message.From == Module.AGENT_MODULE)
                                {
                                    switch (json_message.Opcode)
                                    {
                                        case Opcode.COMMAND_LINE_FORWARD:
                                            {
                                                // TODO: Add command output in return message

                                                CommandResult cmd = JsonConvert.DeserializeObject<CommandResult>(json_message.Data);
                                                await _admin.LogSlaveCommandLine(json_message.SlaveID, cmd);
                                                /*send command to admin here*/
                                                break;
                                            }

                                    }
                                }
                                else if (json_message.From == Module.CORE_MODULE)
                                {
                                    switch (json_message.Opcode)
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
            } while (message.MessageType != WebSocketMessageType.Close);

            return false;
        }

        public async Task Send(WebSocket ws, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await ws.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true,
                CancellationToken.None);
        }

        private async Task<WebSocketReceiveResult> ReceiveMessage(WebSocket ws, Stream memoryStream)
        {
            var readBuffer = new ArraySegment<byte>(new byte[4 * 1024]);
            WebSocketReceiveResult result;
            do
            {
                result = await ws.ReceiveAsync(readBuffer, CancellationToken.None);
                await memoryStream.WriteAsync(readBuffer.Array, readBuffer.Offset, result.Count,
                    CancellationToken.None);
            } while (!result.EndOfMessage);

            return result;
        }
    }
}
