#include "Framework.h"
#include "Session.h"
#include "Signalling handling.h"
#include "RC config.h"
#include "Handle data channel.h"
#include "Handle pipeline.h"
#include "session-core.h"
#include "Session.h"






struct _SessionCore
{

	GObject* parent_instance;
	
	Pipeline* pipe;

	SessionQoE* qoe;

	Session* session;
	
	WebRTCHub* hub;

	CoreState state;

	IPC* ipc;
};

enum
{
	PROP_CORE_STATE,

	PROP_LAST
};
static GParamSpec* properties[PROP_LAST] = { NULL, };


enum
{
	SIGNAL_SESSION_READY,
	SIGNAL_MEMORY_HUB_CONNETED,
	SIGNAL_ON_MESSAGE,


	SIGNAL_LAST
};

static guint signals[SIGNAL_LAST] = { 0, };


G_DEFINE_TYPE(SessionCore, session_core, G_TYPE_OBJECT)

static void
session_core_class_init(SessionCoreClass* klass)
{
	GObjectClass* object_class = G_OBJECT_GET_CLASS(klass);
	object_class->constructed = session_core_constructed;
	object_class->get_property = session_core_get_property;
	object_class->get_property = session_core_set_property;
	object_class->dispose = session_core_dispose;
	object_class->finalize = session_core_finalize;

	signals[SIGNAL_SESSION_READY] =
		g_signal_new("session-ready",
			G_OBJECT_CLASSTYPE(object_class),
			G_SIGNAL_RUN_FIRST,
			0,
			NULL, NULL, NULL, G_TYPE_NONE, 0);

	signals[SIGNAL_ON_MESSAGE] =
		g_signal_new("on-message",
			G_OBJECT_CLASSTYPE(object_class),
			G_SIGNAL_RUN_FIRST,
			0,
			NULL, NULL, NULL, G_TYPE_NONE, 0);




	properties[PROP_CORE_STATE] =
		g_param_spec_int("core-state",
			"State of session core",
			"current state of session core",
			0, CORE_STATE_LAST, 0,
			G_PARAM_READWRITE);

}
static void
session_core_constructed(GObject* object)
{
	SessionCore* self = (SessionCore*)object;
	SessionCoreClass* klass = SESSION_CORE_GET_CLASS(self);
	/*connect necessary signal*/

	g_signal_connect(self, "session-ready", connect_to_websocket_signalling_server_async, NULL);
	g_signal_connect(self, "on-message", NULL, NULL);
	g_signal_connect(self, "signalling-server-connected", NULL, NULL);
	g_signal_connect(self, "pipeline-ready", NULL, NULL);
	g_signal_connect(self, "handshake-signal-connected", NULL, NULL);
	g_signal_connect(self, "data-channel-connected", NULL, NULL);
	g_signal_connect(self, "pipeline-started", NULL, NULL);

	g_signal_connect(self->ipc->link, "on-message",NULL)
}

static void
session_core_get_property(GObject* object, 
	guint prop_id,
	GValue* value,
	GParamSpec* pspec)
{
	SessionCore* self = SESSION_CORE(object);

	switch (prop_id)
	{
	case PROP_CORE_STATE:
		g_value_set_int(value, self->state);
	}
}

static void
session_core_set_property(GObject* object, guint prop_id,
	const GValue *value, GParamSpec* pspec)
{
	SessionCore* self = SESSION_CORE(object);

	switch (prop_id)
	{
	case PROP_CORE_STATE:
		self->state = g_value_get_int(value);
	}
}



static void
session_core_dispose(GObject* object)
{
	SessionCore* self = (SessionCore*)object;

	if (self->hub->ws)
	{
		if (soup_websocket_connection_get_state(self->hub->ws) ==
			SOUP_WEBSOCKET_STATE_OPEN)
			 soup_websocket_connection_close(self->hub->ws, 1000, "");
		else
			g_object_unref(self->hub->ws);
	}



	gst_element_set_state(GST_ELEMENT(self->pipe->pipeline), GST_STATE_NULL);
	g_print("Pipeline stopped\n");
	gst_object_unref(self->pipe->pipeline);

	g_object_unref(self->pipe);
	g_object_unref(self->session);
	g_object_unref(self->hub);
	g_object_unref(self->session);
	g_object_unref(self->state);
}
static void
session_core_finalize(GObject* object)
{
	g_object_unref(object);
}






void
session_core_link_shared_memory_hub(GObject* object)
{
	SessionCore* self = (GObject*)object;
	self->ipc->hub = shared_memory_hub_new_default(self->ipc->core_id);
		
	shared_memory_hub_link_default_async(self->ipc->hub,
		self->ipc->core_id,
		self->ipc->block_size,
		self->ipc->pipe_size,
		NULL,
		on_link_connected,
		NULL);
	return;
}

static void
on_link_connected(GObject* object,
	GAsyncResult* res,
	gpointer user_data)
{
	SessionCore* self = (SessionCore*)object;

	GError* error = NULL;
	self->ipc->link = shared_memory_hub_link_finish(self, res, &error);

	if (error)
	{
		session_core_end("cannot connect to shared memory hub", self, APP_STATE_ERROR);
		g_error_free(error);
	}

	self->state = WAITING_SESSION_INFORMATION;
	g_signal_emit(self, signals[SIGNAL_MEMORY_HUB_CONNETED], 0);
	return;
}





static void
session_core_send_message(GObject* object,Message* msg)
{
	SessionCore* self = (SessionCore*)object;
	gboolean ret;

	GBytes* bytes;
	switch (msg->destination)
	{
	case CLIENT:
		bytes = g_bytes_new(msg, sizeof(msg));
		g_signal_emit_by_name(self->hub->control, "send-data", bytes);
		ret = TRUE;
	case AGENT:
		ret = shared_memory_link_send_message(self->ipc->link, self->ipc->agent_id, msg,sizeof(msg));
	case LOADER:
		ret = shared_memory_link_send_message(self->ipc->link, self->ipc->loader_id, msg, sizeof(msg));
	}
	return ret;
}


/// <summary>
/// responsible for message from agent and session loader
/// </summary>
/// <param name="self"></param>
/// <param name="msg"></param>
/// <param name="user_data"></param>
void
session_core_on_shared_memory_message(SessionCore* self,
	gint from,
	gint to,
	Message* msg,
	gpointer user_data)
{
	Session* session;
	gint* bitrate;


	if (from == self->ipc->agent_id)
	{
		switch (msg->opcode)
		{
		case SESSION_INFORMATION:
			if (self->state != WAITING_SESSION_INFORMATION)
			{
				session_core_end("received unknown session information\n",self, APP_STATE_ERROR);
			}
			
			session = (Session*)msg->data;
			self->qoe = session->qoe;
			self->hub->client_offer = session->client_offer;
			self->hub->signalling_url = session->signalling_url;
			self->hub->stun_server = session->stun_server;
			self->hub->disable_ssl = session->disable_ssl;
			self->hub->slave_id = session->SlaveID;

			 self->state = SESSION_INFORMATION_SETTLED;

			 g_signal_emit(self, signals[SIGNAL_SESSION_READY], 0);
		case CLIENT_MESSAGE:
			return;
		}
	}
	else if (from == LOADER)
	{
		return;
	}
	else
	{
		g_printerr("unknown message");
	}

	g_free(session);
	g_free(bitrate);
}














SessionCore*
session_core_new(gint id)
{
	return g_object_new(SESSION_TYPE_CORE, "hub-id",id);
}


void
session_core_end(const gchar* msg, SessionCore* core, CoreState state)
{


	if (msg)
		g_printerr("%s\n", msg);

	if (state > 0)
		g_object_set_property(core, "core-state", state);

	g_object_unref(core);
	return;
}







Pipeline*
session_core_get_pipeline(SessionCore* self)
{
	return self->pipe;
}

WebRTCHub*
session_core_get_rtc_hub(SessionCore* self)
{
	return self->hub;
}


SessionQoE*
session_core_get_qoe(SessionCore* self)
{
	return self->qoe;
}


