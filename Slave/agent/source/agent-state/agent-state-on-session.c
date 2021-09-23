#include <agent-state-on-session.h>
#include <agent-state.h>
#include <agent-session-initializer.h>
#include <agent-socket.h>
#include <agent-object.h>
#include <agent-state-open.h>
#include <agent-state-on-session-off-remote.h>

#include <state-indicator.h>
#include <logging.h>
#include <general-constant.h>
#include <error-code.h>
#include <message-form.h>



#include <glib.h>

static void
on_session_session_terminate(AgentObject* agent)
{
    GFile* hdl = g_file_parse_name(SESSION_SLAVE_FILE);
    GError* error = NULL;

    g_file_replace_contents(hdl, "EmptySession", strlen("EmptySession"), NULL, TRUE,
        G_FILE_CREATE_NONE, NULL, NULL, &error);
        
    if(!error == NULL)
    {
		agent_report_error(agent,ERROR_FILE_OPERATION);
    }

    AgentState* open_state = transition_to_on_open_state();
    agent_set_state(agent, open_state);
}

static void
on_session_remote_control_disconnect(AgentObject* agent)
{
    // child process will be close automatically after state switch to off remote
    AgentState* off_remote_state = transition_to_off_remote_state();
    agent_set_state(agent,off_remote_state);
}

static void
on_session_send_message_to_host(AgentObject* agent,
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
            agent_report_error(agent, error->message);
        }
        JsonNode* root = json_parser_get_root(parser);
        JsonObject* obj = json_node_get_object(root);
        SlaveID = json_object_get_int_member(obj,DEVICE_ID);
        initialized = TRUE;
    }

    GError* error = NULL;
    Message* object = get_json_object_from_string(message,&error);
	if(!error == NULL || object == NULL) {return;}

    json_object_set_int_member(object,
        DEVICE_ID, SlaveID);

    send_message_to_host(agent,
        get_string_from_json_object(object));
}

static void
on_session_on_shell_process_exit(AgentObject* agent,gint ProcessID)
{
    GError* error_a = NULL, * error_b = NULL;
    JsonObject* obj = get_json_object_from_file(HOST_CONFIG_FILE,&error_a);
    JsonObject* output = get_json_object_from_file(shell_output_map(ProcessID), &error_b);
    if(error_a != NULL|| error_b != NULL)
    {
        agent_report_error(agent, error_a->message);
        agent_report_error(agent, error_b->message);
        return;
    }

    gint SlaveID = json_object_get_int_member(obj, DEVICE_ID);
    gchar* shell_output = get_string_from_json_object(output);


    Message* shell = json_object_new();
    json_object_set_int_member(shell, "ProcessID", ProcessID);
    json_object_set_int_member(shell, "SlaveID", SlaveID);
    json_object_set_string_member(shell, "Output", shell_output);


    Message* message = message_init(
        AGENT_MODULE, HOST_MODULE,
        END_SHELL_SESSION, shell);

    agent_send_message(agent, message);
}

static void
on_session_session_core_exit(AgentObject* agent)
{
    Message* message = 
        empty_message_init(AGENT_MODULE,HOST_MODULE,SESSION_CORE_EXIT);

    agent_send_message(agent,message);
    AgentState* state = transition_to_off_remote_state();
    agent_set_state(agent,state);
}


static gchar* 
on_session_get_state(void)
{
    return AGENT_ON_SESSION;
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
        on_session_state.remote_control_disconnect = on_session_remote_control_disconnect;
        on_session_state.on_session_core_exit = on_session_session_core_exit;
        on_session_state.on_shell_process_exit = on_session_on_shell_process_exit;
        on_session_state.get_current_state = on_session_get_state;

        initialized = TRUE; 
    }
    write_to_log_file(AGENT_GENERAL_LOG,on_session_get_state());
    return &on_session_state;
}