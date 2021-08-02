#include <agent-cmd.h>
#include <agent-type.h>
#include <agent-message.h>
#include <agent-child-process.h>
#include <agent-socket.h>








void
command_line_output_handle(GBytes* data,
    gint process_id,
    AgentObject* agent)
{
    gchar* message = g_bytes_get_data(data,NULL);
    g_print(message);
    Message* object = json_object_new();
    json_object_set_int_member(object, "\nProcessID", process_id);
    json_object_set_string_member(object, "Command", message);

    //Message* msg = message_init(AGENT_MODULE, HOST_MODULE,
    //    COMMAND_LINE_FORWARD, object);

    //g_print(get_string_from_json_object(msg));

    //agent_send_message(agent, msg);
}



void
create_new_cmd_process(gint position, 
    AgentObject* agent, 
    gchar** first_command)
{

    ChildProcess* child_process = create_new_child_process(
        "C:\\Windows\\System32\\cmd.exe /k ", position, first_command,
        command_line_output_handle, &agent);

   agent_set_child_process(agent,position, 
        child_process);
}



















