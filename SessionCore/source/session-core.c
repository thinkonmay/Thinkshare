#include <SignallingHandling.h>
#include <RcConfig.h>
#include <HandleDataChannel.h>
#include <HandlePipeline.h>
#include <session-core.h>
#include <SharedMemory.h>





typedef struct 
{
	Pipeline* pipe;

	SessionQoE* qoe;

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

	klass->connect_signalling_server =	connect_to_websocket_signalling_server_async;
	klass->setup_pipeline =				setup_pipeline;
	klass->setup_webrtc_signalling =	connect_WebRTCHub_handler;
	klass->setup_data_channel =			connect_data_channel_signals;
	klass->start_pipeline =				start_pipeline;
	klass->stop_pipeline =				stop_pipeline;
	klass->send_message =				session_core_send_message;


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
session_core_set_property(GObject* object, 
	guint prop_id,
	const GValue *value, 
	GParamSpec* pspec)
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
	g_object_unref(priv->hub);
	g_object_unref(priv->state);
}
static void
session_core_finalize(GObject* object)
{
	g_object_unref(object);
}







gboolean
session_core_send_message(GObject* object,
	Message* message)
{
	SessionCore* self = (SessionCore*)object;
	SessionCorePrivate* priv = session_core_get_instance_private(self);
	SharedMemoryHubClass* klass = SHARED_MEMORY_HUB_GET_CLASS(priv->ipc->hub);

	gboolean ret;

	
	switch (message->to)
	{
	case CLIENT:
	{
		GBytes* data = g_bytes_new(message, sizeof(message));
		g_signal_emit_by_name(priv->hub->control, "send-data", data);
		ret = TRUE;
	}
	case AGENT:
		ret = send_message_through_shared_memory(self, message);
	case LOADER:
		ret = send_message_through_shared_memory(self, message);
	case HOST:
		ret = send_message_through_shared_memory(self, message);
	}
	return ret;
}














SessionCore*
session_core_initialize(gint argc, gchar* argv[])
{
	SessionCore* core = g_object_new(SESSION_TYPE_CORE,NULL);
	SessionCorePrivate* priv = session_core_get_instance_private(core);

	priv->ipc->hub = shared_memory_hub_initialize(argc,argv);

	priv->ipc->hub_id[AGENT] = 0;
	priv->ipc->hub_id[CORE]  = *(gint*)argv[2];
	priv->ipc->hub_id[LOADER] = *(gint*)argv[3];

	g_signal_connect(priv->ipc->hub,"on-message",
		G_CALLBACK(on_shared_memory_message),core);
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
