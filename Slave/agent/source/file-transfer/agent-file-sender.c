/// <summary>
/// @file agent-file-sender.c
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-30
/// 
/// @copyright Copyright (c) 2021
/// 
#include <agent-file-sender.h>
#include <agent-file-transfer-service.h>

#include <glib-2.0/glib.h>


#include <child-process-constant.h>



struct _FileTransceiver
{
    gint process_id;

    gint SessionSlaveID;

    gboolean completed;

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
    for(gint i = 0; i<MAX_FILE_TRANSFER_INSTANCE;i++)
    {
        if(transceiver_pool[i].completed)
        {
            return &(transceiver_pool[i]);
        }
    }
    Sleep(1000);
    return get_available_file_transceiver();
}




void
on_file_compress_completed(gint SessionSlaveID)
{
    FileTransceiver* transceiver = NULL;
    for(gint i = 0; i < MAX_FILE_TRANSFER_INSTANCE; i++)
    {
        if(transceiver_pool[i].SessionSlaveID == SessionSlaveID)
        {
            transceiver = &(transceiver_pool[i].SessionSlaveID);
        }
    }
    if(transceiver == NULL)
    {
        return;
    }

        

}


FileTransceiver*
init_file_transceiver(FileTransferSession* session)
{
    FileTransceiver* transceiver = get_available_file_commpressor();
    transceiver->completed = FALSE;
    transceiver->input_file =       file_transfer_session_get_zip_file(session);
    transceiver->SessionSlaveID =   file_transfer_session_get_session_id(session);
    transceiver->file_transceiver = get_available_child_process();
    transceiver->process_id =       get_child_process_id(transceiver->file_transceiver);
    transceiver->turn_connection =  file_transfer_session_get_turnconnection(session);
    transceiver->signalling_url =   file_transfer_session_get_signalling_url(session);
    return transceiver;
}


