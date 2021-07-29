#include <agent-session-initializer.h>
#include <agent-socket.h>
#include <agent-type.h>
#include <agent-message.h>
#include <agent-child-process.h>


#include <gmodule.h>
#include <Windows.h>
#include <stdio.h>

#define SESSION_CORE_NAME  "SessionCore.exe"

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
        agent_get_child_process(agent, 0));
}

gboolean
session_initialize(AgentObject* object)
{
    ChildProcess* session_core = create_new_child_process("D:\\OneDrive - VINACADEMY LLC\\Desktop\\personal-cloud-computing\\bin\\SessionCore.exe", 0, NULL,
        (ChildHandleFunc)handle_session_core_function, object);
    agent_set_child_process(object, 0, session_core);
}

gboolean
send_message_to_core(AgentObject* self, gchar* buffer)
{
    send_message_to_child_process(
        agent_get_child_process(self, 0), 
            buffer, strlen(buffer) * sizeof(gchar));
}