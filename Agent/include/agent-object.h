#ifndef __AGENT_OBJECT_H__
#define __AGETN_OBJECT_H__
#include <glib.h>


#include <agent-type.h>
#include <agent-device.h>
 






gboolean										agent_connect_to_host				(AgentObject* self);

gboolean										agent_disconnect_host				(AgentObject* self);

gboolean										agent_session_initialize			(AgentObject* self);

gboolean										agent_session_terminate				(AgentObject* self);

gboolean										agent_remote_control_disconnect		(AgentObject* self);

gboolean										agent_remote_control_reconnect		(AgentObject* self);

gboolean										agent_command_line_passing			(AgentObject* self,
																					 gchar* command);

gboolean										agent_add_local_nas_storage			(AgentObject* self,
																					 LocalStorage* storage);

gboolean										agent_send_message					(AgentObject* self,
																					 Message* message);

/// <summary>
/// Create new agent object based on information of host
/// </summary>
/// <param name="Host_URL"></param>
/// <param name="Host_ID"></param>
/// <returns></returns>
AgentObject*									agent_object_new					(gchar* Host_URL);

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


IPC*											agent_object_get_ipc				(AgentObject* self);


Socket*											agent_object_get_socket				(AgentObject* self);


DeviceInformation*								agent_get_device_information		(AgentObject* self);

DeviceState*									agent_get_device_state				(AgentObject* self);

Session* 										agent_object_get_session			(AgentObject* self);

void											agent_object_set_session			(AgentObject* self, 
																					 Session* session);

void											agent_object_finalize				(AgentObject* object);


AgentState										agent_get_state						(AgentObject* self);

void											agent_set_state						(AgentObject* self,
																					 AgentState state);
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
HANDLE*											agent_get_mutex_handle_ptr				(AgentObject* self);
#endif // !__AGENT_OBJECT__