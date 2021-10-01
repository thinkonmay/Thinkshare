#include <agent-file-transfer-service.h>
#include <agent-file-sender.h>
#include <agent-file-compressor.h>
#include <agent-type.h>

#include <message-form.h>
#include <agent-object.h>

#include <child-process-constant.h>


struct _FileTransferSession
{
    FileCompressor* compressor;

    FileTransceiver* transceiver;

    gchar* input_file;

    gchar* zip_file;

    gchar* signalling_url;

    gchar* turn_connection;

    gint SessionSlaveID;
};

static FileTransferSession session_pool[MAX_FILE_TRANSFER_INSTANCE] = {0};

void
initialize_file_transfer_service(AgentObject* agent)
{
    memset(&session_pool,0,sizeof(session_pool));
}


void
start_file_transfer(FileTransferSession* service)
{
}

void
on_file_compress_completed(gint SessionSlaveID)
{
    // get session from pool with session slave id
    FileTransferSession *session = NULL;
    for(gint i = 0; i<MAX_FILE_TRANSFER_INSTANCE;i++)
    {
        if(session_pool[i].SessionSlaveID == SessionSlaveID)
        {
            session = session_pool[i].SessionSlaveID;
        }
    }
    if(session == NULL)
    {
        return;
    }

    // actual handle the event



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
    session->input_file =       json_object_get_string_member(message, "InputPath");

    session->compressor =       init_file_compressor(session);
    session->transceiver =      init_file_transceiver(session);
    
    
    return session;
}



gchar* 
file_transfer_session_get_turnconnection(FileTransferSession* session)
{
    return session->turn_connection;
}

gchar* 
file_transfer_session_get_input_file(FileTransferSession* session)
{
    return session->input_file;
}

gchar* 
file_transfer_session_get_zip_file(FileTransferSession* session)
{
    return session->zip_file;
}

void
file_transfer_session_set_zip_file(FileTransferSession* session, 
                                   gchar* path)
{
    session->zip_file = path;
}


gchar*
file_transfer_session_get_signalling_url(FileTransferSession* session)
{
    return session->signalling_url;
}

gchar* 
file_transfer_session_get_session_id(FileTransferSession* session)
{
    return session->SessionSlaveID;
}