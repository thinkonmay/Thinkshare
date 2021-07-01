#include <agent-ipc.h>
#include <agent-socket.h>
#include <agent-type.h>
#include <agent-object.h>

















/// <summary>
/// contain information about shared memory hub and connection
/// </summary>
struct _IPC
{
	SharedMemoryHub* hub;

	gint hub_id[LOCATION_MAX];
};

/// <summary>
/// responsible for message from agent and session loader.
/// attached with "on-message" signal of SharedMemoryLink object,
/// refer to on_link_connected callback function
/// </summary>
/// <param name="self"></param>
/// <param name="msg"></param>
/// <param name="user_data"></param>
void
on_shared_memory_message(SharedMemoryLink* self,
						gint from,
						gint to,
						gint opcode,
						gpointer data,
						AgentObject* agent)
{
	IPC* ipc = agent_object_get_ipc(agent);
	Socket* socket = agent_object_get_socket(agent);

	if (from == LOADER)
	{
		switch (opcode)
		{
		default:
			break;
		}
	}
	else if (from == CLIENT)
	{
		switch (opcode)
		{
		default:
			break;
		}
	}
	else
	{
		g_printerr("unknown message");
	}
}

/// <summary>
/// send message through shared memory,
/// this function should only be used by other function 
/// </summary>
/// <param name="self"></param>
/// <param name="location"></param>
/// <param name="opcode"></param>
/// <param name="data"></param>
/// <returns></returns>
gboolean
send_message_through_shared_memory(AgentObject* object,
                                   gint destination,
	                               Message* message,
                                   gint message_size)
{
	gboolean ret;
    IPC* ipc = agent_object_get_ipc(object);
	SharedMemoryHubClass* klass = SHARED_MEMORY_HUB_GET_CLASS(ipc->hub);

	ret = klass->send_data(ipc->hub,ipc->hub_id[destination], message,message_size);
	
	return ret;
}




gboolean
session_terminate(AgentObject* object)
{
	
}

gboolean
session_initialize(AgentObject* object)
{

}

gboolean
remote_control_reconnect(AgentObject* object)
{

	return TRUE;
}

gboolean
remote_control_disconnect(AgentObject* object)
{

}
