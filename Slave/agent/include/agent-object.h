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
AgentObject*									agent_new							();

/// <summary>
/// handle message from host
/// </summary>
/// <param name="self"></param>
void											handle_host_connection				(AgentObject* self);


void											agent_send_message					(AgentObject* self, Message* message);

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




void											agent_on_session_core_exit		(AgentObject* self);


/*get-set function for agent object*/

Socket*											agent_get_socket					(AgentObject* self);



void											agent_report_error					(AgentObject* self,
				   																	 gchar* message);


AgentState*										agent_get_state						(AgentObject* self);

void											agent_set_state						(AgentObject* self,
																					 AgentState* state);

void											agent_set_main_loop					(AgentObject* agent,
																					 GMainLoop* loop);


gchar*											agent_get_current_state_string		(AgentObject* self);

ChildProcess*									agent_get_child_process				(AgentObject* self, 
																					 gint position);

void											agent_set_child_process				(AgentObject* self,
																				     gint postion,
																					 ChildProcess* process);

GMainLoop*										agent_get_main_loop					(AgentObject* self);


/*END get-set function for agent object*/
#endif // !__AGENT_OBJECT__