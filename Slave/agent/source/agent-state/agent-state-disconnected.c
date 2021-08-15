#include <agent-state-disconnected.h>
#include <agent-state.h>
#include <glib.h>
#include <agent-socket.h>

#include <state-indicator.h>
#include <logging.h>
#include <general-constant.h>

static void
disconnected_connect_to_host(AgentObject* agent)
{
    connect_to_host_async(agent);
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
        disconnected_state.connect_to_host = disconnected_connect_to_host;
        disconnected_state.get_current_state = disconnected_get_state;

        initialized = TRUE; 
    }
    write_to_log_file(AGENT_GENERAL_LOG,disconnected_get_state());
    return &disconnected_state;
}