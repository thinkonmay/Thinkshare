

#include <agent-cmd.h>
#include <agent-type.h>
#include <agent-message.h>
#include <agent-child-process.h>
#include <agent-socket.h>


#include <logging.h>
#include <general-constant.h>





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
create_new_cmd_process(AgentObject* agent, 
                       gint position,
                       gchar* first_command)
{

    ChildProcess* child_process = create_new_child_process(
        "C:\\Windows\\System32\\cmd.exe /k ", position, first_command,
        command_line_output_handle, agent);

    agent_set_child_process(agent,position, 
        child_process);
}



















