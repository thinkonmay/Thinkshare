#include <agent-state-disconnected.h>
#include <agent-state.h>
#include <glib.h>
#include <agent-socket.h>
#include <agent-object.h>

#include <state-indicator.h>
#include <logging.h>
#include <general-constant.h>
#include <opcode.h>
#include <message-form.h>





static void
disconnected_connect_to_host(AgentObject* agent)
{
    //attemp to connect to host until connection return successfully
    while(TRUE)
    {
        agent_set_socket(initialize_socket(agent));

        g_main_loop_quit(agent_get_main_loop(agent));
        connect_to_host_async(agent);
        agent_set_main_loop(agent,g_main_loop_new(NULL, FALSE));
        Sleep(RECONNECT_INTERVAL);
        g_main_loop_run(agent_get_main_loop(agent));
    }
}


static void
disconnected_register_with_host(AgentObject* agent)
{
    register_with_host(agent);
}

static void
disconnected_send_message_to_host(AgentObject* agent, 
                                  char* message)
{
    GError* error = NULL;

    Message* object = get_json_object_from_string(message,&error);
	if(!error == NULL || object == NULL) {return;}
    gint i = json_object_get_int_member(object, "Opcode");

    if (i != REGISTER_SLAVE)
    {
        write_to_log_file(AGENT_GENERAL_LOG,"Unknown message send to host while not configured");
    }
    else
    {
        send_message_to_host(agent, message);
    }   
}


static gchar* 
disconnected_get_state(void)
{
    return AGENT_DISCONNECTED;
}



AgentState* 
transition_to_disconnected_state(void)
{
    static AgentState disconnected_state;
    static gboolean initialized = FALSE;

    if(!initialized)
    {
        default_method(&disconnected_state);
        disconnected_state.send_message_to_host = disconnected_connect_to_host;
        disconnected_state.register_to_host = disconnected_register_with_host;
        disconnected_state.send_message_to_host = disconnected_send_message_to_host;
        disconnected_state.connect_to_host = disconnected_connect_to_host;
        disconnected_state.get_current_state = disconnected_get_state;

        initialized = TRUE; 
    }
    write_to_log_file(AGENT_GENERAL_LOG,disconnected_get_state());
    return &disconnected_state;
}