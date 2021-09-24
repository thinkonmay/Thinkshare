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
        agent_on_shell_process_terminate(agent,
            get_child_process_id(proc));
        close_child_process(proc);
    }
}



static void
create_new_shell_process(AgentObject* agent, 
                         gint process_id)
{
    GString* string = g_string_new(shell_script_map(process_id));
    g_string_append(string, " | out-file ");
    g_string_append(string, shell_output_map(process_id));
    g_string_append(string," -encoding utf8");
    gchar* command = g_string_free(string,FALSE);

    ChildProcess* child_process = create_new_child_process(
        "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe ", 
            process_id, command,
                shell_output_handle,
                shell_process_handle, agent);

}

static void 
write_to_script_file(AgentObject* agent, gint process_id, gchar* powershell_command)
{
    GFile* file = g_file_parse_name(shell_script_map(process_id));
    if(!g_file_replace_contents(file, powershell_command,strlen(powershell_command),
        NULL,FALSE,G_FILE_CREATE_REPLACE_DESTINATION,NULL,NULL, NULL,NULL))
    {
        agent_report_error(agent,ERROR_FILE_OPERATION);					
    }

    create_new_shell_process(agent,process_id);
    return;
}

void
initialize_shell_session(AgentObject* agent,
                         gchar* data_string)
{
    GError* error = NULL;
    Message* json_data = get_json_object_from_string(data_string,&error);
    if(!error == NULL) {return;}

    gint a = json_object_get_int_member(json_data, "ProcessID");
    gchar* b = json_object_get_string_member(json_data,"Script");

    gint process_id =  6;
    gchar* powershell_command = "hello";
    
    write_to_script_file(agent,process_id,powershell_command);
}


















