#include <agent-socket.h>
#include <agent-type.h>
#include <agent-session-initializer.h>
#include <agent-message.h>
#include <agent-device.h>
#include <agent-state-disconnected.h>
#include <agent-state-open.h>
#include <agent-object.h>

#include <error-code.h>
#include <logging.h>
#include <general-constant.h>

#include <glib-object.h>
#include <json-glib/json-glib.h>
#include <libsoup/soup.h>

/// <summary>
/// contain information about websocket socket with host
/// </summary>
struct _Socket
{
    SoupWebsocketConnection* ws;

    SoupSession* session;

    SoupLogger* logger;

    SoupMessage* message;
};





void 
send_message_to_host(AgentObject* object,
                     gchar* message)
{
    Socket* socket = agent_get_socket(object);
    soup_websocket_connection_send_text(socket->ws, message);
}





gchar*
get_string_from_json_object(JsonObject* object)
{
    JsonNode* root;
    JsonGenerator* generator;
    gchar* text;

    /* Make it the root node */
    root =      json_node_init_object(json_node_alloc(), object);
    generator = json_generator_new();
    json_generator_set_root(generator, root);
    text =      json_generator_to_data(generator, NULL);

    /* Release everything */
    g_object_unref(generator);
    json_node_free(root);
    return text;
}


void
socket_close(Socket* socket)
{
    write_to_log_file(AGENT_NETWORK_LOG, "socket closed");

    if (socket->ws != NULL)
    {
        if (soup_websocket_connection_get_state(socket->ws) == SOUP_WEBSOCKET_STATE_OPEN)
        {
            soup_websocket_connection_close(socket->ws, 1000, "");
        }
        else
        {
            g_object_unref(socket->ws);
        }
    }
    ZeroMemory(socket,sizeof(Socket));
}

/// <summary>
/// handle close signal from host, 
/// terminate websocket connection then try to reconnect
/// </summary>
/// <param name="websocket connection"></param>
/// <param name=""></param>
void
on_server_closed(SoupWebsocketConnection* conn,
    AgentObject* agent)
{
    /*close websocket connection*/
    Socket* socket = agent_get_socket(agent);
    socket_close(socket);
    
    AgentState* disconnected = transition_to_disconnected_state();
    agent_set_state(agent, disconnected);

    /*then attemp to reconnect*/
    agent_set_socket(agent,initialize_socket(agent));	
	agent_connect_to_host(agent);
}

/// <summary>
/// on server connected function, 
/// callback function invoke when websocket connection has been established
/// </summary>
/// <param name="session"></param>
/// <param name="res"></param>
/// <param name="self"></param>
void
on_server_connected(SoupSession* session,
    GAsyncResult* res,
    AgentObject* agent)
{
    GError* error = NULL;
    Socket* socket = agent_get_socket(agent);

    socket->ws = soup_session_websocket_connect_finish(session, res, &error);

    /*if error happen during connection, restart agent_connect_to_host*/
    if (error)
    {
        write_to_log_file(AGENT_NETWORK_LOG,error->message);
        return;
    }
    g_main_context_push_thread_default(g_main_loop_get_context(agent_get_main_loop(agent)));

    /*connect websocket connection signal with signal handler*/
    g_signal_connect(socket->ws, "closed", G_CALLBACK(on_server_closed), agent);
    g_signal_connect(socket->ws, "message", G_CALLBACK(on_server_message), agent);

    g_main_context_pop_thread_default(g_main_loop_get_context(agent_get_main_loop(agent)));



    /*after establish websocket connection with host, perform register procedure*/
    agent_register_with_host(agent);
}


void
connect_to_host_async(AgentObject* self)
{
    Socket* socket = 
        agent_get_socket(self);

    write_to_log_file(AGENT_GENERAL_LOG,"Connecting to server");

    /* Once connected, we will register */
    soup_session_websocket_connect_async(socket->session,
        socket->message, NULL, NULL, NULL,
        (GAsyncReadyCallback)on_server_connected, self);
}




void
on_server_message(SoupWebsocketConnection* conn,
                    SoupWebsocketDataType type,
                    GBytes* message,
                    AgentObject* self)
{
    gchar* text = "ERROR";
    switch (type) 
    {
    case SOUP_WEBSOCKET_DATA_BINARY:
        write_to_log_file(AGENT_MESSAGE_LOG,"Received unknown binary message, ignoring\n");
        return;
    case SOUP_WEBSOCKET_DATA_TEXT: 
    {
        gsize size;
        const gchar* data = g_bytes_get_data(message, &size);
        /* Convert to NULL-terminated string */
        text = g_strndup(data, size);
        break;
    }
    default:
        write_to_log_file(AGENT_MESSAGE_LOG,"Received unknown binary message, ignoring\n");
        return;
    }
    on_agent_message(self, text);
    g_free(text);
}




gboolean
register_with_host(AgentObject* agent)
{
    JsonParser* parser = json_parser_new();

    GError* error =NULL;
    json_parser_load_from_file(parser,HOST_CONFIG_FILE,&error);

    if(error != NULL)
    {
        JsonObject* object = json_object_new();
        json_object_set_string_member(object,UNDEFINED_ERROR,error->message);

        agent_send_message(agent,
            message_init(AGENT_MODULE,HOST_MODULE,ERROR_REPORT,object));
    }

    JsonNode* root = json_parser_get_root(parser);
    JsonObject* object = json_node_get_object(root);

    gint ID = json_object_get_int_member(object, DEVICE_ID);

    write_to_log_file(AGENT_GENERAL_LOG,"Registering with host");
    Message* package =
        message_init(AGENT_MODULE, HOST_MODULE, 
            REGISTER_SLAVE, get_registration_message(ID));

    agent_send_message(agent, package); 
    return TRUE;     
}






/*START get-set-function for Socket*/

SoupWebsocketConnection*
socket_get_connection(Socket* socket)
{
    socket->ws;
}

gchar*
socket_get_host_url(AgentObject* agent)
{
    JsonParser* parser = json_parser_new();

    GError* error = NULL;
    json_parser_load_from_file(parser,HOST_CONFIG_FILE,error);
    if(error != NULL)
    {
        agent_report_error(agent, error->message);
    }
    JsonNode* root = json_parser_get_root(parser);
    JsonObject* obj = json_node_get_object(root);

    return json_object_get_string_member(obj,HOST_URL);
}




void
agent_logger(SoupLogger* logger,
            SoupLoggerLogLevel  level,
            char                direction,
            const char         *data,
            gpointer            user_data)
{
    write_to_log_file(AGENT_NETWORK_LOG,data);
}



Socket*
initialize_socket(AgentObject* agent)
{

    const gchar* https_aliases[] = { "ws", NULL };
    static Socket socket;
    ZeroMemory(&socket,sizeof(Socket));

    JsonParser* parser = json_parser_new();

    GError* error = NULL;
    json_parser_load_from_file(parser,HOST_CONFIG_FILE,&error);
    if(error != NULL)
    {
        JsonObject* object = json_object_new();
        json_object_set_string_member(object,UNDEFINED_ERROR,error->message);
        write_to_log_file(AGENT_GENERAL_LOG,get_string_from_json_object(object));
        return;
    }

    JsonNode* root = json_parser_get_root(parser);
    JsonObject* obj = json_node_get_object(root);


    socket.session =
        soup_session_new_with_options(
            SOUP_SESSION_SSL_STRICT, !json_object_get_boolean_member(obj,DISABLE_SSL),
            SOUP_SESSION_SSL_USE_SYSTEM_CA_FILE, TRUE,
            //SOUP_SESSION_SSL_CA_FILE, "/etc/ssl/certs/ca-bundle.crt",
            SOUP_SESSION_HTTPS_ALIASES, https_aliases, NULL);

    socket.logger = soup_logger_new(SOUP_LOGGER_LOG_BODY, -1);
    soup_session_add_feature(socket.session, SOUP_SESSION_FEATURE(socket.logger));
    g_object_unref(socket.logger);

    //soup_logger_set_printer(socket.logger,agent_logger,NULL,NULL);

    socket.message = soup_message_new(SOUP_METHOD_GET,
        json_object_get_string_member(obj,HOST_URL));

    return &socket;
}
