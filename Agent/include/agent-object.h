#ifndef __AGENT_OBJECT_H__
#define __AGETN_OBJECT_H__
#include <glib.h>


#include <agent-type.h>
#include <agent-device.h>
 





/// <summary>
/// start websocket connection with host, 
/// invoke during initialization of agent object
/// run recursively until connection has been established
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
gboolean										agent_connect_to_host				(AgentObject* self);


/// <summary>
/// agent disconnect host connection,
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
gboolean										agent_disconnect_host				(AgentObject* self);


/// <summary>
/// session initialize, invoke when request from server is received,
/// wrap aroun session initialize function from agent-ipc
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
gboolean										agent_session_initialize			(AgentObject* self);


/// <summary>
/// agent session terminate session, 
/// wrap around session_terminate function from agent-ipc
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
gboolean										agent_session_terminate				(AgentObject* self);

/// <summary>
/// agent remote control disconnect function, 
/// wrap around remote control disconnect function
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
gboolean										agent_remote_control_disconnect		(AgentObject* self);

/// <summary>
/// agent remote control reconnect function,
/// wrap around remote control reconnect function
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
gboolean										agent_remote_control_reconnect		(AgentObject* self);




gboolean										agent_command_line_passing			(AgentObject* self,
																					 gchar* command);

gboolean										agent_add_local_nas_storage			(AgentObject* self,
																					 LocalStorage* storage);


/// <summary>
/// agent send message function, wrap around send_messsage function
/// </summary>
/// <param name="self"></param>
/// <param name="message"></param>
/// <returns></returns>
gboolean										agent_send_message					(AgentObject* self,
																					 Message* message);


/// <summary>
/// agent_finalize, end main loop and all related thread, 
/// close websocket connection and close agent process
/// </summary>
/// <param name="object"></param>
void											agent_finalize						(AgentObject* object);
/// <summary>
/// Create new agent object based on information of host
/// </summary>
/// <param name="Host_URL"></param>
/// <param name="Host_ID"></param>
/// <returns></returns>
AgentObject*									agent_new							(gchar* Host_URL);

/// <summary>
/// handle message from host
/// </summary>
/// <param name="self"></param>
void											handle_host_connection				(AgentObject* self);

/// <summary>
/// handle message from session core and session loader
/// </summary>
/// <param name="self"></param>
void											handle_shared_memory_hub			(AgentObject* self);

/// <summary>
/// send message to arbitrary device in system
/// </summary>
/// <param name="self"></param>
/// <param name="message"></param>
/// <returns></returns>
gboolean										send_message						(AgentObject* self,
																					Message* message);



/// <summary>
/// register slave device with host by sending SLAVE_REGISTER message,
/// only invoke when agent in attemp to reconnect state
/// </summary>
/// <param name="self"></param>
void											agent_register_with_host			(AgentObject* self);

/// <summary>
/// settle device information update thread and other stuff,
/// only invoked when SLAVE_ACCEPT message is received,
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
gboolean										agent_register_settled				(AgentObject* self);

/// <summary>
/// get mutex for device state(shared resource)
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
HANDLE*											agent_get_mutex_handle_ptr			(AgentObject* self);



/*get-set function for agent obnject*/

IPC*											agent_get_ipc						(AgentObject* self);


Socket*											agent_get_socket					(AgentObject* self);


DeviceInformation*								agent_get_device_information		(AgentObject* self);

DeviceState*									agent_get_device_state				(AgentObject* self);

Session*										agent_get_session					(AgentObject* self);

void											agent_set_session					(AgentObject* self,
																					 Session* session);


AgentState										agent_get_state						(AgentObject* self);

void											agent_set_state						(AgentObject* self,
																					 AgentState state);



/*END get-set function for agent object*/
#endif // !__AGENT_OBJECT__