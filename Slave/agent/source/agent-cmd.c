#include <agent-cmd.h>
#include <agent-type.h>
#include <agent-message.h>
#include <agent-child-process.h>
#include <agent-socket.h>


#include <logging.h>
#include <general-constant.h>
#include <message-form.h>





void
command_line_output_handle(GBytes* data,
    gint process_id,
    AgentObject* agent)
{
    gchar* message = g_bytes_get_data(data,NULL);

    write_to_log_file(AGENT_CMD_LOG,message);

    Message* object = json_object_new();
    json_object_set_int_member(object, "ProcessID", process_id);
    json_object_set_string_member(object, "Command", message);
    json_object_set_int_member(object, "Time",g_get_real_name());
    
    Message* msg = message_init(AGENT_MODULE, HOST_MODULE,
        COMMAND_LINE_FORWARD, object);
    agent_send_message(agent, msg);
}


void
command_line_process_handle(ChildProcess* proc,
                            DWORD exit_code,
                            AgentObject* agent)
{
    if(exit_code != STILL_ACTIVE)
    {
        
        close_child_process(proc);
    }
}



void
create_new_cmd_process(AgentObject* agent, 
                       gint position)
{

    ChildProcess* child_process = create_new_child_process(
        "C:\\Windows\\System32\\cmd.exe /k ", position, " ",
        command_line_output_handle,NULL, agent);

    agent_set_child_process(agent,position, 
        child_process);
}



void
agent_send_command_line(AgentObject* agent, 
						gchar* command, 
						gint order)
{
    ChildProcess* cmdproc = agent_get_child_process(agent, order);

	//append new line to the end of commandline by copying command to new cmd_with_enter
	gchar* cmd_with_enter = malloc(strlen(command)+1);
	ZeroMemory(cmd_with_enter,strlen(cmd_with_enter));
	strcat(cmd_with_enter,command);
	strcat(cmd_with_enter,"\n");

    // send message to cmd process if it is running
    // otherwise, report error
	if (get_current_child_process_state(agent,order))
	{
	    send_message_to_child_process(cmdproc,
		    cmd_with_enter,strlen(cmd_with_enter));
	}
    else
    {
        agent_report_error(agent,"Foward command to uninitialzed process");
    }
}




















