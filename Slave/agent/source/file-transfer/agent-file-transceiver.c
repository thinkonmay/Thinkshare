/// <summary>
/// @file agent-file-transceiver.c
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-30
/// 
/// @copyright Copyright (c) 2021
/// 
#include <agent-file-transceiver.h>
#include <agent-file-transfer-service.h>

#include <glib-2.0/glib.h>


#include <child-process-constant.h>
#include <general-constant.h>



struct _FileTransceiver
{
    gint process_id;

    gboolean completed;

    gint SessionSlaveID;

    gchar* input_file;

    gchar* signalling_url;

    gchar* turn_connection;

    ChildProcess* file_transceiver;
};

static FileTransceiver transceiver_pool[MAX_FILE_TRANSFER_INSTANCE] = {0};

void
init_file_transceiver_pool()
{
    memset(&transceiver_pool,0,sizeof(transceiver_pool));
}

FileTransceiver*
get_available_file_transceiver()
{
    for(gint i = 0; i < MAX_FILE_TRANSFER_INSTANCE;i++)
    {
        if(transceiver_pool[i].completed)
        {
            transceiver_pool[i].completed = FALSE;
            return &(transceiver_pool[i]);
        }
    }
    Sleep(1000);
    return get_available_file_transceiver();
}


static void
file_transceiver_finalize(FileTransceiver* transceiver)
{
    transceiver->file_transceiver = NULL;
    transceiver->completed = TRUE;
}

static void
on_file_transceive_process_completed(ChildProcess* process)
{
    // get transceiver from process id
    gint id = get_child_process_id(process);
    FileTransceiver* transceiver = NULL;
    for(gint i = 0;i<MAX_FILE_TRANSFER_INSTANCE;i++)
    {
        if(transceiver_pool[i].process_id == id)
        {
            transceiver = &(transceiver_pool[i]);
        }
    }
    if(transceiver == NULL)
    {
        return;
    }

    // signal back to file transfer session
    on_file_transceive_completed(transceiver->SessionSlaveID);
    // finalize after stop using
    file_transceiver_finalize(transceiver);
}

static void
file_transceiver_output_handle(GBytes* data,
                            gint process_id,
                            AgentObject* agent)
{
    return;
}

static void
transceiver_process_handle(ChildProcess* proc,
                    DWORD exit_code,
                    AgentObject* agent)
{
    if(exit_code == STILL_ACTIVE){
        return;
    }
    else
    {
        on_file_transceive_process_completed(proc);
        close_child_process(proc);
    }
}

void
start_transceive_compressed_file(FileTransceiver* transceiver, 
                              AgentObject* agent)
{

    GString* string = g_string_new("--sessionid ");
    g_string_append(string, transceiver->SessionSlaveID);
    g_string_append(string, " --signalling ");
    g_string_append(string, transceiver->signalling_url);
    g_string_append(string, " --file ");
    g_string_append(string, transceiver->input_file);
    g_string_append(string, " --turn ");
    g_string_append(string, transceiver->turn_connection);

    create_new_child_process(
        FILE_TRANSCEIVER_BINARY, 
            get_child_process_id(transceiver->file_transceiver),
            g_string_free(string,FALSE),
            file_transceiver_output_handle,
            transceiver_process_handle, agent);
}







FileTransceiver*
init_file_transceiver(FileTransferSession* session)
{
    FileTransceiver* transceiver =  get_available_file_transceiver();
    transceiver->file_transceiver = get_available_child_process();
    transceiver->input_file =       file_transfer_session_get_zip_file(session);
    transceiver->SessionSlaveID =   file_transfer_session_get_session_id(session);
    transceiver->process_id =       get_child_process_id(transceiver->file_transceiver);
    transceiver->turn_connection =  file_transfer_session_get_turnconnection(session);
    transceiver->signalling_url =   file_transfer_session_get_signalling_url(session);
    return transceiver;
}


