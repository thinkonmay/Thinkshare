#include <agent-state-unregistered.h>
#include <agent-state.h>
#include <glib.h>
#include <agent-socket.h>



void
unregistered_connect_to_host(AgentObject* agent)
{
    static gint i = 0;

    if (i == 5)
        agent_finalize(agent);

    connect_to_host_async(agent);
    Sleep(1000);
    i++;
}

void
unregistered_register_with_host(AgentObject* agent)
{
    register_with_host(agent);
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
        unregistered_state.send_message_to_host = send_message_to_host;

        initialized = TRUE; 
    }
    return &unregistered_state;
}