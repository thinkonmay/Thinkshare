#include <agent-state-unregistered.h>
#include <agent-state.h>
#include <glib.h>
#include <agent-socket.h>
#include <opcode.h>


#include <logging.h>
#include <general-constant.h>
#include <state-indicator.h>
#include <message-form.h>
#include <opcode.h>

static void
unregistered_connect_to_host(AgentObject* agent)
{
    connect_to_host_async(agent);
}

static void
unregistered_register_with_host(AgentObject* agent)
{
    register_with_host(agent);
}

static void
unregistered_send_message_to_host(AgentObject* agent, char* message)
{
    GError* error = NULL;
    JsonParser* parser = json_parser_new();
    Message* object = get_json_object_from_string(message,&error,parser);
	if(!error == NULL || object == NULL) {return;}
    gint i= json_object_get_int_member(object, "Opcode");

    if (i == REGISTER_SLAVE)
    { 
        send_message_to_host(agent, message); 
        return; 
    }
    
    write_to_log_file(AGENT_GENERAL_LOG,"Unknown message send to host while not configured");
    g_object_unref(parser);
    return;
}


static gchar* 
unregistered_get_state(void)
{
    return AGENT_UNREGISTERED;
}

AgentState* 
transition_to_unregistered_state(void)
{
    static AgentState unregistered_state;
    static gboolean initialized = FALSE;

    if(!initialized)
    {
        default_method(&unregistered_state);

        unregistered_state.connect_to_host = unregistered_connect_to_host;
        unregistered_state.register_to_host = unregistered_register_with_host;
        unregistered_state.send_message_to_host = unregistered_send_message_to_host;
        unregistered_state.session_terminate = unregistered_connect_to_host;
        unregistered_state.get_current_state = unregistered_get_state;

        initialized = TRUE; 
    }
    
    write_to_log_file(AGENT_GENERAL_LOG,unregistered_get_state());
    return &unregistered_state;
}