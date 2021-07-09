#include <agent-socket.h>
#include <agent-type.h>
#include <agent-ipc.h>
#include <agent-message.h>

#include <json-glib/json-glib.h>
#include <agent-object.h>
#include <agent-object.h>
#include <glib-object.h>
#include <libsoup/soup.h>

/// <summary>
/// contain information about websocket socket with host
/// </summary>
struct _Socket
{
    SoupWebsocketConnection* ws;

    gchar* host_url;
};





void 
send_message_to_host(AgentObject* object,
                     gchar* message)
{
    if (agent_get_state(object) == ATTEMP_TO_RECONNECT || agent_get_state(object) == AGENT_CLOSED)
    {
        g_printerr("not connected to host");
        return;
    }
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
    g_print("socket closed");

    if (socket->ws)
    {
        if (soup_websocket_connection_get_state(socket->ws) == SOUP_WEBSOCKET_STATE_OPEN)
        {
            soup_websocket_connection_close(socket->ws, 1000, "");
        }
        else
            g_object_unref(socket->ws);
    }
}

/// <summary>
/// handle close signal from host, terminate websocket connection then try to reconnect
/// </summary>
/// <param name="websocket connection"></param>
/// <param name=""></param>
void
on_server_closed(SoupWebsocketConnection* conn,
    AgentObject* self)
{
    /*close websocket connection*/
    Socket* socket = agent_get_socket(self);
    socket_close(socket);
    /*then attemp to reconnect*/
    agent_connect_to_host(self);
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
    AgentObject* self)
{
    GError* error = NULL;
    Socket* socket = agent_get_socket(self);

    socket->ws = soup_session_websocket_connect_finish(session, res, &error);

    /*if error happen during connection, restart agent_connect_to_host*/
    if (error) 
    {
        on_server_closed(socket, self);
        g_error_free(error);
        return;
    }

    g_assert_nonnull(socket->ws);

    g_print("Connected to host\n");

    /*connect websocket connection signal with signal handler*/
    g_signal_connect(socket->ws, "closed", G_CALLBACK(on_server_closed), self);
    g_signal_connect(socket->ws, "message", G_CALLBACK(on_server_message), self);
    
    agent_set_state(self, SLAVE_REGISTERING);

    /*after establish websocket connection with host, perform register procedure*/
    agent_register_with_host(self);
}

void
connect_to_host_async(AgentObject* self)
{
    /*currently there is some bug of soup logger related to write access violation*/
    ///SoupLogger* logger;
    SoupMessage* message;
    SoupSession* session;
    const gchar *https_aliases[] = { "wss", NULL };
    Socket* socket = agent_get_socket(self);

    ///logger = soup_logger_new(SOUP_LOGGER_LOG_BODY, -1);

    session =   soup_session_new_with_options(SOUP_SESSION_SSL_USE_SYSTEM_CA_FILE, TRUE,
                ///SOUP_SESSION_ADD_FEATURE, logger,
                //SOUP_SESSION_SSL_CA_FILE, "/etc/ssl/certs/ca-bundle.crt",
                SOUP_SESSION_HTTPS_ALIASES, https_aliases);

    



    message =   soup_message_new(SOUP_METHOD_GET, socket->host_url);

    g_print("Connecting to server...\n");

    /* Once connected, we will register */
    soup_session_websocket_connect_async(session,
        message, NULL, NULL, NULL,
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
        g_printerr("Received unknown binary message, ignoring\n");
        return;
    case SOUP_WEBSOCKET_DATA_TEXT: 
    {
        gsize size;
        const char* data = g_bytes_get_data(message, &size);
        /* Convert to NULL-terminated string */
        text = g_strndup(data, size);
        break;
    }
    default:
        g_assert_not_reached();
    }
    on_agent_message(self, text);
}

gpointer 
update_device_with_host(AgentObject* data)
{
	while(TRUE)
	{

        Message* msg = get_json_message_from_device(data);

        message_init( AGENT_MODULE, HOST_MODULE,
            UPDATE_SLAVE_STATE, msg, sizeof(msg));
		agent_send_message(data,msg);

		Sleep(10);

        if (agent_get_state(data) == AGENT_CLOSED || agent_get_state(data) == ATTEMP_TO_RECONNECT)
            break;
	}

    return NULL;
}


gboolean
register_with_host(AgentObject* self)
{
    DeviceInformation* infor = agent_get_device_information(self);
    JsonObject* json;

    Message* message = 
        get_json_message_from_device_information(infor);

    Message* package =
        message_init(AGENT_MODULE, HOST_MODULE, 
            REGISTER_SLAVE,message, sizeof(message));

    agent_send_message(self, package); 
    return TRUE;     
}






/*START get-set-function for Socket*/

SoupWebsocketConnection*
socket_get_connection(Socket* socket)
{
    socket->ws;
}

gchar*
socket_get_host_url(Socket* socket)
{
    socket->host_url;
}

Socket*
initialize_socket(gchar* host_url)
{
    Socket* socket = malloc(sizeof(Socket));
    socket->host_url = host_url;
    return socket;
}

/*END get-set-function for Socket*/
