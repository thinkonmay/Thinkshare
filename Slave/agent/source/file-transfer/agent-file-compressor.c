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


#include <glib-2.0/glib.h>

#include <child-process-constant.h>
#include <general-constant.h>

struct _FileCompressor
{
    gchar* input_path;

    gchar* output_path;

    ChildProcess* file_compressor;
};


static FileCompressor compressor_pool[MAX_FILE_TRANSFER_INSTANCE] = {0};

void
init_file_compressor()
{
    memset(&compressor_pool,0,sizeof(compressor_pool));
    
}



static void
shell_process_handle(ChildProcess* proc,
                    DWORD exit_code,
                    AgentObject* agent)
{
    if(exit_code == STILL_ACTIVE)
    {
        return;
    }
    else
    {
        gint id = get_child_process_id(proc);
        agent_on_file_compress_completed(proc);
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
create_powershell_compressor(AgentObject* agent)
{

    GString* string = g_string_new("Compress-Archive ");
    g_string_append(string, compressor->input_path);
    g_string_append(string, " -Update -DestinationPath ");
    g_string_append(string, 
        output_zip_map(
        get_child_process_id(compressor->file_compressor)));


    create_new_child_process(
        "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe ", 
            get_child_process_id(compressor->file_compressor),
            g_string_free(string,FALSE),
            file_compress_output_handle,
            shell_process_handle, agent);
}


FileCompressor*
init_file_compressor(gchar* input_path, 
                     gchar* output_path)
{
    FileCompressor* compressor = malloc(sizeof(FileCompressor));
    compressor->input_path = input_path;
    compressor->output_path = output_path;
    compressor->file_compressor = get_available_child_process();
    return compressor;
}
