#include "SignallingHandling.h"
#include "RcConfig.h"
#include "HandleDataChannel.h"
#include "HandlePipeline.h"
#include "session-core.h"
#include "SharedMemory.h"





typedef struct 
{

	GObject* parent_instance;
	
	Pipeline* pipe;

	SessionQoE* qoe;

	Session* session;
	
	WebRTCHub* hub;

	CoreState state;

	IPC* ipc;
}SessionCorePrivate;

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

/*define type with private struct*/
G_DEFINE_TYPE_WITH_PRIVATE(SessionCore, session_core, G_TYPE_OBJECT)

static void
session_core_class_init(SessionCoreClass* klass)
{

	/*Override gobject base class virtual method and define session core virtual method*/
	GObjectClass* object_class = G_OBJECT_GET_CLASS(klass);
	object_class->constructed =		session_core_constructed;
	object_class->get_property =	session_core_get_property;
	object_class->get_property =	session_core_set_property;
	object_class->dispose =			session_core_dispose;
	object_class->finalize =		session_core_finalize;

	klass->connect_shared_memory_hub =	link_shared_memory_hub;
	klass->connect_signalling_server =	connect_to_websocket_signalling_server_async;
	klass->setup_pipeline =				setup_pipeline;
	klass->setup_webrtc_signalling =	connect_WebRTCHub_handler;
	klass->setup_data_channel =			connect_data_channel_signals;
	klass->start_pipeline =				start_pipeline;
	klass->stop_pipeline =				stop_pipeline;


	/*signal registering
	*/

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
	


	/*properties registering
	*/
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
	SessionCorePrivate* priv = session_core_get_instance_private(self);

	SessionCoreClass* klass = SESSION_CORE_GET_CLASS(self);
	/*connect necessary signal*/

	g_signal_connect(priv, "session-ready", NULL, NULL);
	g_signal_connect(priv, "on-message", NULL, NULL);
	g_signal_connect(priv, "signalling-server-connected", NULL, NULL);
	g_signal_connect(priv, "pipeline-ready", NULL, NULL);
	g_signal_connect(priv, "handshake-signal-connected", NULL, NULL);
	g_signal_connect(priv, "data-channel-connected", NULL, NULL);
	g_signal_connect(priv, "pipeline-started", NULL, NULL);
	g_signal_connect(priv->ipc->link, "on-message",NULL,NULL,NULL, NULL);
}

static void
session_core_get_property(GObject* object, 
	guint prop_id,
	GValue* value,
	GParamSpec* pspec)
{
	SessionCore* self = SESSION_CORE(object);
	SessionCorePrivate* priv = session_core_get_instance_private(self);
	switch (prop_id)
	{
	case PROP_CORE_STATE:
		g_value_set_int(value, priv->state);
	}
}

static void
session_core_set_property(GObject* object, guint prop_id,
	const GValue *value, GParamSpec* pspec)
{
	SessionCore* self = SESSION_CORE(object);
	SessionCorePrivate* priv = session_core_get_instance_private(self);

	switch (prop_id)
	{
	case PROP_CORE_STATE:
		priv->state = g_value_get_int(value);
	}
}



static void
session_core_dispose(GObject* object)
{
	SessionCore* self = (SessionCore*)object;
	SessionCorePrivate* priv = session_core_get_instance_private(self);

	if (priv->hub->ws)
	{
		if (soup_websocket_connection_get_state(priv->hub->ws) ==
			SOUP_WEBSOCKET_STATE_OPEN)
			 soup_websocket_connection_close(priv->hub->ws, 1000, "");
		else
			g_object_unref(priv->hub->ws);
	}



	gst_element_set_state(GST_ELEMENT(priv->pipe->pipeline), GST_STATE_NULL);
	g_print("Pipeline stopped\n");
	gst_object_unref(priv->pipe->pipeline);

	g_object_unref(priv->pipe);
	g_object_unref(priv->session);
	g_object_unref(priv->hub);
	g_object_unref(priv->session);
	g_object_unref(priv->state);
}
static void
session_core_finalize(GObject* object)
{
	g_object_unref(object);
}










gboolean 
session_core_connect_shared_memory_hub(SessionCore* self)
{
	SessionCoreClass* klass = SESSION_CORE_GET_CLASS(self);
	return klass->connect_shared_memory_hub(self);
}

gboolean
session_core_connect_signalling_server(SessionCore* self)
{
	SessionCoreClass* klass = SESSION_CORE_GET_CLASS(self);
	return klass->connect_signalling_server(self);
}

gboolean 
session_core_setup_pipeline(SessionCore* self)
{
	SessionCoreClass* klass = SESSION_CORE_GET_CLASS(self);
	return klass->setup_pipeline(self);
}

gboolean 
session_core_setup_data_channel(SessionCore* self)
{
	SessionCoreClass* klass = SESSION_CORE_GET_CLASS(self);
	return klass->setup_data_channel (self);
}

gboolean 
session_core_setup_webrtc_signalling(SessionCore* self)
{
	SessionCoreClass* klass = SESSION_CORE_GET_CLASS(self);
	return klass->setup_webrtc_signalling(self);
}

gboolean 
session_core_start_pipeline(SessionCore* self)
{
	SessionCoreClass* klass = SESSION_CORE_GET_CLASS(self);
	return klass->start_pipeline(self);
}

gboolean 
session_core_stop_pipeline(SessionCore* self)
{
	SessionCoreClass* klass = SESSION_CORE_GET_CLASS(self);
	return klass->stop_pipeline(self);
}







void
session_core_send_message(GObject* object,
	Message* msg)
{
	SessionCore* self = (SessionCore*)object;
	SessionCorePrivate* priv = session_core_get_instance_private(self);

	gboolean ret;

	GBytes* bytes;
	switch (msg->to)
	{
	case CLIENT:
		bytes = g_bytes_new(msg, sizeof(msg));
		g_signal_emit_by_name(priv->hub->control, "send-data", bytes);
		ret = TRUE;
	case AGENT:
		ret = shared_memory_link_send_message(priv->ipc->link, priv->ipc->agent_id, msg,sizeof(msg));
	case LOADER:
		ret = shared_memory_link_send_message(priv->ipc->link, priv->ipc->loader_id, msg, sizeof(msg));
	}
	return ret;
}














SessionCore*
session_core_new(gint id)
{
	return g_object_new(SESSION_TYPE_CORE, "hub-id",id);
}


void
session_core_end(const gchar* msg, SessionCore* core, CoreState state)
{

}







Pipeline*
session_core_get_pipeline(SessionCore* self)
{

	SessionCorePrivate* priv = session_core_get_instance_private(self);
	return priv->pipe;
}

WebRTCHub*
session_core_get_rtc_hub(SessionCore* self)
{
	SessionCorePrivate* priv = session_core_get_instance_private(self);
	return priv->hub;
}


SessionQoE*
session_core_get_qoe(SessionCore* self)
{
	SessionCorePrivate* priv = session_core_get_instance_private(self);
	return priv->qoe;
}

IPC*
session_core_get_ipc(SessionCore* self)
{
	SessionCorePrivate* priv = session_core_get_instance_private(self);
	return priv->ipc;
}
