using Newtonsoft.Json;
using SharedHost.Models;
using SharedHost.Models.Device;
using SlaveManager.Interfaces;
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

        private readonly IConductorSocket _conductor;



        public WebSocketConnection(ISlavePool slavePool,
                                    IConductorSocket conductor)
        {
            _slavePool = slavePool;
            _conductor = conductor;
        }

        private async Task<bool> UpgradeToSlave(WebSocket ws, SlaveDeviceInformation device_information)
        {
            SlaveDevice slave;
            //reject request if ID is not found
            if(!_slavePool.SearchForSlaveID(device_information.ID))
            {
                if (await _conductor.ReportSlaveRegistered(device_information))
                {
                    _slavePool.AddSlaveId(device_information.ID);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                await _conductor.ReportSlaveRegistered(device_information);
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
        public async Task KeepReceiving(WebSocket ws)
        {
            int SlaveID = 0;
            try
            {
                do
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        var message = await ReceiveMessage(ws, memoryStream);
                        if (message.Count > 0)
                        {
                            var receivedMessage = Encoding.UTF8.GetString(memoryStream.ToArray());
                            var message_json = JsonConvert.DeserializeObject<Message>(receivedMessage);
                            if (message_json.Opcode == Opcode.REGISTER_SLAVE)
                            {
                                //perform slave registration, break this loop and go to slave connection loop if registration return successfully
                                var registeredInfor =  JsonConvert.DeserializeObject<SlaveDeviceInformation>(message_json.Data);
                                bool result = await UpgradeToSlave(ws, registeredInfor);
                                if(result)
                                {
                                    SlaveID = registeredInfor.ID;
                                    break; 
                                }
                                else
                                { return; }
                            }
                        }
                    }
                } while (ws.State == WebSocketState.Open);
            } catch (Exception ex)
            {
                Serilog.Log.Information("Connection closed due to {reason}.",ex.Message);
                return;
            }
            await HandleSlaveCreation(ws, SlaveID);
        }

        private async Task HandleSlaveCreation(WebSocket ws, int slaveID)
        {
            var slave = _slavePool.GetSlaveDevice(slaveID);
            slave.ws = ws;
            await slave.KeepReceiving(slaveID);
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
            }  catch (Exception)   {  }
        }
    }
}