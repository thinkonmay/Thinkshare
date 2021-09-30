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

	FileTransferSignalling* signalling;
};



void
file_transfer_setup_session(FileTransferSvc* svc)
{
	Message* message = get_json_object_from_file();
	FileTransferSignalling* signalling = file_transfer_get_signalling_hub();

}


FileTransferSvc*
file_transfer_initialize()
{
	static FileTransferSvc core;
	write_to_log_file(SESSION_CORE_GENERAL_LOG,"Session core process started");

	core.hub =				init_datachannel_pool();
	core.signalling =		signalling_hub_initialize(&core);

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
file_transfer_finalize(FileTransferSvc* self, 
					  GError* error)
{
	ExitProcess(0);
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



GMainContext*
file_transfer_get_main_context(FileTransferSvc* core)
{
	return g_main_loop_get_context(core->loop);
}


FileTransferSignalling*
file_transfer_get_signalling_hub(FileTransferSvc* self)
{
	return self->signalling;
}