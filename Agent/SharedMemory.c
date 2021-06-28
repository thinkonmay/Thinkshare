<<<<<<< Updated upstream
=======
#include "SharedMemory.h"

/// <summary>
/// declare array of function pointer, reduce complexity of the code
/// </summary>
void
(*on_link_connected[2])(GObject* object,
	GAsyncResult* res,
	gpointer user_data);

void
link_shared_memory_hub(GObject* object)
{
	AgentObject* self = (GObject*)object;
	IPC* ipc = agent_object_get_ipc(self);
	gint max_link = 2;

	ipc->hub = shared_memory_hub_new(ipc->agent_id,max_link,TRUE);

	on_link_connected[0] = on_link_core_connected;
	on_link_connected[1] = on_link_loader_connected;

	for (int i = 0; i < max_link; i++)
	{
		shared_memory_hub_link_default_async(ipc->hub,
			ipc->core_id,
			ipc->block_size,
			ipc->pipe_size,
			NULL,
			on_link_connected[i],
			NULL);
		return;
	}
	g_free(max_link);
}

/// <summary>
/// callback function for link_shared_memory_hub_async function
/// </summary>
/// <param name="object"></param>
/// <param name="res"></param>
/// <param name="user_data"></param>
void
on_link_core_connected(GObject* object,
	GAsyncResult* res,
	gpointer user_data)
{
	AgentObject* self = (AgentObject*)user_data;

	GError* error = NULL;
	IPC* ipc = agent_object_get_ipc(self);

	ipc->core_link = shared_memory_hub_link_finish(self, res, &error);

	g_signal_connect(ipc->core_link, "on-message",
		G_CALLBACK(on_shared_memory_message), self);

	if (error)
	{
		g_error_free(error);
	}

	return;
}


void
on_link_loader_connected(GObject* object,
	GAsyncResult* res,
	gpointer user_data)
{
	AgentObject* self = (AgentObject*)user_data;

	GError* error = NULL;
	IPC* ipc = agent_object_get_ipc(self);

	ipc->loader_link = shared_memory_hub_link_finish(self, res, &error);

	g_signal_connect(ipc->loader_link, "on-message",
		G_CALLBACK(on_shared_memory_message), self);

	if (error)
	{
		g_error_free(error);
	}

	return;
}





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
	gpointer* data,
	AgentObject* agent)
{
	IPC* ipc = agent_object_get_ipc(agent);
	Socket* socket = agent_object_get_socket(agent);

	AgentState state;
	g_object_get_property(self, "agent-state", &state);

	if (from == ipc->core_id)
	{
		switch (opcode)
		{
		case CLIENT_MESSAGE:
			return;
		}
	}
	else if (from == ipc->loader_id)
	{
		switch (opcode)
		{
		default:
			break;
		}
	}
	else if (from == ipc->client_id)
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
send_message_through_shared_memory(AgentObject* self,
	Message* message)
{
	gboolean ret;

	IPC* ipc = agent_object_get_ipc(self);

	switch (message->to)
	{
	case HOST:
		g_printerr("wrong lane");
		return FALSE;
	case LOADER:
		ret = shared_memory_link_send_message(ipc->loader_link,ipc->loader_id, message);
	case CORE:
		ret = shared_memory_link_send_message(ipc->core_link,ipc->core_id, message);
	case CLIENT:
		ret = shared_memory_link_send_message(ipc->core_link, ipc->core_id, message);
	}
	return ret;
}
>>>>>>> Stashed changes
