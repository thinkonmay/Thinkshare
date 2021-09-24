/// <summary>
/// @file agent-shell-session.c
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-24
/// 
/// @copyright Copyright (c) 2021
/// 
#include <agent-shell-session.h>
#include <agent-type.h>
#include <agent-message.h>
#include <agent-child-process.h>
#include <agent-socket.h>


#include <logging.h>
#include <general-constant.h>
#include <message-form.h>
#include <error-code.h>
#include <agent-object.h>
#include <child-process-constant.h>

   
struct _ShellSession
{
    ChildProcess* process;

    gchar* script_file;

    gchar* output_file;

    gchar* shell_script;
};


static ShellSession shell_session_pool[LAST_CHILD_PROCESS] = {0};


gchar*
shell_session_get_output(gint process_id)
{
    GError* error = NULL;
    gchar* shell_script;
    g_file_get_contents(shell_session_pool[process_id].output_file,
        &shell_script,NULL, &error);
    if(error != NULL)
        return NULL;

    return shell_script;
}


gchar*
shell_session_get_script(gint process_id)
{
    GError* error = NULL;
    gchar* shell_script;
    g_file_get_contents(shell_session_pool[process_id].script_file,
        &shell_script,NULL, &error);
    if(error != NULL)
        return NULL;

    return shell_script;
}



gchar*
shell_script_map(gint process_id)
{
    switch(process_id)
    {
        case POWERSHELL_1:
            return SHELL_SCRIPT_BUFFER_1;
        case POWERSHELL_2:
            return SHELL_SCRIPT_BUFFER_2;
        case POWERSHELL_3:
            return SHELL_SCRIPT_BUFFER_3;
        case POWERSHELL_4:
            return SHELL_SCRIPT_BUFFER_4;
        case POWERSHELL_5:
            return SHELL_SCRIPT_BUFFER_5;
        case POWERSHELL_6:
            return SHELL_SCRIPT_BUFFER_6;
        case POWERSHELL_7:
            return SHELL_SCRIPT_BUFFER_7;
        case POWERSHELL_8:
            return SHELL_SCRIPT_BUFFER_8;
    }
}

gchar*
shell_output_map(gint process_id)
{
    switch(process_id)
    {
        case POWERSHELL_1:
            return SHELL_OUTPUT_BUFFER_1;
        case POWERSHELL_2:
            return SHELL_OUTPUT_BUFFER_2;
        case POWERSHELL_3:
            return SHELL_OUTPUT_BUFFER_3;
        case POWERSHELL_4:
            return SHELL_OUTPUT_BUFFER_4;
        case POWERSHELL_5:
            return SHELL_OUTPUT_BUFFER_5;
        case POWERSHELL_6:
            return SHELL_OUTPUT_BUFFER_6;
        case POWERSHELL_7:
            return SHELL_OUTPUT_BUFFER_7;
        case POWERSHELL_8:
            return SHELL_OUTPUT_BUFFER_8;
    }
}

static ShellSession* 
get_shell_session(gint process_id)
{
    return &(shell_session_pool[process_id]);
}

void
shell_output_handle(GBytes* data,
    gint process_id,
    AgentObject* agent)
{
    return;
}


void
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
        agent_on_shell_process_terminate(agent, id);
        close_child_process(proc);
    }
}



static void
create_new_shell_process(AgentObject* agent, 
                         ShellSession* session)
{
    GString* string = g_string_new(session->script_file);
    g_string_append(string, " | Out-File -Encoding UTF8 ");
    g_string_append(string, session->output_file);
    gchar* command = g_string_free(string,FALSE);

    gint childprocess_id = get_child_process_id(session->process);

    create_new_child_process(
        "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe ", 
            childprocess_id, command,
            shell_output_handle,
            shell_process_handle, agent);
}

static void 
write_to_script_file(AgentObject* agent, ShellSession* session)
{
    GFile* file = g_file_parse_name(session->script_file);

    if(!g_file_replace_contents(file, session->shell_script,strlen(session->shell_script),
        NULL,FALSE,G_FILE_CREATE_REPLACE_DESTINATION,NULL,NULL, NULL,NULL))
    {
        agent_report_error(agent,ERROR_FILE_OPERATION);					
    }

    return;
}

void
initialize_shell_session(AgentObject* agent,
                         gchar* data_string)
{

    GError* error = NULL;
    Message* json_data = get_json_object_from_string(data_string,&error);
    if(!error == NULL) {return;}

    ChildProcess* process = get_available_shell_process();
    gint process_id = get_child_process_id(process);

    ShellSession* session = get_shell_session(process_id);
    session->script_file = shell_script_map(process_id);
    session->output_file = shell_output_map(process_id);
    session->shell_script = json_object_get_string_member(json_data,"Script");
    session->process = process;

    write_to_script_file(agent, session);
    create_new_shell_process(agent, session);
}