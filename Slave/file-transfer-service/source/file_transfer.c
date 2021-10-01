#include <file-transfer-data-channel.h>
#include <file-transfer.h>
#include <file-transfer-type.h>
#include <file-transfer-signalling.h>
#include <file-transfer-webrtcbin.h>

#include <exit-code.h>
#include <error-code.h>
#include <module.h>
#include <opcode.h>
#include <general-constant.h>


#include <glib.h>
#include <logging.h>
#include <message-form.h>


struct _FileTransferService
{
	WebRTCbin* webrtcbin;

	FileTransferHub* hub;

	GMainLoop* loop;

	FileTransferSignalling* signalling;
};



static void
file_transfer_setup_session(FileTransferService* self,
							gchar* signalling_url,
							gint session_id,
							gchar* file, 
							gchar* turn_connection)
{
	signalling_hub_setup(self->signalling,signalling_url,session_id);
	webrtcbin_get_turn_connection(self->webrtcbin,turn_connection);
	webrtc_data_channel_get_file(self->webrtcbin,file);
}



static FileTransferService service;

FileTransferService*
file_transfer_initialize(gchar* signalling_url,
						 gint session_id, 
						 gchar* file, 
						 gchar* turn)
{
	write_to_log_file(SESSION_CORE_GENERAL_LOG,"Session core process started");

	service.hub =				init_datachannel_pool();
	service.signalling =		signalling_hub_initialize(&service);
	service.webrtcbin = 				webrtcbin_initialize(&service);
	service.loop =				g_main_loop_new(NULL, FALSE);
	 
	file_transfer_setup_session(&service,signalling_url,session_id,file,turn);


	file_transfer_connect_signalling_server(&service);
	g_main_loop_run(service.loop);
	return &service;	
}






void
file_transfer_connect_signalling_server(FileTransferService* self)
{
	connect_to_websocket_signalling_server_async(self);
}

void
file_transfer_setup_pipeline(FileTransferService* self)
{
	setup_pipeline(self);
}				




















void
file_transfer_finalize(FileTransferService* self, 
					  ExitCode code,
					  GError* error)
{
	g_printerr(error->message);
	ExitProcess(code);
}



void
report_file_transfer_error(FileTransferService* self,
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








WebRTCbin*
file_transfer_get_webrtcbin(FileTransferService* self)
{
	return self->webrtcbin;
}

FileTransferHub*
file_transfer_get_transfer_hub(FileTransferService* self)
{
	return self->hub;
}



GMainContext*
file_transfer_get_main_context(FileTransferService* self)
{
	return g_main_loop_get_context(self->loop);
}


FileTransferSignalling*
file_transfer_get_signalling_hub(FileTransferService* self)
{
	return self->signalling;
}