#include <agent-state-unregistered.h>
#include <agent-state.h>
#include <glib.h>
#include <agent-ipc.h>
#include <agent-state-on-session.h>
#include <agent-socket.h>

void
on_open_session_initialize(AgentObject* agent)
{
    session_initialize(agent);

    AgentState* open_state = transition_to_on_session_state();
    agent_set_state(agent, open_state);
}


AgentState* 
transition_to_on_open_state(void)
{
    static AgentState open_state;
    static gboolean initialized = FALSE;

    if(!initialized)
    {
        default_method(&open_state);
        open_state.session_initialize = on_open_session_initialize;
        open_state.send_message_to_host = send_message_to_host;

        

        initialized = TRUE; 
    }
    return &open_state;
}