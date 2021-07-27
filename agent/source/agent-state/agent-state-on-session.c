#include <agent-state-on-session.h>
#include <agent-state.h>
#include <glib.h>
#include <agent-ipc.h>
#include <agent-socket.h>
#include <agent-state-open.h>
#include <agent-state-on-session-off-remote.h>





void
on_session_session_terminate(AgentObject* agent)
{
    session_terminate(agent);
    g_free(agent_get_session(agent));
    agent_set_session(agent, NULL);

    AgentState* open_state = transition_to_on_open_state();
    agent_set_state(agent, open_state);
}

void
on_session_remote_control_disconnect(AgentObject* agent)
{
    session_terminate(agent);

    AgentState* off_remote_state = transition_to_off_remote_state();
    agent_set_state(agent,off_remote_state);
}

static void
on_session_send_message_to_host(AgentObject* agent,
    gchar* message)
{
    Socket* socket = agent_get_socket(agent);
    JsonNode* root;
    JsonObject* json_data;

    JsonParser* parser = json_parser_new();
    json_parser_load_from_data(parser, message, -1, NULL);
    root = json_parser_get_root(parser);
    JsonObject* object = json_node_get_object(root);

    GFile* file = agent_get_slave_id(agent);

    GBytes* bytes = g_file_load_bytes(file, NULL, NULL, NULL);

    gchar* buffer = g_bytes_get_data(bytes, NULL);


    json_object_set_int_member(object,
        "SlaveID", atoi(buffer));

    send_message_to_host(agent,
        get_string_from_json_object(object));
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
        on_session_state.send_message_to_session_loader = send_message_to_loader;
        on_session_state.remote_control_disconnect = on_session_remote_control_disconnect;
        initialized = TRUE; 
    }
    return &on_session_state;
}