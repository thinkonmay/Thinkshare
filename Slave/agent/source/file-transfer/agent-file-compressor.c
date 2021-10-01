/// <summary>
/// @file agent-file-compressor.c
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-30
/// 
/// @copyright Copyright (c) 2021
/// 
#include <agent-file-compressor.h>
#include <agent-child-process.h>
#include <agent-file-transfer-service.h>



#include <glib-2.0/glib.h>

#include <child-process-constant.h>
#include <general-constant.h>
#include <child-process-resources-assign.h>

struct _FileCompressor
{
    gint process_id;

    gint SessionSlaveID;

    gboolean completed;

    gchar* input_path;

    gchar* output_path;

    ChildProcess* file_compressor;
};


static FileCompressor compressor_pool[MAX_FILE_TRANSFER_INSTANCE] = {0};

void
init_file_compressor_pool()
{
    memset(&compressor_pool,0,sizeof(compressor_pool));
    for(gint i = 0; i< MAX_FILE_TRANSFER_INSTANCE;i++)
    {
        compressor_pool[i].completed = TRUE;
    }
}


void
file_compressor_finalize(FileCompressor* compressor)
{
    compressor->file_compressor = NULL;
    compressor->completed = TRUE;
}

void
on_file_compress_process_completed(ChildProcess* process)
{
    // get file compressor from pool with process id
    gint process_id = get_child_process_id(process);
    FileCompressor* compressor = NULL;
    for(gint i = 0; i<MAX_FILE_TRANSFER_INSTANCE;i++)
    {
        if(compressor_pool[i].process_id == process_id)
        {
            compressor = &(compressor_pool[i].process_id);
            compressor->completed = TRUE;
        }
    }
    if(compressor == NULL)
    {
        return;
    }

    // signal back to file transfer session
    on_file_compress_completed(compressor->SessionSlaveID);
    file_compressor_finalize(compressor);
}



static void
compressor_process_handle(ChildProcess* proc,
                    DWORD exit_code,
                    AgentObject* agent)
{
    if(exit_code == STILL_ACTIVE){
        return;
    }
    else
    {
        on_file_compress_process_completed(proc);
        close_child_process(proc);
    }
}

static void
file_compress_output_handle(GBytes* data,
                            gint process_id,
                            AgentObject* agent)
{
    return;
}

void
start_compressor(FileCompressor* compressor, 
                AgentObject* agent)
{

    GString* string = g_string_new(" Compress-Archive ");
    g_string_append(string, compressor->input_path);
    g_string_append(string, " -Update -DestinationPath ");
    g_string_append(string, compressor->output_path);


    create_new_child_process(
        POWERSHELL_BINARY, 
            get_child_process_id(compressor->file_compressor),
            g_string_free(string,FALSE),
            file_compress_output_handle,
            compressor_process_handle, agent);
}

FileCompressor*
get_available_file_commpressor()
{
    for(gint i = 0; i<MAX_FILE_TRANSFER_INSTANCE;i++)
    {
        if(compressor_pool[i].completed)
        {
            compressor_pool[i].completed = FALSE;
            return &(compressor_pool[i].completed);
        }
    }
    Sleep(1000);
    return get_available_file_commpressor();
}


FileCompressor*
init_file_compressor(FileTransferSession* session)
{
    FileCompressor* compressor = get_available_file_commpressor();
    compressor->input_path = file_transfer_session_get_intput_file(session);
    compressor->SessionSlaveID = file_transfer_session_get_session_id(session);
    compressor->file_compressor = get_available_child_process();
    compressor->process_id = get_child_process_id(compressor->file_compressor);
    compressor->output_path = GET_ZIP_OUTPUT_FILE(get_child_process_id(compressor->file_compressor));
    file_transfer_session_set_zip_file(session,compressor->output_path);
    return compressor;
}
