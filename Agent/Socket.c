#include "Socket.h"


gchar*
get_string_from_json_object(JsonObject* object)
{
    JsonNode* root;
    JsonGenerator* generator;
    gchar* text;

    /* Make it the root node */
    root = json_node_init_object(json_node_alloc(), object);
    generator = json_generator_new();
    json_generator_set_root(generator, root);
    text = json_generator_to_data(generator, NULL);

    /* Release everything */
    g_object_unref(generator);
    json_node_free(root);
    return text;
}

void
connect_to_host_async(AgentObject* self)
{
    SoupLogger* logger;
    SoupMessage* message;
    SoupSession* session;
    const char* https_aliases[] = { "wss", NULL };
    Socket* socket = agent_object_get_socket(self);

    session =
        soup_session_new_with_options(SOUP_SESSION_SSL_STRICT,
            //!disable_ssl,
            SOUP_SESSION_SSL_USE_SYSTEM_CA_FILE, TRUE,
            //SOUP_SESSION_SSL_CA_FILE, "/etc/ssl/certs/ca-bundle.crt",
            SOUP_SESSION_HTTPS_ALIASES, https_aliases, NULL);

    logger = soup_logger_new(SOUP_LOGGER_LOG_BODY, -1);
    soup_session_add_feature(session, SOUP_SESSION_FEATURE(logger));
    g_object_unref(logger);

    message = soup_message_new(SOUP_METHOD_GET, socket->host_url);

    g_print("Connecting to server...\n");

    /* Once connected, we will register */
    soup_session_websocket_connect_async(session,
        message, NULL, NULL, NULL,
        (GAsyncReadyCallback)on_server_connected, self);
    g_object_set_property(self,"agent-state", HOST_CONNECTING);
}


void
on_server_connected(SoupSession* session,
    GAsyncResult* res,
    AgentObject* self)
{
    GError* error = NULL;
    Socket* socket = agent_object_get_socket(self);

    socket->ws = soup_session_websocket_connect_finish(session, res, &error);

    if (error) {
        on_server_closed(error->message, HOST_CONNECTION_ERROR);
        g_error_free(error);
        return;
    }

    g_assert_nonnull(socket->ws);

    g_object_set_property(self, "agent-state", HOST_CONNECTED_OFF_SESSION);
    g_print("Connected to host\n");

    /*connect websocket connection signal with signal handler*/
    g_signal_connect(socket->ws, "closed", G_CALLBACK(on_server_closed), self);
    g_signal_connect(socket->ws, "message", G_CALLBACK(on_server_message), self);


    register_with_server(self);
}

/// <summary>
/// handle close signal from host
/// </summary>
/// <param name="G_GNUC_UNUSED"></param>
/// <param name="G_GNUC_UNUSED"></param>
void
on_server_closed(SoupWebsocketConnection* conn G_GNUC_UNUSED,
    AgentObject* self G_GNUC_UNUSED)
{

}

/// <summary>
/// handle message from host
/// </summary>
/// <param name="conn"></param>
/// <param name="type"></param>
/// <param name="message"></param>
/// <param name="self"></param>
void
on_server_message(SoupWebsocketConnection* conn,
    SoupWebsocketDataType type,
    GBytes* message,
    AgentObject* self)
{
    AgentState* state;
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
}

/*register slave device with host, provide slave information*/
gboolean
register_with_server(AgentObject* self)
{

}

void 
send_message_to_host(AgentObject* self,
    Location from,
    Location to,
    Opcode opcode,
    gpointer data)
{

}


