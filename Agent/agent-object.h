#pragma once
#ifndef __AGENT_OBJECT_H__
#define __AGETN_OBJECT_H__

#include "Framework.h"
#include "Object.h"
 
G_BEGIN_DECLS

#define AGENT_TYPE_OBJECT agent_object_get_type()
/*declare derivable enable agentobject to have virtual method*/
G_DECLARE_DERIVABLE_TYPE (AgentObject, agent_object, AGENT,OBJECT, GObject)


/// <summary>
/// base class for agent object
/// </summary>
struct _AgentObjectClass
{
	GObjectClass parent;

	void
	(*connect_to_host)(AgentObject* self);

	DeviceInformation*
	(*query_device_information)(AgentObject* self);

	gboolean
	(*session_initialization)(AgentObject* self,
		Session* session);

	gboolean
	(*session_termination)(AgentObject* self,
		Session* session);

	gboolean
	(*remote_control_disconnect)(AgentObject* self,
		Session* session);

	gboolean
	(*remote_control_reconnect)(AgentObject* self,
		Session* session);

	gboolean
	(*command_line_passing)(AgentObject* self,
		gchar* command);

	gboolean
	(*add_local_nas_storage)(AgentObject* self, 
		LocalStorage* storage);
	void
	(*send_message)(AgentObject* self,
		Location from,
		Location location,
		Opcode opcode,
		gpointer data);
};

/// <summary>
/// Create new agent object based ovn information of host
/// </summary>
/// <param name="Host_URL"></param>
/// <param name="Host_ID"></param>
/// <returns></returns>
AgentObject*
agent_object_new(gchar* Host_URL,
	gint Host_ID);

/// <summary>
/// handle message from host
/// </summary>
/// <param name="self"></param>
void
handle_host_connection(AgentObject* self);

/// <summary>
/// handle message from session core and session loader
/// </summary>
/// <param name="self"></param>
void
handle_shared_memory_hub(AgentObject* self);

/// <summary>
/// send message to arbitrary device in system
/// </summary>
/// <param name="self"></param>
/// <param name="message"></param>
/// <returns></returns>
gboolean
send_message_to(AgentObject* self,
	Message* message);


IPC*
agent_object_get_ipc(AgentObject* self);

Socket*
agent_object_get_socket(AgentObject* self);


G_END_DECLS
#endif // !__AGENT_OBJECT__