#include <SharedMemory.h>



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
	gpointer data,
	SessionCore* core)
{
	IPC* ipc = session_core_get_ipc(self);
	WebRTCHub* hub = session_core_get_rtc_hub(self);
	SessionQoE* qoe = session_core_get_qoe(self);

	CoreState state;
	g_object_get_property(self, "core-state", &state);

	Session* session;
	gint* bitrate;

	Message* message = (Message*) data;

	if (message->to = CORE)
	{
		if (message->from == AGENT)
		{
			switch (message->opcode)
			{
			case SESSION_INFORMATION:
				if (state != WAITING_SESSION_INFORMATION)
				{
					session_core_end("received unknown session information\n", self, APP_STATE_ERROR);
				}

				session = (Session*)message->data;
				qoe = session->qoe;
				hub->client_offer = session->client_offer;
				hub->signalling_url = session->signalling_url;
				hub->stun_server = session->stun_server;
				hub->disable_ssl = session->disable_ssl;
				hub->slave_id = session->SessionSlaveID;

				g_object_set_property(self, "core-state", SESSION_INFORMATION_SETTLED);

				g_signal_emit_by_name(self, "session-ready", 0);
			case CLIENT_MESSAGE:
				return;
			}
		}
		else if (message->from == LOADER)
		{
			switch (message->opcode)
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
	else if (message->to == CLIENT)
	{
		GBytes* byte = g_bytes_new(data,sizeof(Message));
		g_signal_emit_by_name(hub->control,"send-data", byte);
	}

}


/// <summary>
/// send message through shared memory,
/// this function should only be used by other function 
/// base on shared-memory library
/// </summary>
/// <param name="self"></param>
/// <param name="location"></param>
/// <param name="opcode"></param>
/// <param name="data"></param>
/// <returns></returns>
gboolean
send_message_through_shared_memory(SessionCore* self,
	Message* message)
{
	gboolean ret;
	IPC* ipc = session_core_get_ipc(self);
	SharedMemoryHubClass* klass = SHARED_MEMORY_HUB_GET_CLASS(ipc->hub);

	switch(message->to)
	{
	case CLIENT:
		g_printerr("wrong lane");
		return FALSE;
	case HOST:
		ret = klass->send_data(ipc->hub,ipc->hub_id[HOST], message);
	case LOADER:
		ret = klass->send_data(ipc->hub,ipc->hub_id[LOADER], message);
	case AGENT:
		ret = klass->send_data(ipc->hub,ipc->hub_id[AGENT], message);
	}
	return ret; 
}