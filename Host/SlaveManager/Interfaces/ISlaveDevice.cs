using System.Threading.Tasks;
using SharedHost.Models;
using SharedHost.Models.Device;
using SharedHost.Models.Session;

namespace SlaveManager.Interfaces
{
    /// <summary>
    /// SlaveManager manage many slave device connected with websocket connection using state design pattern,
    /// information about this design pattern can be found in the book:
    /// Design Patterns: Elements of Reusable Object-Oriented Software 
    /// https://www.oreilly.com/library/view/design-patterns-elements/0201633612/
    /// 
    /// </summary>
    public interface ISlaveDevice
    {
        /// <summary>
        /// Change the current state of slave device
        /// </summary>
        /// <param name="newstate"></param>
        void ChangeState(ISlaveState newstate);

        /// <summary>
        /// Notice that the result of this method is a string represent the state of slave device,
        /// the string format can be found at SlaveServiceState class
        /// </summary>
        /// <returns>current State of slave device</returns>
        string GetSlaveState();

        /// <summary>
        /// Send message to agent module, the message will be encoded to json format and send 
        /// through websocket connection
        /// </summary>
        /// <param name="message">Message send to slave device</param>
        /// <returns></returns>
        Task SendMessage(Message message);

        /// <summary>
        /// Initialize session with slave device, the initialize process will be check
        /// for state confliction in both host and agnet, any state conflict will be report to admin
        /// </summary>
        /// <param name="session">slave session contain information about remote control session</param>
        /// <returns></returns>
        Task SessionInitialize(SlaveSession session);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        Task OnRemoteControlDisconnected(int SlaveID);

        /// <summary>
        /// Send disconnect remote control to agent module, after that, session core module will be 
        /// exit without clear the session-slave cached on slave device 
        /// the disconnect process will be check
        /// for state confliction in both host and agnet, any state conflict will be report to admin
        /// </summary>
        /// <returns></returns>
        Task RemoteControlDisconnect();

        /// <summary>
        /// Send reconnect remote control to agent module, after that, session core module will be 
        /// invoke without session-slave cached on slave device 
        /// the reconnect process will be check
        /// for state confliction in both host and agnet, any state conflict will be report to admin
        /// </summary>
        /// <returns></returns>
        Task RemoteControlReconnect();

        /// <summary>
        /// Create new commandline Session, new commandline will run under administrator privillege
        /// after initialize new commandline session, admin will be able to remote control slave 
        /// (similiar to ssh)
        /// </summary>
        /// <param name="order">order(process-id) of commandline process to initialize</param>
        /// <returns></returns>
        Task InitializeShellSession(int order);

        /// <summary>
        /// Send reject signal to slave device, 
        /// this signal will invoke agent_finalize and terminate agent process
        /// </summary>
        /// <returns></returns>
        Task RejectSlave();
    }
    
}