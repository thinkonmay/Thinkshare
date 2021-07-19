#include <agent-state-on-session.h>
#include <agent-state.h>
#include <glib.h>
#include <agent-ipc.h>
#include <agent-socket.h>
#include <agent-state-open.h>
#include <agent-state-on-session-off-remote.h>





static void
on_session_session_terminate(AgentObject* agent)
{
    session_terminate(agent);
    g_free(agent_get_session(agent));
    agent_set_session(agent, NULL);

    AgentState* open_state = transition_to_on_open_state();
    agent_set_state(agent, open_state);
}

static void
on_session_remote_control_disconnect(AgentObject* agent)
{
    session_terminate(agent);

    AgentState* off_remote_state = transition_to_off_remote_state();
    agent_set_state(agent,off_remote_state);
}



AgentState*
transition_to_on_session_state(void)
{
    static AgentState on_session_state;
    static gboolean initialized = FALSE;

    if(!initialized)
    {
        default_method(&on_session_state);
        on_session_state.session_terminate = on_session_session_terminate;
        on_session_state.send_message_to_host = send_message_to_host;
        on_session_state.send_message_to_session_core = send_message_to_core;
//        on_session_state.send_message_to_session_loader = send_message_to_loader;
        on_session_state.remote_control_disconnect = on_session_remote_control_disconnect;
        initialized = TRUE; 
    }
    return &on_session_state;
}