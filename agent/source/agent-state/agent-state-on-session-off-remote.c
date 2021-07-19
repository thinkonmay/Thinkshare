#include <agent-state-on-session-off-remote.h>
#include <agent-state.h>
#include <glib.h>
#include <agent-ipc.h>
#include <agent-state-open.h>
#include <agent-state-on-session.h>
#include <agent-socket.h>

static void
off_remote_session_terminate(AgentObject* agent)
{
    g_free(agent_get_session(agent));
    agent_set_session(agent, NULL);

    AgentState* open_state = transition_to_on_open_state();
    agent_set_state(agent, open_state);
}

static void
off_remote_remote_control_reconnect(AgentObject* agent)
{
    session_initialize(agent);

    AgentState* on_session = transition_to_on_session_state();
    agent_set_state(agent, on_session);
}

AgentState*
transition_to_off_remote_state(void)
{
    static AgentState off_remote_state;
    static gboolean initialized = FALSE;

    if(!initialized)
    {
        default_method(&off_remote_state);
        off_remote_state.session_terminate = off_remote_session_terminate;
        off_remote_state.remote_control_reconnect = off_remote_remote_control_reconnect;
        off_remote_state.send_message_to_host = send_message_to_host;
  //      off_remote_state.send_message_to_session_loader = send_message_to_loader;
        off_remote_state.send_message_to_session_core = send_message_to_core;
        initialized = TRUE; 
    }
    return &off_remote_state;
} 