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

    session =
        soup_session_new_with_options(SOUP_SESSION_SSL_STRICT,
            !disable_ssl,
            SOUP_SESSION_SSL_USE_SYSTEM_CA_FILE, TRUE,
            //SOUP_SESSION_SSL_CA_FILE, "/etc/ssl/certs/ca-bundle.crt",
            SOUP_SESSION_HTTPS_ALIASES, https_aliases, NULL);

    logger = soup_logger_new(SOUP_LOGGER_LOG_BODY, -1);
    soup_session_add_feature(session, SOUP_SESSION_FEATURE(logger));
    g_object_unref(logger);

    message = soup_message_new(SOUP_METHOD_GET, Host_URL);

    g_print("Connecting to server...\n");

    /* Once connected, we will register */
    soup_session_websocket_connect_async(session,
        message, NULL, NULL, NULL,
        (GAsyncReadyCallback)on_server_connected, message);
    g_object_set_property(self,"agent-state", HOST_CONNECTING);
}


void
on_server_connected(SoupSession* session,
    GAsyncResult* res,
    SoupMessage* msg)
{
    GError* error = NULL;

    ws_conn = soup_session_websocket_connect_finish(session, res, &error);
    if (error) {
        on_server_disconnect(error->message, HOST_CONNECTION_ERROR);
        g_error_free(error);
        return;
    }

    g_assert_nonnull(ws_conn);

    agent_state = HOST_CONNECTED_OFF_SESSION;
    g_print("Connected to signalling server\n");

    g_signal_connect(ws_conn, "closed", G_CALLBACK(on_server_closed), NULL);
    g_signal_connect(ws_conn, "message", G_CALLBACK(on_server_message), NULL);
}

void
on_server_closed(SoupWebsocketConnection* conn G_GNUC_UNUSED,
    gpointer user_data G_GNUC_UNUSED)
{

}

void
on_server_message(SoupWebsocketConnection* conn,
    SoupWebsocketDataType type,
    GBytes* message,
    gpointer user_data)
{

}

/*register slave device with host, provide slave information*/
gboolean
register_with_server(void)
{
}

gboolean
on_server_disconnect(gchar* message, DisconnectState state)
{
    if (message)
        g_printerr("%s\n", message);

    switch (state)
    {
    case (HOST_CONNECTION_ERROR):
        g_print("attempting to reconnect server...");
        connect_to_host_async();

    case (HOST_CONNECTION_FORCE_END):
        g_print("closing agent");
        agent_state = AGENT_STATE_CLOSED;
        
        if (ws_conn) {
            if (soup_websocket_connection_get_state(ws_conn) ==
                SOUP_WEBSOCKET_STATE_OPEN)
                /* This will call us again */
                soup_websocket_connection_close(ws_conn, 1000, "");
            else
                g_object_unref(ws_conn);
        }
        /*clost main event loop*/
        if (loop) {
            g_main_loop_quit(loop);
            loop = NULL;
        }
    }
    return FALSE;
}



