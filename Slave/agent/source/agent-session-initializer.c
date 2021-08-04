#include <agent-session-initializer.h>
#include <agent-socket.h>
#include <agent-type.h>
#include <agent-message.h>
#include <agent-child-process.h>

#include <general-constant.h>
#include <child-process-constant.h>

#include <gmodule.h>
#include <Windows.h>
#include <stdio.h>

#define BUFFER_SIZE 10000






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
        SESSION_CORE_PROCESS_ID, NULL,(ChildHandleFunc)handle_session_core_function, agent);
    agent_set_child_process(agent, SESSION_CORE_PROCESS_ID, session_core);
}

gboolean
send_message_to_core(AgentObject* self, gchar* buffer)
{
    send_message_to_child_process(
        agent_get_child_process(self, SESSION_CORE_PROCESS_ID), 
            buffer, strlen(buffer) * sizeof(gchar));
}