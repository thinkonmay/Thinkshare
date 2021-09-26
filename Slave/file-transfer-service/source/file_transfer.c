#include <file-transfer-data-channel.h>
#include <file-transfer.h>
#include <file-transfer-type.h>

#include <exit-code.h>
#include <error-code.h>
#include <module.h>
#include <opcode.h>
#include <general-constant.h>


#include <glib.h>
#include <logging.h>
#include <message-form.h>


struct _FileTransferSvc
{
	WebRTChub* pipe;

	WebRTCDataChannelPool* hub;

	GMainLoop* loop;

	SignallingHub* signalling;
};




/// <summary>
/// setup slave session, this step include get value from json config file 
/// </summary>
/// <param name="self"></param>
static void
file_transfer_setup_session(FileTransferSvc* self)
{

	root = json_parser_get_root(parser);
	object = json_node_get_object(root);

	signalling_hub_setup(self->signalling,
		json_object_get_string_member(object, "TurnConnection"),
		json_object_get_string_member(object, "SignallingUrl"),
		json_object_get_int_member(object, 	  "FileTransferSlaveID"));



	signalling_hub_set_signalling_state(self->signalling, SIGNALLING_SERVER_READY);
	signalling_hub_set_peer_call_state(self->signalling, PEER_CALL_READY);
	pipeline_set_state(self->pipe, PIPELINE_READY);
	
	write_to_log_file(SESSION_CORE_GENERAL_LOG,"session core setup done");
}

FileTransferSvc*
file_transfer_initialize()
{
	static FileTransferSvc core;
	write_to_log_file(SESSION_CORE_GENERAL_LOG,"Session core process started");

	core.hub =				init_datachannel_pool();
	core.signalling =		signalling_hub_initialize(&core);

	core.state =			SESSION_CORE_INITIALIZING;
	core.loop =				g_main_loop_new(NULL, FALSE);
	 
	file_transfer_setup_session(&core);


	file_transfer_connect_signalling_server(&core);
	g_main_loop_run(core.loop);
	return &core;	
}






void
file_transfer_connect_signalling_server(FileTransferSvc* self)
{
	connect_to_websocket_signalling_server_async(self);
}

void
file_transfer_setup_pipeline(FileTransferSvc* self)
{
	setup_pipeline(self);
}				


void
file_transfer_send_message(FileTransferSvc* core, Message* message)
{
	send_message(core, message);
}
















Message*
get_json_exit_state(ExitState* state)
{
	Message* message = json_object_new();
	json_object_set_int_member(message, "ExitCode", state->code);
	json_object_set_string_member(message, "CoreState", state->core_state);
	json_object_set_string_member(message, "WebRTChubState", state->pipeline_state);
	json_object_set_string_member(message, "SignallingState", state->signalling_state);
	json_object_set_string_member(message, "PeerCallState", state->peer_state);
	json_object_set_string_member(message, "Message", state->error->message);
	return message;
}




void
file_transfer_finalize(FileTransferSvc* self, 
					  ExitCode exit_code, 
					  GError* error)
{
	WebRTChubState pipeline_state = pipeline_get_state(self->pipe);

	WebRTChub* pipe = self->pipe;
		GstElement* pipeline = pipeline_get_pipline(pipe);

	SignallingHub* signalling = 
		file_transfer_get_signalling_hub(self);

    //exit current state to report to slave manager
	ExitState state;

	state.code = exit_code;
	state.pipeline_state = pipeline_state;
	state.error = error;

	state.signalling_state = 
		signalling_hub_get_signalling_state(self->signalling);
	state.peer_state = 
		signalling_hub_get_peer_call_state(self->signalling);

	signalling_close(signalling);

	write_to_log_file(SESSION_CORE_GENERAL_LOG,"session core exited\n");

	Message* message = get_json_exit_state(&state);
	if(!error == NULL)
	{
		report_file_transfer_error(self,
			get_string_from_json_object(message));
	}
	/*agent will catch session core exit code to restart session*/
	ExitProcess(exit_code);
}



void
report_file_transfer_error(FileTransferSvc* self,
						  ErrorCode code)
{
	JsonParser* parser = json_parser_new();
	json_parser_load_from_file(parser, HOST_CONFIG_FILE,NULL);
	JsonNode* root = json_parser_get_root(parser);
	JsonObject* json = json_node_get_object(root);
	gint SlaveID = json_object_get_int_member(json,DEVICE_ID);


	JsonObject* obj = json_object_new();
	json_object_set_int_member(obj,
		"SlaveID",SlaveID);
	json_object_set_int_member(obj,
		"Module",CORE_MODULE);	
	json_object_set_string_member(obj,
		"ErrorMessage",code);

	Message* msg_host = message_init(CORE_MODULE,
		HOST_MODULE, ERROR_REPORT, obj);

	file_transfer_send_message(self, msg_host);
}








WebRTChub*
file_transfer_get_pipeline(FileTransferSvc* self)
{
	return self->pipe;
}

WebRTCDataChannelPool*
file_transfer_get_dc_pool(FileTransferSvc* self)
{
	return self->hub;
}


QoE*
file_transfer_get_qoe(FileTransferSvc* self)
{
	return self->qoe;
}



GMainContext*
file_transfer_get_main_context(FileTransferSvc* core)
{
	return g_main_loop_get_context(core->loop);
}