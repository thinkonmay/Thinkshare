using Newtonsoft.Json;
using SharedHost.Models;
using SlaveManager.Administration;
using SlaveManager.Data;
using SlaveManager.Interfaces;
using SlaveManager.Models;
using SlaveManager.Services;
using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlaveManager.Services
{
    public class WebSocketConnection : IWebSocketConnection
    {
        private readonly ISlaveFactory _slaveFatory;

        private readonly ISlavePool _slavePool;

        private readonly IAdmin _admin;

        private readonly ISlaveConnection _connection;

        private readonly ApplicationDbContext _db;

        public WebSocketConnection (ISlaveFactory slaveFactory,
                                    ISlavePool slavePool,
                                    IAdmin admin,
                                    ISlaveConnection connection,
                                    ApplicationDbContext db)
        {
            _slaveFatory = slaveFactory;
            _slavePool = slavePool;
            _admin = admin;
            _connection = connection;
            _db = db;
        }

        private async Task<bool> UpgradeToSlave(WebSocket ws, DeviceInformation device_information)
        {
            bool accepted;
            int slave_id = 0;
            /*
            if (device_information.ID == 0)
            {
                Random random = new Random();
                accepted = await _admin.AskForNewDevice(device_information);
                var slave = _slaveFatory.CreateSlaveDevice(ws, true);
                slave_id = _slavePool.AddSlaveDevice(slave);
            }
            else
            {*/
            var slave = _slaveFatory.CreateSlaveDevice(ws, true);
            accepted = _slavePool.AddSlaveDeviceWithKey(device_information.ID, slave);

            //Add Slave into database if SlaveID is not found in database
            if (_db.Devices.Where(o => o.Id == device_information.ID).Count() == 0)
            {
                Slave device = new Slave();
                device.CPU = device_information.CPU;
                device.GPU = device_information.GPU;
                device.RAMcapacity = device_information.RAM;
                device.Id = device_information.ID;
                device.OS = device_information.OS;

                _db.Devices.Add(device);
                await _db.SaveChangesAsync();
            }
            /*}*/


            if (accepted)
            {
                Message accept = new Message();
                accept.From = Module.HOST_MODULE;
                accept.To = Module.AGENT_MODULE;
                accept.Opcode = Opcode.SLAVE_ACCEPTED;
                accept.Data = slave_id.ToString();
                await Send(ws ,JsonConvert.SerializeObject(accept));
                return true;
            }
            else
            {
                Message deny = new Message();
                deny.From = Module.AGENT_MODULE;
                deny.To = Module.AGENT_MODULE;
                deny.Opcode = Opcode.DENY_SLAVE;
                deny.Data = null;
                await Send(ws, JsonConvert.SerializeObject(deny));
                return false;
            }
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
                        var message_json = JsonConvert.DeserializeObject<Message>(receivedMessage);
                        if (message_json.Opcode == Opcode.REGISTER_SLAVE)
                        {
                            var result = await UpgradeToSlave(ws, JsonConvert.DeserializeObject<DeviceInformation>(message_json.Data));
                            if (result)
                            {
                                break;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            } while (message.MessageType != WebSocketMessageType.Close);

            await _connection.KeepReceiving(ws);
            return true;
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

        public async Task Send(WebSocket ws, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await ws.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true,
                CancellationToken.None);
        }

        public async Task Close(WebSocket ws)
        {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
    }
}