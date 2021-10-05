#include <remote-app-signalling.h>
#include <remote-app-remote-config.h>
#include <remote-app-pipeline.h>
#include <remote-app-data-channel.h>
#include <remote-app.h>
#include <remote-app-message.h>
#include <remote-app-type.h>

#include <exit-code.h>
#include <error-code.h>
#include <module.h>
#include <opcode.h>
#include <general-constant.h>


#include <glib.h>
#include <logging.h>
#include <message-form.h>
#include <module.h>


struct _RemoteApp
{
	Pipeline* pipe;

	WebRTCHub* hub;

	GMainLoop* loop;

	SignallingHub* signalling;

	QoE* qoe;

};



/// <summary>
/// setup slave session, this step include get value from json config file 
/// </summary>
/// <param name="self"></param>
static void
remote_app_setup_session(RemoteApp* self)
{
	JsonNode* root;
	JsonObject* object;
	JsonParser* parser = json_parser_new();

	GError* error = NULL;
	json_parser_load_from_file(parser, SESSION_SLAVE_FILE, &error);
	if (error != NULL)
	{
		remote_app_finalize(self, CORRUPTED_CONFIG_FILE_EXIT, error);
		return;
	}


	root = json_parser_get_root(parser);
	object = json_node_get_object(root);

	signalling_hub_setup(self->signalling,
		json_object_get_string_member(object, "TurnConnection"),
		json_object_get_string_member(object, "SignallingUrl"),
		json_object_get_int_member(object, "SessionSlaveID"));

	JsonObject* qoe = json_object_get_object_member(object, "QoE");

	qoe_setup(self->qoe,
		json_object_get_int_member(qoe, "ScreenWidth"),
		json_object_get_int_member(qoe, "ScreenHeight"),
		json_object_get_int_member(qoe, "AudioCodec"),
		json_object_get_int_member(qoe, "VideoCodec"),
		json_object_get_int_member(qoe, "QoEMode"));


	signalling_hub_set_signalling_state(self->signalling, SIGNALLING_SERVER_READY);
	signalling_hub_set_peer_call_state(self->signalling, PEER_CALL_READY);
	pipeline_set_state(self->pipe, PIPELINE_READY);
}


static RemoteApp core = {0};


RemoteApp*
remote_app_initialize()
{
	core.hub =				webrtchub_initialize();
	core.signalling =		signalling_hub_initialize(&core);

	core.qoe =				qoe_initialize();
	core.pipe =				pipeline_initialize(&core);
	core.loop =				g_main_loop_new(NULL, FALSE);
	 
	remote_app_setup_session(&core);


	remote_app_connect_signalling_server(&core);
	g_main_loop_run(core.loop);
	return &core;	
}






void
remote_app_connect_signalling_server(RemoteApp* self)
{
	connect_to_websocket_signalling_server_async(self);
}

void
remote_app_setup_pipeline(RemoteApp* self)
{
	setup_pipeline(self);
}				


void
remote_app_send_message(RemoteApp* core, JsonObject* message)
{
	send_message(core, message);
}



















void
remote_app_finalize(RemoteApp* self, 
					  ExitCode exit_code, 
					  GError* error)
{
	PipelineState pipeline_state = pipeline_get_state(self->pipe);

	Pipeline* pipe = self->pipe;
		GstElement* pipeline = pipeline_get_pipline(pipe);

	SignallingHub* signalling = 
		remote_app_get_signalling_hub(self);

	/*agent will catch session core exit code to restart session*/
	ExitProcess(exit_code);
}









Pipeline*
remote_app_get_pipeline(RemoteApp* self)
{
	return self->pipe;
}

WebRTCHub*
remote_app_get_rtc_hub(RemoteApp* self)
{
	return self->hub;
}


QoE*
remote_app_get_qoe(RemoteApp* self)
{
	return self->qoe;
}

SignallingHub*
remote_app_get_signalling_hub(RemoteApp* core)
{
	return core->signalling;
}

GMainContext*
remote_app_get_main_context(RemoteApp* core)
{
	return g_main_loop_get_context(core->loop);
}