#include <agent-file-transfer-service.h>
#include <agent-file-sender.h>
#include <agent-file-compressor.h>

#include <message-form.h>
#include <agent-object.h>

#include <child-process-constant.h>


struct _FileTransferSession
{
    FileCompressor* compressor;

    FileTransceiver* sender;

    gchar* input_file;

    gchar* signalling_url;

    gchar* turn_connection;

    gint SessionSlaveID;
};





void
start_file_transfer(FileTransferSession* service)
{
}



FileTransferSession*
setup_file_transfer_session(gchar* server_command)
{
    GError* error = NULL;
    Message* message = get_json_object_from_string(server_command,&error);
    if(error != NULL){
        return NULL;
    }

    FileTransferSession* session = malloc(sizeof(FileTransferSession));

    session->input_file =       json_object_get_string_member(message,"SignallingUrl");
    session->SessionSlaveID =   json_object_get_int_member(message,"SessionSlaveID");
    session->turn_connection =  json_object_get_string_member(message,"TurnConnection");




    return session;
}