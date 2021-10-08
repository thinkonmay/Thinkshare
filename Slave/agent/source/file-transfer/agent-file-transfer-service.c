/// <summary>
/// @file agent-file-transfer-service.c
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-10-01
/// 
/// @copyright Copyright (c) 2021
/// 
#include <agent-file-transfer-service.h>
#include <agent-file-transceiver.h>
#include <agent-file-compressor.h>
#include <agent-file-transceiver.h>
#include <agent-type.h>

#include <message-form.h>
#include <agent-object.h>

#include <child-process-constant.h>


#define SIGNALLING_URL  "SignallingUrl"
#define TURN_STRING     "TurnConnection"
#define SESSION_ID      "SessionSlaveID"
#define INPUT_FILE      "InputPath"

struct _FileTransferSession
{
    FileCompressor* compressor;

    FileTransceiver* transceiver;

    gchar* input_file;

    gchar* zip_file;

    gchar* signalling_url;

    gchar* turn_connection;

    gint SessionSlaveID;

    gboolean completed;

    AgentObject* agent;
};

static FileTransferSession session_pool[MAX_FILE_TRANSFER_INSTANCE] = {0};

void
initialize_file_transfer_service(AgentObject* agent)
{
    init_file_compressor_pool();
    init_file_transceiver_pool();
    memset(&session_pool,0,sizeof(session_pool));
    for(int i = 0; i<MAX_FILE_TRANSFER_INSTANCE;i++)
    {
        session_pool[i].agent = agent;
        agent_set_file_transfer_service(agent, i, &(session_pool[i]));
    }
}


FileTransferSession*
get_available_file_transfer_session()
{
    for(int i = 0;i<MAX_FILE_TRANSFER_INSTANCE;i++)
    {
        if(session_pool[i].completed)
        {
            session_pool[i].completed = FALSE;
            return &(session_pool[i]);
        }
    }
    Sleep(1000);
    return get_available_file_transfer_session();
}

void
start_file_transfer(FileTransferSession* session)
{
    session->compressor =       init_file_compressor(session);   
    start_compressor(session->compressor,session->agent);
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


    session->transceiver =      init_file_transceiver(session);
    start_transceive_compressed_file(session->transceiver,session->agent);
}

static void 
file_transfer_session_finalize(FileTransferSession* session)
{
    session->compressor = NULL;
    session->transceiver = NULL;
    session->SessionSlaveID = 0;
    session->completed = TRUE;
}


void
on_file_transceive_completed(gint SessionSlaveID)
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

    // finalize trasceiver
    file_transceiver_finalize(session);
}


FileTransferSession*
setup_file_transfer_session(gchar* server_command)
{
    GError* error = NULL;
    JsonParser* parser = json_parser_new();
    Message* message = get_json_object_from_string(server_command,&error,parser);
    if(error != NULL){
        return NULL;
    }

    FileTransferSession* session =  get_available_file_transfer_session();
    session->SessionSlaveID =       json_object_get_int_member(message, SESSION_ID);
    session->input_file =           json_object_get_string_member(message, SIGNALLING_URL);
    session->turn_connection =      json_object_get_string_member(message, TURN_STRING);
    session->input_file =           json_object_get_string_member(message, INPUT_FILE);    
    json_object_unref(message);
    g_object_unref(parser);
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