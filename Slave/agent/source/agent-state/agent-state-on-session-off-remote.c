#include <agent-state-on-session-off-remote.h>
#include <agent-state.h>
#include <glib.h>
#include <agent-session-initializer.h>
#include <agent-state-open.h>
#include <agent-state-on-session.h>
#include <agent-socket.h>


#include <state-indicator.h>
#include <general-constant.h>
#include <logging.h>



static void
off_remote_session_terminate(AgentObject* agent)
{
    GFile* hdl = g_file_parse_name(SESSION_SLAVE_FILE);

    g_file_replace_contents(hdl, "EmptySession", sizeof("EmptySession"), NULL, TRUE,
        G_FILE_CREATE_NONE, NULL, NULL, NULL);

    AgentState* open_state = transition_to_on_open_state();
    agent_set_state(agent, open_state);
}


static void
off_remote_send_message_to_host(AgentObject* agent,
    gchar* message)
{
    static gboolean initialized = FALSE;
    static gint SlaveID;
    if(!initialized)
    {
        JsonParser* parser = json_parser_new();
        GError* error = NULL;
        json_parser_load_from_file(parser, HOST_CONFIG_FILE,&error);
        if(error != NULL)
        {
            //
        }
        JsonNode* root = json_parser_get_root(parser);
        JsonObject* obj = json_node_get_object(root);
        SlaveID = json_object_get_int_member(obj,DEVICE_ID);
        initialized = TRUE;
    }


    JsonParser* parser = json_parser_new();
    json_parser_load_from_data(parser, message, -1, NULL);
    JsonNode* root = json_parser_get_root(parser);
    JsonObject* object = json_node_get_object(root);




    json_object_set_int_member(object,
        DEVICE_ID, SlaveID);

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


static gchar* 
on_session_off_remote_get_state(void)
{
    return AGENT_ON_SESSION_OFF_REMOTE;
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
        off_remote_state.send_message_to_session_core = send_message_to_core;
        off_remote_state.get_current_state = on_session_off_remote_get_state;

        initialized = TRUE; 
    }
    write_to_log_file(AGENT_GENERAL_LOG,on_session_off_remote_get_state());
    return &off_remote_state;
} 