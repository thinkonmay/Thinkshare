#include <session-core-signalling.h>
#include <session-core-remote-config.h>
#include <session-core-pipeline.h>
#include <session-core-data-channel.h>
#include <session-core.h>
#include <session-core-message.h>
#include <session-core-type.h>
#include <session-core-ipc.h>

#include <exit-code.h>
#include <error-code.h>
#include <module.h>
#include <opcode.h>
#include <general-constant.h>


#include <glib.h>
#include <logging.h>


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



typedef struct
{
	ExitCode				code;
	CoreState				core_state;
	PipelineState			pipeline_state;
	SignallingServerState	signalling_state;
	PeerCallState			peer_state;
	GError*					error;
}ExitState;



void
session_core_setup_session(SessionCore* self)
{
	JsonNode* root;
	JsonObject* object;
	JsonParser* parser = json_parser_new();

	GError* error = NULL;
	json_parser_load_from_file(parser, SESSION_SLAVE_FILE, &error);
	if (error != NULL)
	{
		session_core_finalize(self, CORRUPTED_CONFIG_FILE_EXIT, error);
		return;
	}


	root = json_parser_get_root(parser);
	object = json_node_get_object(root);

	signalling_hub_setup(self->signalling,
		json_object_get_string_member(object, "SignallingUrl"),
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


	signalling_hub_set_signalling_state(self->signalling, SIGNALLING_SERVER_READY);
	signalling_hub_set_peer_call_state(self->signalling, PEER_CALL_READY);
	pipeline_set_state(self->pipe, PIPELINE_READY);
	self->state = SESSION_INFORMATION_SETTLED;
	
	write_to_log_file(SESSION_CORE_GENERAL_LOG,"session core setup done");
}

SessionCore*
session_core_initialize()
{
	static SessionCore core;
	write_to_log_file(SESSION_CORE_GENERAL_LOG,"Session core process started");

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
















Message*
get_json_exit_state(ExitState* state)
{
	Message* message = json_object_new();


	json_object_set_int_member(message, "ExitCode", state->code);
	json_object_set_int_member(message, "CoreState", state->core_state);
	json_object_set_int_member(message, "PipelineState", state->pipeline_state);
	json_object_set_int_member(message, "SignallingState", state->signalling_state);
	json_object_set_int_member(message, "PeerCallState", state->peer_state);
	json_object_set_int_member(message, "ExitTime", g_get_real_time());

	if(state->error != NULL)
	{
		json_object_set_string_member(message, "Message", state->error->message);
	}
	else
	{
		json_object_set_string_member(message, "Message"," ");
	}
	return message;
}




void
session_core_finalize(SessionCore* self, 
					ExitCode exit_code, 
					GError* error)
{
	PipelineState pipeline_state = pipeline_get_state(self->pipe);

	Pipeline* pipe = self->pipe;
		GstElement* pipeline = pipeline_get_pipline(pipe);

	gst_element_set_state(
		GST_ELEMENT(pipeline), GST_STATE_NULL);

	SignallingHub* signalling = 
		session_core_get_signalling_hub(self);

	SoupWebsocketConnection* connection = 
		signalling_hub_get_websocket_connection(signalling);

	ExitState state;

	state.code = exit_code;
	state.core_state = self->state;
	state.pipeline_state = pipeline_state;
	state.error = error;

	state.signalling_state = 
		signalling_hub_get_signalling_state(self->signalling);
	state.peer_state = 
		signalling_hub_get_peer_call_state(self->signalling);

	
	if (connection != NULL) {
		soup_websocket_connection_close(connection, 0, "");
	}

	write_to_log_file(SESSION_CORE_GENERAL_LOG,"session core exited\n");

	Message* msg_host = message_init(CORE_MODULE, 
		HOST_MODULE, EXIT_CODE_REPORT, get_json_exit_state(&state));

	session_core_send_message(self, msg_host);

	/*agent will catch session core exit code to restart session*/
	ExitProcess(exit_code);
}



void
report_session_core_error(SessionCore* self,
						  ErrorCode code)
{
	Message* msg = json_object_new();

	json_object_set_string_member(msg, 
		"ErrorMessage", code);
	json_object_set_int_member(msg,
		"ErrorTime",g_get_real_time());

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

GMainContext*
session_core_get_main_context(SessionCore* core)
{
	return g_main_loop_get_context(core->loop);
}