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


static void
open_state_send_message_to_host(AgentObject* agent,
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