#include <agent-state-unregistered.h>
#include <agent-state.h>
#include <glib.h>
#include <agent-session-initializer.h>
#include <agent-state-on-session.h>
#include <agent-socket.h>
#include <agent-shell-session.h>

#include <state-indicator.h>
#include <general-constant.h>
#include <message-form.h>
#include <error-code.h>
#include <logging.h>

static void
on_open_session_initialize(AgentObject* agent)
{
    AgentState* open_state = transition_to_on_session_state();
    agent_set_state(agent, open_state);
    session_initialize(agent);
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
        GError* error = NULL;
        json_parser_load_from_file(parser, HOST_CONFIG_FILE,&error);
        if(!error == NULL)
        {
            agent_report_error(agent, ERROR_FILE_OPERATION);
        }
        JsonNode* root = json_parser_get_root(parser);
        JsonObject* obj = json_node_get_object(root);
        SlaveID = json_object_get_int_member(obj,DEVICE_ID);
        initialized = TRUE;
    }

    GError* error = NULL;
    Message* object = get_json_object_from_string(message,&error);
    if (!error == NULL || object == NULL) { return; }


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


static void
open_on_shell_process_exit(AgentObject* agent, 
                           gint process_id)
{
    gchar* script = shell_session_get_script(process_id);
    gchar* output = shell_session_get_output(process_id);
    gint id =       shell_session_get_id(process_id);
    gint model =    shell_session_get_model(process_id);
    if(script == NULL || output == NULL) 
    {
        agent_report_error(agent, "fail to get script output");
        return;
    }

    Message* shell = json_object_new();
    json_object_set_string_member(shell, "Output", output);
    json_object_set_string_member(shell, "Script", script);
    json_object_set_int_member(shell, "ID", id);
    json_object_set_int_member(shell, "ModelID", model);


    Message* message = message_init(
        AGENT_MODULE, HOST_MODULE,
        END_SHELL_SESSION, shell);

    agent_send_message(agent, message);
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
        open_state.send_message_to_host = open_state_send_message_to_host;
        open_state.get_current_state = open_get_state;  
        open_state.on_shell_process_exit = open_on_shell_process_exit;


        initialized = TRUE; 
    }
    write_to_log_file(AGENT_GENERAL_LOG, open_get_state());    
    return &open_state;
}