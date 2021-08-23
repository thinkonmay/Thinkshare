#include <agent-state-unregistered.h>
#include <agent-state.h>
#include <glib.h>
#include <agent-session-initializer.h>
#include <agent-state-on-session.h>
#include <agent-socket.h>


#include <state-indicator.h>
#include <general-constant.h>
#include <message-form.h>
#include <error-code.h>
#include <logging.h>

static void
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
    static gboolean initialized = FALSE;
    static gint SlaveID;
    if(!initialized)
    {
        JsonParser* parser = json_parser_new();
        GError* error = malloc(sizeof(GError));
        json_parser_load_from_file(parser, HOST_CONFIG_FILE,&error);
        if(error != NULL)
        {
            agent_report_error(agent, ERROR_FILE_OPERATION);
        }
        JsonNode* root = json_parser_get_root(parser);
        JsonObject* obj = json_node_get_object(root);
        SlaveID = json_object_get_int_member(obj,DEVICE_ID);
        initialized = TRUE;
    }

    GError* error = malloc(sizeof(GError));
    Message* object = get_json_object_from_string(message,&error);
    if (error != NULL || object == NULL) { return; }


    json_object_set_int_member(object,
        DEVICE_ID, SlaveID);

    send_message_to_host(agent,
        get_string_from_json_object(object));
}

static gchar*
open_get_state(void)
{
    return AGENT_OPEN;
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
        open_state.get_current_state = open_get_state;      

        initialized = TRUE; 
    }
    write_to_log_file(AGENT_GENERAL_LOG, open_get_state());    
    return &open_state;
}