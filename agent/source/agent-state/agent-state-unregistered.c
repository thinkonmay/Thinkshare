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

static void
unregistered_send_message_to_host(AgentObject* agent, char* message)
{
    JsonNode* root;
    JsonObject* object, * json_data;

    JsonParser* parser = json_parser_new();
    json_parser_load_from_data(parser, message, -1, NULL);
    root = json_parser_get_root(parser);
    object = json_node_get_object(root);

    int i= json_object_get_int_member(object, "Opcode");
    if (i != REGISTER_SLAVE)
        g_print("We haven't registered yet \n");
    else
        send_message_to_host(agent, message);
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
<<<<<<< Updated upstream
        unregistered_state.send_message_to_host = send_message_to_host;
=======
        unregistered_state.send_message_to_host = unregistered_send_message_to_host;
        unregistered_state.session_terminate = unregistered_connect_to_host;
>>>>>>> Stashed changes

        initialized = TRUE; 
    }
    return &unregistered_state;
}