#include <agent-session-initializer.h>
#include <agent-state-on-session-off-remote.h>
#include <agent-socket.h>
#include <agent-type.h>
#include <agent-message.h>
#include <agent-child-process.h>
#include <state-indicator.h>

#include <general-constant.h>
#include <child-process-constant.h>

#include <gmodule.h>
#include <Windows.h>
#include <stdio.h>

#define BUFFER_SIZE 10000


void
handler_session_core_state_function(ChildProcess* proc,
                                    DWORD exit_code, 
                                    AgentObject* agent)
{
        /*
        *if child process terminated is session core
        *let agent handle that
        */
        if (exit_code != STILL_ACTIVE)
        {
            agent_on_session_core_exit(agent);
            //switch to off remote state if session-core terminate during session
            if(!g_strcmp0(agent_get_current_state_string(agent) , AGENT_ON_SESSION))
            {
                AgentState* off_remote = transition_to_off_remote_state();
                agent_set_state(agent,off_remote);
                close_child_process(proc);
            }
            return;
        }

        /*
        *if child process is session core, check for current state of agent,
        *Terminate process if agent is not in session,
        */
    
        if(g_strcmp0(agent_get_current_state_string(agent) , AGENT_ON_SESSION))
        {
            close_child_process(proc);
            return;
        }
        
}



void
handle_session_core_function(GBytes* buffer,
    gint process_id,
    AgentObject* agent)
{
    gchar* message = g_bytes_get_data(buffer, NULL);
    on_agent_message(agent, message);
}


gboolean
session_terminate(AgentObject* agent)
{
    close_child_process(
        agent_get_child_process(agent, SESSION_CORE_PROCESS_ID));
}

gboolean
session_initialize(AgentObject* agent)
{
    ChildProcess* session_core = 
    create_new_child_process(SESSION_CORE_BINARY,
        SESSION_CORE_PROCESS_ID, " ", 
        (ChildStdHandle)handle_session_core_function,
        (ChildStateHandle)handler_session_core_state_function, agent);
    agent_set_child_process(agent, SESSION_CORE_PROCESS_ID, session_core);
}

gboolean
send_message_to_core(AgentObject* self, gchar* buffer)
{
    send_message_to_child_process(
        agent_get_child_process(self, SESSION_CORE_PROCESS_ID), 
            buffer, strlen(buffer) * sizeof(gchar));
}