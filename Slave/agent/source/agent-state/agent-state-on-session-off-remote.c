#include <agent-state-on-session-off-remote.h>
#include <agent-state.h>
#include <glib.h>
#include <agent-session-initializer.h>
#include <agent-state-open.h>
#include <agent-state-on-session.h>
#include <agent-socket.h>

static void
off_remote_session_terminate(AgentObject* agent)
{
    GFile* hdl = agent_get_session(agent);

    g_file_replace_contents(hdl, "EmptySession", sizeof("EmptySession"), NULL, TRUE,
        G_FILE_CREATE_NONE, NULL, NULL, NULL);

    AgentState* open_state = transition_to_on_open_state();
    agent_set_state(agent, open_state);
}


static void
off_remote_send_message_to_host(AgentObject* agent,
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
        //off_remote_state.send_message_to_session_loader = send_message_to_loader;
        off_remote_state.send_message_to_session_core = send_message_to_core;
        initialized = TRUE; 
    }
    return &off_remote_state;
} 