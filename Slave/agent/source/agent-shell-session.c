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


   



gchar*
shell_script_map(gint process_id)
{
    switch(process_id)
    {
        case 2:
            return SHELL_SCRIPT_BUFFER_2;
        case 3:
            return SHELL_SCRIPT_BUFFER_3;
        case 4:
            return SHELL_SCRIPT_BUFFER_4;
        case 5:
            return SHELL_SCRIPT_BUFFER_5;
        case 6:
            return SHELL_SCRIPT_BUFFER_6;
        case 7:
            return SHELL_SCRIPT_BUFFER_7;
        case 8:
            return SHELL_SCRIPT_BUFFER_8;
    }
}

gchar*
shell_output_map(gint process_id)
{
    switch(process_id)
    {
        case 2:
            return SHELL_OUTPUT_BUFFER_2;
        case 3:
            return SHELL_OUTPUT_BUFFER_3;
        case 4:
            return SHELL_OUTPUT_BUFFER_4;
        case 5:
            return SHELL_OUTPUT_BUFFER_5;
        case 6:
            return SHELL_OUTPUT_BUFFER_6;
        case 7:
            return SHELL_OUTPUT_BUFFER_7;
        case 8:
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



void
create_new_shell_process(AgentObject* agent, 
                         gint position)
{
    GString* string = g_string_new(shell_script_map(position));
    g_string_append(string, " > ");
    g_string_append(string, shell_output_map(position));

    ChildProcess* child_process = create_new_child_process(
        "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe ", 
            position, g_string_free(string,TRUE),
                shell_output_handle,
                shell_process_handle, agent);

    agent_set_child_process(agent,position, 
        child_process);
}



















