#include <session-core-signalling.h>
#include <session-core-remote-config.h>
#include <session-core-pipeline.h>
#include <session-core-data-channel.h>
#include <session-core.h>
#include <session-core-message.h>
#include <session-core-type.h>
#include <session-core-ipc.h>
#include <glib.h>




struct _SessionCore
{
	Pipeline* pipe;

	WebRTCHub* hub;

	CoreState state;

	GMainLoop* loop;

	SignallingHub* signalling;

	QoE* qoe;

	IPC* ipc;
};


struct _SessionQoE
{
	gint screen_height;
	gint screen_width;
	gint framerate;
	gint bitrate;
};

struct _Session
{
	gint SessionSlaveID;
	gchar* signalling_url;
	SessionQoE* qoe;
	gboolean client_offer;
	gchar* stun_server;
};





SessionCore*
session_core_initialize()
{
	SessionCore* core = malloc(sizeof(SessionCore));
	memset(core, 0, sizeof(SessionCore));

	core->ipc =				ipc_initialize(core);
	core->hub =				webrtchub_initialize();
	core->signalling =		signalling_hub_initialize();
	core->loop =			g_main_loop_new(NULL, FALSE);
	core->qoe =				qoe_initialize();
	core->pipe =			pipeline_initialize();
	core->state =			WAITING_SESSION_INFORMATION;


	g_main_loop_run(core->loop);
	g_main_loop_unref(core->loop);

	handle_thread_start(core);

	session_core_send_message(core,
		message_init(CORE_MODULE, AGENT_MODULE, 
			SESSION_INFORMATION_REQUEST, NULL));

	return core;	
}






void
session_core_connect_signalling_server(SessionCore* self)
{
	connect_to_websocket_signalling_server_async(self);
}

void
session_core_setup_pipeline (self)
{
	setup_pipeline(self);
}				

gboolean
session_core_start_pipeline(SessionCore* self)
{
	return start_pipeline(self);
}

void
session_core_send_message(SessionCore* self, Message* message)
{
	send_message(self, message);
}

gboolean
session_core_setup_data_channel(SessionCore* core)
{
	connect_data_channel_signals(core);
}







void
session_qoe_init(SessionQoE* qoe,
	gint frame_rate,
	gint screen_width,
	gint screen_height,
	gint bitrate)
{
	qoe = malloc(sizeof(SessionQoE));

	qoe->bitrate = bitrate;
	qoe->framerate = frame_rate;
	qoe->screen_height = screen_height;
	qoe->screen_width = screen_width;
}

void
session_information_init(Session* session,
	gint session_slave_id,
	gchar* signalling_url,
	SessionQoE* qoe,
	gboolean client_offer,
	gchar* stun_server)
{


	session = malloc(sizeof(Session));

	session->SessionSlaveID = session_slave_id;
	session->client_offer = client_offer;
	session->stun_server = stun_server;
	session->qoe = qoe;
	session->signalling_url = signalling_url;
}


/// <summary>
/// (PRIVATE function)
/// get_session_information from host message, 
/// used to set session information
/// </summary>
/// <param name="object"></param>
/// <returns></returns>
Session*
get_session_information_from_message(Message* object)
{
	Session* session;
	SessionQoE* qoe;

	JsonObject* session_object = json_object_get_member(object, "data");
	gint SessionSlaveID = json_object_get_int_member(session_object, "SessionSlaveID");
	gchar* signalling_url = json_object_get_string_member(session_object, "SignallingURL");
	gchar* client_offer = json_object_get_boolean_member(session_object, "ClientOffer");
	gchar* stun_server = json_object_get_string_member(session_object, "StunServer");

	JsonObject* qoe_object = json_object_get_member(session_object, "SessionQoE");
	gint screen_width = json_object_get_int_member(qoe_object, "ScreenWidth");
	gint screen_height = json_object_get_int_member(qoe_object, "ScreenHeight");
	gint framerate = json_object_get_int_member(qoe_object, "FrameRate");
	gint bitrate = json_object_get_int_member(qoe_object, "Bitrate");

	session_qoe_init(qoe, framerate,
		screen_width, screen_height, bitrate);

	session_information_init(session, SessionSlaveID,
		signalling_url, qoe, client_offer, stun_server);
	return session;
}


void
session_core_setup_session(SessionCore* self,
	Session* session)
{
	signalling_hub_setup(self->hub,
		session->signalling_url,
		session->client_offer,
		session->stun_server,
		session->SessionSlaveID);

	qoe_setup(self->qoe,
		session->qoe->screen_width,
		session->qoe->screen_height,
		session->qoe->framerate,
		session->qoe->bitrate);

	/*session core setup step have done,
	* new step into pipeline setup and 
	* signalling register
	*/

	session_core_connect_signalling_server(self);

	g_thread_new("setup-pipeline", (GThreadFunc)setup_pipeline, self);

}





void
session_core_finalize(SessionCore* self, gint exit_code)
{
	Message* msg = message_init(CORE_MODULE, AGENT_MODULE, exit_code ,NULL);
	session_core_send_message(self, msg);

	Pipeline* pipe = self->pipe;
	GstElement* pipeline = pipeline_get_pipline(pipe);

	gst_element_set_state(
		GST_ELEMENT(pipeline), GST_STATE_NULL);
	g_free(self->pipe);
	g_free(self->pipe);
	g_free(self->hub);
	g_free(self->state);



	if(self->loop)
	{
		g_main_loop_quit(self->loop);
	}
}











/*START get-set function related to session core object*/

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


CoreState
session_core_get_state(SessionCore* self)
{
	self->state;
}

void
session_core_set_state(SessionCore* core, CoreState state)
{
	core->state = state;
}

SignallingHub*
session_core_get_signalling_hub(SessionCore* core)
{
	return core->signalling;
}

IPC*
session_core_get_ipc(SessionCore* core)
{
	return core->ipc;
}


/*END get-set function related to session core object*/