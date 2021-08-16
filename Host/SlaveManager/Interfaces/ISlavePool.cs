using SharedHost.Models;
using SlaveManager.SlaveDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlaveManager.Interfaces
{
    public interface ISlavePool
    {
        /// <summary>
        /// Add new slave id to slave queue,
        /// use to add new slave to the system,
        /// slave id after add to slavepool will automatically set to disconnected state
        /// </summary>
        /// <param name="slaveid"></param>
        /// <returns></returns>
        public bool AddSlaveId(int slaveid,SlaveDevice slave);

        /// <summary>
        /// Get SlaveDevice correspond to a ID in slave pool, 
        /// remember that slavedevice class do not contain device information, that information is stored inside database,
        /// usually pair with AddSlaveDeviceWithKey to register slavedevice
        /// </summary>
        /// <param name="ID"></param>
        /// <returns>SlaveDevice instance correspond to given ID</returns>
        public SlaveDevice GetSlaveDevice(int ID);

        /// <summary>
        /// Get slave state of slave with id in slave pool
        /// </summary>
        /// <param name="ID">id of slave device to query state</param>
        /// <returns>the string represent slave state</returns>
        public string GetSlaveState(int ID);

        /// <summary>
        /// Get slave state of all slace device in slave pool
        /// </summary>
        /// <returns>list of tuple, first item contain slaveID, second contain slavee state in form of string</returns>
        public List<Tuple<int, string>> GetSystemSlaveState();

        /// <summary>
        /// replace slave device with specific key,
        /// typically use when slave are registered to slave pool
        /// </summary>
        /// <param name="key"></param>
        /// <param name="slave"></param>
        /// <returns>true if slave was replaced successfully</returns>
        public bool AddSlaveDeviceWithKey(int key, SlaveDevice slave);

        /// <summary>
        /// search for id in slave queue, return true if id is found
        /// </summary>
        /// <param name="slave_id"></param>
        /// <returns>true if slave id was found in slave pool, otherwise return false</returns>
        public bool SearchForSlaveID(int slave_id);


        /// <summary>
        /// Send slave reject signal to slave device without delete slaveid from slavepool
        /// </summary>
        /// <param name="slaveid"></param>
        /// <returns></returns>
        public bool DisconnectSlave(int slaveid);

        /// <summary>
        /// Send reject slave message to agent, then remove slavedevice and slave id from slavepool
        /// </summary>
        /// <param name="slaveid"></param>
        /// <returns></returns>
        public bool RejectSlave(int slaveid);

        /// <summary>
        /// send command to a specific process id at an specific slavedevice
        /// </summary>
        /// <param name="slaveid">SlaveID of slavedevice</param>
        /// <param name="order">processid of commandline processs</param>
        /// <param name="command"></param>
        /// <returns></returns>
        public bool SendCommand(int slaveid, int order, string command);



        public bool RemoteControlReconnect(int slaveid);

        public bool RemoteControlDisconnect(int slaveid);

        public bool SessionInitialize(int slaveid, SlaveSession session);

        public bool SessionTerminate(int slaveid);
    }
}
