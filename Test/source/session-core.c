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

	GFile* session;
};



typedef struct
{
	ExitCode				code;
	CoreState				core_state;
	PipelineState			pipeline_state;
	SignallingServerState	signalling_state;
	PeerCallState			peer_state;
}ExitState;



void
session_core_setup_session(SessionCore* self);

SessionCore*
session_core_initialize()
{
	static SessionCore core;


	core.session = g_file_parse_name("C:\\ThinkMay\\Session.txt");

	core.ipc =				ipc_initialize(&core);
	core.hub =				webrtchub_initialize();
	core.signalling =		signalling_hub_initialize(&core);

	core.qoe =				qoe_initialize();
	core.pipe =				pipeline_initialize(&core);

	core.state =			SESSION_CORE_INITIALIZING;
	core.loop =				g_main_loop_new(NULL, FALSE);

	session_core_setup_session(&core);


	session_core_connect_signalling_server(&core);
	g_main_loop_run(core.loop);
	g_main_loop_unref(core.loop);

	return &core;	
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
session_core_start_pipeline(SessionCore* core)
{
	return start_pipeline(core);
}

void
session_core_send_message(SessionCore* core, Message* message)
{
	send_message(core, message);
}

gboolean
session_core_setup_data_channel(SessionCore* core)
{
	connect_data_channel_signals(core);
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

	JsonObject* session_object =		json_object_get_member(object, "data");
	gint SessionSlaveID =				json_object_get_int_member(session_object, "SessionSlaveID");
	gchar* signalling_url =				json_object_get_string_member(session_object, "SignallingURL");
	gchar* client_offer =				json_object_get_boolean_member(session_object, "ClientOffer");
	gchar* stun_server =				json_object_get_string_member(session_object, "StunServer");

	JsonObject* qoe_object =			json_object_get_member(session_object, "SessionQoE");
	gint screen_width =					json_object_get_int_member(qoe_object, "ScreenWidth");
	gint screen_height =				json_object_get_int_member(qoe_object, "ScreenHeight");
	gint framerate =					json_object_get_int_member(qoe_object, "FrameRate");
	gint bitrate =						json_object_get_int_member(qoe_object, "Bitrate");



	return session;
}


void
session_core_setup_session(SessionCore* self)
{
	GFile* session = session_core_get_session(self);

	GBytes* bytes = g_file_load_bytes(session, NULL, NULL, NULL);

	gchar* text = g_bytes_get_data(bytes, NULL);

	JsonNode* root;
	JsonObject* object;
	JsonParser* parser = json_parser_new();
	json_parser_load_from_data(parser, text, -1, NULL);

	root = json_parser_get_root(parser);
	if (!JSON_NODE_HOLDS_OBJECT(root))
	{
		report_session_core_error(self, CORRUPTED_SESSION_INFORMATION);
		return;
	}

	object = json_node_get_object(root);

	signalling_hub_setup(self->signalling,
		json_object_get_string_member(object,	"SignallingUrl"),
		json_object_get_boolean_member(object, "ClientOffer"),
		json_object_get_string_member(object, "StunServer"),
		json_object_get_int_member(object, "SessionSlaveID"));

	JsonNode* qoe = json_object_get_object_member(object, "QoE");

	qoe_setup(self->qoe,
		json_object_get_int_member(qoe, "ScreenWidth"),
		json_object_get_int_member(qoe, "ScreenHeight"),
		json_object_get_int_member(qoe, "Framerate"),
		json_object_get_int_member(qoe, "Bitrate"),
		json_object_get_int_member(qoe, "AudioCodec"),
		json_object_get_int_member(qoe, "VideoCodec"),
		json_object_get_int_member(qoe, "QoEMode"));

	/*session core setup step have done,
	* new step into pipeline setup and 
	* signalling register
	*/

	signalling_hub_set_signalling_state(self->signalling, SIGNALLING_SERVER_READY);
	signalling_hub_set_peer_call_state(self->signalling, PEER_CALL_READY);
	pipeline_set_state(self->pipe, PIPELINE_READY);
	self->state = SESSION_INFORMATION_SETTLED;
}















Message*
get_json_exit_state(ExitState* state)
{
	Message* message = json_object_new();

	json_object_set_int_member(message, "ExitCode", state->code);
	json_object_set_int_member(message, "CoreState", state->core_state);
	json_object_set_int_member(message, "PipelineState", state->pipeline_state);
	json_object_set_int_member(message, "SignallingState", state->signalling_state);
	json_object_set_int_member(message, "PeerCallState", state->peer_state);

	return message;
}




void
session_core_finalize(SessionCore* self, ExitCode exit_code)
{
	PipelineState pipeline_state = pipeline_get_state(self->pipe);

	ExitState state;

	state.code = exit_code;
	state.core_state = self->state;
	state.pipeline_state = pipeline_state;

	state.signalling_state = 
		signalling_hub_get_signalling_state(self->signalling);
	state.peer_state = 
		signalling_hub_get_peer_call_state(self->signalling);


	g_print("Exit session core with exit code %d,\n core state %d, \n pipeline state %d, \n signalling state %d,\n peer call state %d\n",
		state.code, state.core_state, state.pipeline_state, state.signalling_state, state.peer_state);


	Message* msg_host = message_init(CORE_MODULE, 
		HOST_MODULE, EXIT_CODE_REPORT, get_json_exit_state(&state));
	Message* msg_agent = message_init(CORE_MODULE,
		AGENT_MODULE, EXIT_CODE_REPORT, get_json_exit_state(&state));

	session_core_send_message(self, msg_host);
	session_core_send_message(self, msg_agent);


	Pipeline* pipe = self->pipe;
	GstElement* pipeline = pipeline_get_pipline(pipe);

	gst_element_set_state(
		GST_ELEMENT(pipeline), GST_STATE_NULL);

	ExitProcess(0);
}



void
report_session_core_error(SessionCore* self,
						  ErrorCode code)
{
	Message* msg = json_object_new();
	json_object_set_int_member(msg, "ErrorCode", code);

	Message* msg_host = message_init(CORE_MODULE,
		HOST_MODULE, ERROR_REPORT, msg);

	session_core_send_message(self, msg_host);
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

GFile* 
session_core_get_session(SessionCore* core)
{
	return core->session;
}

GMainContext*
sessioin_core_get_main_context(SessionCore* core)
{
	return g_main_loop_get_context(core->loop);
}