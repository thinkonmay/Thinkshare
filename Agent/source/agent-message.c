#include <agent-message.h>
#include <glib.h>
#include <glib-types.h>
#include <agent-object.h>
#include <agent-socket.h>
#include <agent-ipc.h>
#include <json-glib.h>

/// <summary>
/// Message template to send to all device in one session
/// </summary>
struct _Message
{
	gint opcode;
	gint from;
	gint to;
	gchar* data;
    gint data_size;
};


/// <summary>
/// SessionQoE (quality of experience) of the remote control session
/// (included QoE controller)
/// </summary>
struct _SessionQoE
{
	gint screen_height;
	gint screen_width;
	gint framerate;

	gint bitrate;
};

void
message_init(Message* message,
            gint from,
            gint to,
            gint opcode,
            gpointer data,
            gint data_size)
{
    message = malloc(sizeof(gint)*4+data_size);
    message->from = from;
    message->to   = to;
    message->opcode=opcode;
    memcpy(&message->data,data,data_size);
    message->data_size = data_size;
}




gboolean
send_message(AgentObject* self,
             Message* message)
{
	switch (message->to)
	{
	case HOST:
	    send_message_to_host(self, 
                            message->from,
                            message->to,
                            message->opcode,
                            message->data);
	case CORE:
		send_message_through_shared_memory(self,CORE, message, sizeof(message));
	case LOADER:
		send_message_through_shared_memory(self, LOADER, message, sizeof(message));
	case CLIENT:
		send_message_through_shared_memory(self, CORE, message, sizeof(message));
	}
}





