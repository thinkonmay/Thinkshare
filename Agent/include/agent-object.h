#ifndef __AGENT_OBJECT_H__
#define __AGETN_OBJECT_H__
#include <glib.h>


#include <agent-type.h>
#include <agent-device.h>
#include <agent-state.h>
 





/// <summary>
/// start websocket connection with host, 
/// invoke during initialization of agent object
/// run recursively until connection has been established
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
void											agent_connect_to_host				(AgentObject* self);


/// <summary>
/// agent disconnect host connection,
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
void											agent_disconnect_host				(AgentObject* self);


/// <summary>
/// session initialize, invoke when request from server is received,
/// wrap aroun session initialize function from agent-ipc
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
void											agent_session_initialize			(AgentObject* self);


/// <summary>
/// agent session terminate session, 
/// wrap around session_terminate function from agent-ipc
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
void											agent_session_terminate				(AgentObject* self);

/// <summary>
/// agent remote control disconnect function, 
/// wrap around remote control disconnect function
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
void											agent_remote_control_disconnect		(AgentObject* self);

/// <summary>
/// agent remote control reconnect function,
/// wrap around remote control reconnect function
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
void											agent_remote_control_reconnect		(AgentObject* self);




gboolean										agent_command_line_passing			(AgentObject* self,
																					 gchar* command);


/// <summary>
/// agent send message function, wrap around send_messsage function
/// </summary>
/// <param name="self"></param>
/// <param name="message"></param>
/// <returns></returns>
void											agent_send_message					(AgentObject* self,
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
/// send message to arbitrary device in system
/// </summary>
/// <param name="self"></param>
/// <param name="message"></param>
/// <returns></returns>
void											agent_send_message_to_host			(AgentObject* self,
																					gchar* message);
/// <summary>
/// 
/// </summary>
/// <param name="self"></param>
/// <param name="message"></param>
void											agent_send_message_to_session_core	(AgentObject* self,
																					 gchar* message);

/// <summary>
/// 
/// </summary>
/// <param name="self"></param>
/// <param name="message"></param>
void											agent_send_message_to_session_loader(AgentObject* self,
																					 gchar* message);

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

GFile*											agent_get_slave_id					(AgentObject* self);

GFile*											agent_get_device_log				(AgentObject* self);

/// <summary>
/// 
/// </summary>
/// <param name="self"></param>
/// <param name="command"></param>
/// <param name="order"></param>
void											agent_send_command_line				(AgentObject* self,
																					 gchar* command,
																					 gint order);



/*get-set function for agent object*/

IPC*											agent_get_ipc						(AgentObject* self);


Socket*											agent_get_socket					(AgentObject* self);


GFile*											agent_get_session					(AgentObject* self);


AgentState*										agent_get_state						(AgentObject* self);

void											agent_set_state						(AgentObject* self,
																					 AgentState* state);

ChildProcess*									agent_get_child_process				(AgentObject* self, 
																					 gint position);

void											agent_set_child_process				(AgentObject* self,
																				     gint postion,
																					 ChildProcess* process);

GMainLoop*										agent_get_main_loop					(AgentObject* self);


/*END get-set function for agent object*/
#endif // !__AGENT_OBJECT__