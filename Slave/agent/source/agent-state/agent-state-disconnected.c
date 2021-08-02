#include <agent-state-disconnected.h>
#include <agent-state.h>
#include <glib.h>
#include <agent-socket.h>



void
disconnected_connect_to_host(AgentObject* agent)
{
    connect_to_host_async(agent);
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

        initialized = TRUE; 
    }
    return &disconnected_state;
}