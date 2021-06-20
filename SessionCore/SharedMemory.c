#include "SharedMemory.h"


void
link_shared_memory_hub(GObject* object)
{
	SessionCore* self = (GObject*)object;
	IPC* ipc = session_core_get_ipc(self);

	ipc->hub = shared_memory_hub_new_default(ipc->core_id);
	

	shared_memory_hub_link_default_async(ipc->hub,
		ipc->agent_id,
		ipc->block_size,
		ipc->pipe_size,
		NULL,
		on_link_connected,
		NULL);
	return;
}

/// <summary>
/// callback function for link_shared_memory_hub_async function
/// </summary>
/// <param name="object"></param>
/// <param name="res"></param>
/// <param name="user_data"></param>
void
on_link_connected(GObject* object,
	GAsyncResult* res,
	gpointer user_data)
{
	SessionCore* self = (SessionCore*)user_data;

	GError* error = NULL;
	IPC* ipc = session_core_get_ipc(self);

	ipc->link = shared_memory_hub_link_finish(self, res, &error);

	g_signal_connect(ipc->link,"on-message", 
		G_CALLBACK(on_shared_memory_message), self);

	if (error)
	{
		session_core_end("cannot connect to shared memory hub", self, APP_STATE_ERROR);
		g_error_free(error);
	}

	g_object_set_property(self, "core-state", WAITING_SESSION_INFORMATION);
	g_signal_emit(self, "hub-connected", 0);
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
	SessionCore* core)
{
	IPC* ipc = session_core_get_ipc(self);
	WebRTCHub* hub = session_core_get_rtc_hub(self);
	SessionQoE* qoe = session_core_get_qoe(self);

	CoreState state;
	g_object_get_property(self, "core-state", &state);

	Session* session;
	gint* bitrate;


	if (from == ipc->agent_id)
	{
		switch (opcode)
		{
		case SESSION_INFORMATION:
			if (state != WAITING_SESSION_INFORMATION)
			{
				session_core_end("received unknown session information\n", self, APP_STATE_ERROR);
			}

			session = (Session*)data;
			qoe = session->qoe;
			hub->client_offer =		session->client_offer;
			hub->signalling_url =	session->signalling_url;
			hub->stun_server =		session->stun_server;
			hub->disable_ssl =		session->disable_ssl;
			hub->slave_id =			session->SessionSlaveID;

			g_object_set_property(self,"core-state",SESSION_INFORMATION_SETTLED);

			g_signal_emit_by_name(self, "session-ready", 0);
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
	else if (from == ipc->host_id)
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

	g_free(session);
	g_free(bitrate);
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
send_message_through_shared_memory(SessionCore* self,
	Location location,
	Opcode opcode,
	gpointer data)
{
	Message* message = malloc(sizeof(Message));
	IPC* ipc = session_core_get_ipc(self);

	switch(location)
	{
	case CLIENT:
		g_printerr("wrong lane");
		return FALSE;
	case HOST:
		message->opcode = opcode;
		message->data = data;
		message->from = ipc->core_id;
		message->to = ipc->host_id;
		shared_memory_link_send_message(ipc->link, message);
	case LOADER:
		message->opcode = opcode;
		message->data = data;
		message->from = ipc->core_id;
		message->to = ipc->loader_id;
		shared_memory_link_send_message(ipc->link, message);
	case AGENT:
		message->opcode = opcode;
		message->data = data;
		message->from = ipc->core_id;
		message->to = ipc->agent_id;
		shared_memory_link_send_message(ipc->link, message);
	}
	return TRUE; 
}