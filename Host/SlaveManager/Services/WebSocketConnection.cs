using Newtonsoft.Json;
using SharedHost.Models;
using SlaveManager.Administration;
using SlaveManager.Data;
using SlaveManager.Interfaces;
using SlaveManager.Models;
using SlaveManager.SlaveDevices;
using SlaveManager.SlaveDevices.SlaveStates;
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

        private readonly ISlavePool _slavePool;

        private readonly IAdmin _admin;

        private readonly ApplicationDbContext _db;



        public WebSocketConnection(ISlavePool slavePool,
                                    IAdmin admin,
                                    ApplicationDbContext db)
        {
            _slavePool = slavePool;
            _admin = admin;
            _db = db;
        }

        private async Task<bool> UpgradeToSlave(WebSocket ws, SlaveDeviceInformation device_information)
        {
            SlaveDevice slave;
            //reject request if ID is not found
            if(!_slavePool.SearchForSlaveID(device_information.ID))
            {
                return false;
            }



            //only check slave state after device has been confirmed in db
            if (_slavePool.GetSlaveState(device_information.ID) == SlaveServiceState.Disconnected)
            {
                //if slave in slave pool is in disconnected state,
                //take out slave from pool,
                //change state and push it back to slave pool
                slave = _slavePool.GetSlaveDevice(device_information.ID);
                if(slave == null)
                {
                    return false;
                }
                var state = new DeviceOpen();
                slave.ChangeState(state);
                slave.ws = ws;
                _slavePool.AddSlaveDeviceWithKey(device_information.ID, slave);

                //Add Slave into database if SlaveID is not found in database
                if (_db.Devices.Where(o => o.ID == device_information.ID).Count() == 0)
                {
                    await _admin.ReportSlaveRegistered(device_information);
                }

                Message accept = new Message();
                accept.From = Module.HOST_MODULE;
                accept.To = Module.AGENT_MODULE;
                accept.Opcode = Opcode.SLAVE_ACCEPTED;
                accept.Data = "ACCEPTED";
                await Send(ws, JsonConvert.SerializeObject(accept));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        public async Task<bool> KeepReceiving(WebSocket ws)
        {
            WebSocketReceiveResult message;
            SlaveDeviceInformation registeredInfor = null;
            SlaveDevice slave = null;
            try
            {
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
                                //perform slave registration, break this loop and go to slave connection loop if registration return successfully
                                registeredInfor =  JsonConvert.DeserializeObject<SlaveDeviceInformation>(message_json.Data);
                                bool result = await UpgradeToSlave(ws, registeredInfor);
                                if(result)
                                { break; }
                                else
                                { return false; }
                            }
                        }
                    }
                } while (ws.State == WebSocketState.Open);
            } catch (WebSocketException)
            {
                return true;
            }

            slave = _slavePool.GetSlaveDevice(registeredInfor.ID);
            slave.ws = ws;
            await slave.KeepReceiving();

            //set slave state to disconnected after websocket connection is closed
            slave.ChangeState(new DeviceDisconnected());
            _slavePool.AddSlaveDeviceWithKey(registeredInfor.ID, slave);     
 
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

        /// <summary>
        /// send message to websocket client, most case use to send 
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task Send(WebSocket ws, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await ws.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true,
                CancellationToken.None);
        }


        /// <summary>
        /// close websocket, use both for registration success and fail
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        public async Task Close(WebSocket ws)
        {
            try
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
            catch (WebSocketException)
            {
                return;
            }
        }
    }
}