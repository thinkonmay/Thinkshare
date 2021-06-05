#include "Framework.h"
#include "Variable.h"
#include "Handle data channel.h"
#include "Handle pipeline.h"
#include "RC config.h"
#include "CorePipeSink.h"
#include "Signalling handling.h"

enum AppState app_state;

void
send_ice_candidate_message(GstElement* webrtc G_GNUC_UNUSED,
    guint mlineindex,
    gchar* candidate,
    gpointer user_data G_GNUC_UNUSED)
{
    gchar* text;
    JsonObject* ice, * msg;

    if (app_state < PEER_CALL_NEGOTIATING) {
        cleanup_and_quit_loop("Can't send ICE, not in call", APP_STATE_ERROR);
        return;
    }

    ice = json_object_new();
    json_object_set_string_member(ice, "candidate", candidate);
    json_object_set_int_member(ice, "sdpMLineIndex", mlineindex);
    msg = json_object_new();
    json_object_set_object_member(msg, "ice", ice);
    text = get_string_from_json_object(msg);
    json_object_unref(msg);

    soup_websocket_connection_send_text(ws_conn, text);
    g_free(text);
}


static void
send_sdp_to_peer(GstWebRTCSessionDescription* desc)
{
    gchar* text;
    JsonObject* msg, * sdp;

    if (app_state < PEER_CALL_NEGOTIATING) {
        cleanup_and_quit_loop("Can't send SDP to peer, not in call",
            APP_STATE_ERROR);
        return;
    }

    text = gst_sdp_message_as_text(desc->sdp);
    sdp = json_object_new();

    if (desc->type == GST_WEBRTC_SDP_TYPE_OFFER) {
        g_print("Sending offer:\n%s\n", text);
        json_object_set_string_member(sdp, "type", "offer");
    }
    else if (desc->type == GST_WEBRTC_SDP_TYPE_ANSWER) {
        g_print("Sending answer:\n%s\n", text);
        json_object_set_string_member(sdp, "type", "answer");
    }
    else {
        g_assert_not_reached();
    }

    json_object_set_string_member(sdp, "sdp", text);
    g_free(text);

    msg = json_object_new();
    json_object_set_object_member(msg, "sdp", sdp);
    text = get_string_from_json_object(msg);
    json_object_unref(msg);

    soup_websocket_connection_send_text(ws_conn, text);
    g_free(text);
}


/* Offer created by our pipeline, to be sent to the peer */
static void
on_offer_created(GstPromise* promise, gpointer user_data)
{
    GstWebRTCSessionDescription* offer = NULL;
    const GstStructure* reply;

    g_assert_cmphex(app_state, == , PEER_CALL_NEGOTIATING);

    g_assert_cmphex(gst_promise_wait(promise), == , GST_PROMISE_RESULT_REPLIED);
    reply = gst_promise_get_reply(promise);
    gst_structure_get(reply, "offer",
        GST_TYPE_WEBRTC_SESSION_DESCRIPTION, &offer, NULL);
    gst_promise_unref(promise);

    promise = gst_promise_new();
    g_signal_emit_by_name(webrtcbin, "set-local-description", offer, promise);
    gst_promise_interrupt(promise);
    gst_promise_unref(promise);

    /* Send offer to peer */
    send_sdp_to_peer(offer);
    gst_webrtc_session_description_free(offer);
}


void
on_negotiation_needed(GstElement* element, gpointer user_data)
{
    app_state = PEER_CALL_NEGOTIATING;

    if (remote_is_offerer) {
        gchar* msg = g_strdup_printf("OFFER_REQUEST");
        soup_websocket_connection_send_text(ws_conn, msg);
        g_free(msg);
    }
    else {
        GstPromise* promise =
            gst_promise_new_with_change_func(on_offer_created, user_data, NULL);

        g_signal_emit_by_name(webrtcbin, "create-offer", NULL, promise);
    }
}



void
on_ice_gathering_state_notify(GstElement* webrtcbin,
    GParamSpec* pspec,
    gpointer user_data)
{
    GstWebRTCICEGatheringState ice_gather_state;
    const gchar* new_state = "unknown";

    g_object_get(webrtcbin, "ice-gathering-state", &ice_gather_state, NULL);
    switch (ice_gather_state) {
    case GST_WEBRTC_ICE_GATHERING_STATE_NEW:
        new_state = "new";
        break;
    case GST_WEBRTC_ICE_GATHERING_STATE_GATHERING:
        new_state = "gathering";
        break;
    case GST_WEBRTC_ICE_GATHERING_STATE_COMPLETE:
        new_state = "complete";
        break;
    }
    g_print("ICE gathering state changed to %s\n", new_state);
}

gboolean
setup_call(void)
{
    gchar* msg;

    if (soup_websocket_connection_get_state(ws_conn) !=
        SOUP_WEBSOCKET_STATE_OPEN)
        return FALSE;

    if (!peer_id)
        return FALSE;

    g_print("Setting up signalling server call with %s\n", peer_id);
    app_state = PEER_CONNECTING;
    msg = g_strdup_printf("SESSION %s", peer_id);
    soup_websocket_connection_send_text(ws_conn, msg);
    g_free(msg);
    return TRUE;
}

gboolean
register_with_server(void)
{
    gchar* hello;

    if (soup_websocket_connection_get_state(ws_conn) !=
        SOUP_WEBSOCKET_STATE_OPEN)
        return FALSE;

    g_print("Registering id %i with server\n", our_id);
    app_state = SERVER_REGISTERING;

    /* Register with the server with a random integer id. Reply will be received
     * by on_server_message() */
    hello = g_strdup_printf("HELLO %i", our_id);
    soup_websocket_connection_send_text(ws_conn, hello);
    g_free(hello);

    return TRUE;
}

void
on_server_closed(SoupWebsocketConnection* conn G_GNUC_UNUSED,
    gpointer user_data G_GNUC_UNUSED)
{
    app_state = SERVER_CLOSED;
    cleanup_and_quit_loop("Server connection closed", app_state = APP_STATE_ERROR);
}

/* Answer created by our pipeline, to be sent to the peer */
void
on_answer_created(GstPromise* promise,
    gpointer user_data)
{
    GstWebRTCSessionDescription* answer = NULL;
    const GstStructure* reply;

    g_assert_cmphex(app_state, == , PEER_CALL_NEGOTIATING);

    g_assert_cmphex(gst_promise_wait(promise), == , GST_PROMISE_RESULT_REPLIED);
    reply = gst_promise_get_reply(promise);
    gst_structure_get(reply, "answer",
        GST_TYPE_WEBRTC_SESSION_DESCRIPTION, &answer, NULL);
    gst_promise_unref(promise);

    promise = gst_promise_new();
    g_signal_emit_by_name(webrtcbin, "set-local-description", answer, promise);
    gst_promise_interrupt(promise);
    gst_promise_unref(promise);

    /* Send answer to peer */
    send_sdp_to_peer(answer);
    gst_webrtc_session_description_free(answer);
}

void
on_offer_set(GstPromise* promise,
    gpointer user_data)
{
    gst_promise_unref(promise);
    promise = gst_promise_new_with_change_func(on_answer_created, NULL, NULL);
    g_signal_emit_by_name(webrtcbin, "create-answer", NULL, promise);
}

void
on_offer_received(GstSDPMessage* sdp)
{
    GstWebRTCSessionDescription* offer = NULL;
    GstPromise* promise;

    offer = gst_webrtc_session_description_new(GST_WEBRTC_SDP_TYPE_OFFER, sdp);
    g_assert_nonnull(offer);

    /* Set remote description on our pipeline */
    {
        promise = gst_promise_new_with_change_func(on_offer_set, NULL, NULL);
        g_signal_emit_by_name(webrtcbin, "set-remote-description", offer,
            promise);
    }
    gst_webrtc_session_description_free(offer);
}



/*Connect to the signalling server. This is the entrypoint for everything else.
 */
void
connect_to_websocket_server_async(void)
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

    message = soup_message_new(SOUP_METHOD_GET, server_url);

    g_print("Connecting to server...\n");

    /* Once connected, we will register */
    soup_session_websocket_connect_async(session,
        message, NULL, NULL, NULL,
        (GAsyncReadyCallback)on_server_connected, message);
    app_state = SERVER_CONNECTING;
}

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

/* One mega message handler for our asynchronous calling mechanism */
void
on_server_message(SoupWebsocketConnection* conn,
    SoupWebsocketDataType type,
    GBytes* message,
    gpointer user_data)
{
    gchar* text = const_cast<gchar*>("ERROR");
    switch (type) {
    case SOUP_WEBSOCKET_DATA_BINARY:
        g_printerr("Received unknown binary message, ignoring\n");
        return;
    case SOUP_WEBSOCKET_DATA_TEXT: {
        gsize size;
        void* temp_ptr = const_cast<void*>(g_bytes_get_data(message, &size));
        const char* data = reinterpret_cast<char*>(temp_ptr);
        g_free(temp_ptr);
        /* Convert to NULL-terminated string */
        text = g_strndup(data, size);
        break;
    }
    default:
        g_assert_not_reached();
    }

    /* Server has accepted our registration, we are ready to send commands */
    if (g_strcmp0(text, "HELLO") == 0) {
        if (app_state != SERVER_REGISTERING) {
            cleanup_and_quit_loop("ERROR: Received HELLO when not registering",
                APP_STATE_ERROR);
            goto out;
        }
        app_state = SERVER_REGISTERED;
        g_print("Registered with server\n");
        /* Ask signalling server to connect us with a specific peer */
        if (!setup_call()) {
            cleanup_and_quit_loop("ERROR: Failed to setup call", PEER_CALL_ERROR);
            goto out;
        }
        /* Call has been setup by the server, now we can start negotiation */
    }
    else if (g_strcmp0(text, "SESSION_OK") == 0) {
        if (app_state != PEER_CONNECTING) {
            cleanup_and_quit_loop("ERROR: Received SESSION_OK when not calling",
                PEER_CONNECTION_ERROR);
            goto out;
        }

        app_state = PEER_CONNECTED;
        /* Start negotiation (exchange SDP and ICE candidates) */
        start_pipeline();
    }
    else if (g_str_has_prefix(text, "ERROR")) {
        switch (app_state) {
        case SERVER_CONNECTING:
            app_state = SERVER_CONNECTION_ERROR;
            break;
        case SERVER_REGISTERING:
            app_state = SERVER_REGISTRATION_ERROR;
            break;
        case PEER_CONNECTING:
            app_state = PEER_CONNECTION_ERROR;
            break;
        case PEER_CONNECTED:
        case PEER_CALL_NEGOTIATING:
            app_state = PEER_CALL_ERROR;
            break;
        default:
            app_state = APP_STATE_ERROR;
        }
        cleanup_and_quit_loop(text, APP_STATE_UNKNOWN);
        /* Look for JSON messages containing SDP and ICE candidates */
    }
    else
    {
        JsonNode* root;
        JsonObject* object, * child;
        JsonParser* parser = json_parser_new();
        if (!json_parser_load_from_data(parser, text, -1, NULL))
        {
            g_printerr("Unknown message '%s', ignoring", text);
            g_object_unref(parser);
            goto out;
        }

        root = json_parser_get_root(parser);
        if (!JSON_NODE_HOLDS_OBJECT(root))
        {
            g_printerr("Unknown json message '%s', ignoring", text);
            g_object_unref(parser);
            goto out;
        }

        object = json_node_get_object(root);
        /* Check type of JSON message */
        if (json_object_has_member(object, "sdp"))
        {
            int ret;
            GstSDPMessage* sdp;
            const gchar* text, * sdptype;
            GstWebRTCSessionDescription* answer;

            g_assert_cmphex(app_state, == , PEER_CALL_NEGOTIATING);

            child = json_object_get_object_member(object, "sdp");

            if (!json_object_has_member(child, "type"))
            {
                cleanup_and_quit_loop("ERROR: received SDP without 'type'",
                    PEER_CALL_ERROR);
                goto out;
            }

            sdptype = json_object_get_string_member(child, "type");
            /* In this example, we create the offer and receive one answer by default,
             * but it's possible to comment out the offer creation and wait for an offer
             * instead, so we handle either here.
             *
             * See tests/examples/webrtcbidirectional.c in gst-plugins-bad for another
             * example how to handle offers from peers and reply with answers using webrtcbin. */
            text = json_object_get_string_member(child, "sdp");
            ret = gst_sdp_message_new(&sdp);
            g_assert_cmphex(ret, == , GST_SDP_OK);
            ret = gst_sdp_message_parse_buffer((guint8*)text, strlen(text), sdp);
            g_assert_cmphex(ret, == , GST_SDP_OK);

            if (g_str_equal(sdptype, "answer"))
            {
                g_print("Received answer:\n%s\n", text);
                answer = gst_webrtc_session_description_new(GST_WEBRTC_SDP_TYPE_ANSWER,
                    sdp);
                g_assert_nonnull(answer);

                /* Set remote description on our pipeline */
                {
                    GstPromise* promise = gst_promise_new();
                    g_signal_emit_by_name(webrtcbin, "set-remote-description", answer,
                        promise);
                    gst_promise_interrupt(promise);
                    gst_promise_unref(promise);
                }
                app_state = PEER_CALL_STARTED;
            }
            else
            {
                g_print("Received offer:\n%s\n", text);
                on_offer_received(sdp);
            }

        }
        else if (json_object_has_member(object, "ice"))
        {
            const gchar* candidate;
            int sdpmlineindex;

            child = json_object_get_object_member(object, "ice");
            candidate = json_object_get_string_member(child, "candidate");
            sdpmlineindex = json_object_get_int_member(child, "sdpMLineIndex");

            /* Add ice candidate sent by remote peer */
            g_signal_emit_by_name(webrtcbin, "add-ice-candidate", sdpmlineindex,
                candidate);
        }
        else
        {
            g_printerr("Ignoring unknown JSON message:\n%s\n", text);
        }

        g_object_unref(parser);
    }

out:
    //g_free(text);
    return;
}


void
on_server_connected(SoupSession* session,
    GAsyncResult* res,
    SoupMessage* msg)
{
    GError* error = NULL;

    ws_conn = soup_session_websocket_connect_finish(session, res, &error);
    if (error) {
        cleanup_and_quit_loop(error->message, SERVER_CONNECTION_ERROR);
        g_error_free(error);
        return;
    }

    g_assert_nonnull(ws_conn);

    app_state = SERVER_CONNECTED;
    g_print("Connected to signalling server\n");

    g_signal_connect(ws_conn, "closed", G_CALLBACK(on_server_closed), NULL);
    g_signal_connect(ws_conn, "message", G_CALLBACK(on_server_message), NULL);

    /* Register with the server so it knows about us and can accept commands */
    register_with_server();
}






