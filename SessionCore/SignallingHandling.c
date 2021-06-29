#include "SignallingHandling.h"

void
send_ice_candidate_message(GstElement* webrtc G_GNUC_UNUSED,
    guint mlineindex,
    gchar* candidate,
    SessionCore* core G_GNUC_UNUSED)
{
    gchar* text;
    JsonObject* ice, * msg;

    CoreState* state;
    g_object_get_property(core,"core-state",state);
    WebRTCHub* hub = session_core_get_rtc_hub(core);

    if (state < PEER_CALL_NEGOTIATING) {
        session_core_end("Can't send ICE, not in call",core, APP_STATE_ERROR);
        return;
    }

    ice = json_object_new();
    json_object_set_string_member(ice, "candidate", candidate);
    json_object_set_int_member(ice, "sdpMLineIndex", mlineindex);
    msg = json_object_new();
    json_object_set_object_member(msg, "ice", ice);
    text = get_string_from_json_object(msg);
    json_object_unref(msg);

    soup_websocket_connection_send_text(hub->ws, text);
    g_free(text);
}



void
send_sdp_to_peer(SessionCore* core,
    GstWebRTCSessionDescription* desc)
{
    gchar* text;
    JsonObject* msg, * sdp;

    CoreState* state;
    g_objet_property(core,"core-state",state);
    WebRTCHub* hub = session_core_get_rtc_hub(core);
    if (state < PEER_CALL_NEGOTIATING) {
        session_core_end("Can't send SDP to peer, not in call", core,
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

    soup_websocket_connection_send_text(hub->ws, text);
    g_free(text);
}


/* Offer created by our pipeline, to be sent to the peer */
void
on_offer_created( GstPromise* promise, SessionCore* core)
{
    GstWebRTCSessionDescription* offer = NULL;
    const GstStructure* reply;

    Pipeline* pipe = session_core_get_pipeline(core);
    gint state;
    g_object_get_property(core, "core-state", state);


    g_assert_cmphex(state, == , PEER_CALL_NEGOTIATING);

    g_assert_cmphex(gst_promise_wait(promise), == , GST_PROMISE_RESULT_REPLIED);

    reply = gst_promise_get_reply(promise);
    gst_structure_get(reply, "offer",
        GST_TYPE_WEBRTC_SESSION_DESCRIPTION, &offer, NULL);
    gst_promise_unref(promise);

    promise = gst_promise_new();
    g_signal_emit_by_name(pipe->webrtcbin, "set-local-description", offer, promise);
    gst_promise_interrupt(promise);
    gst_promise_unref(promise);

    /* Send offer to peer */
    send_sdp_to_peer(core,offer);
    gst_webrtc_session_description_free(offer);
}


void
on_negotiation_needed(GstElement* element, SessionCore* core)
{
    g_object_set_property(core,"core-state", PEER_CALL_NEGOTIATING);

    Pipeline* pipe = session_core_get_pipeline(core);
    WebRTCHub* hub = session_core_get_rtc_hub(core);

    if (hub->client_offer) {
        gchar* msg = g_strdup_printf("WAITING_CLIENT");
        soup_websocket_connection_send_text(hub->ws, msg);
        g_free(msg);
    }
    else {
        GstPromise* promise =
        gst_promise_new_with_change_func(on_offer_created, core, NULL);

        g_signal_emit_by_name(pipe->webrtcbin, "create-offer", NULL, promise);
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


/// <summary>
/// register with server by sending SLAVEREQUEST +{hub_id}
/// </summary>
/// <param name="core"></param>
/// <returns></returns>
gboolean
register_with_server(SessionCore* core)
{
    gchar* hello;
    CoreState* state;
    JsonObject* json_object;

    json_object = json_object_new();


    g_object_get_property(core, "core-state", state);
    WebRTCHub* hub = session_core_get_rtc_hub(core);

    if (soup_websocket_connection_get_state(hub->ws) !=
        SOUP_WEBSOCKET_STATE_OPEN)
        return FALSE;

    g_print("Registering ID with signalling server\n");
    g_object_set_property(core,"core-state",SERVER_REGISTERING);

    /*register to signalling server by send an json object which has 3 members:
    * "request_type" : "SLAVEREQUEST"
    * "subject_id": {SlaveSessionID}
    * "content": {SlaveSessionID}
    */
    json_object_set_string_member(json_object,"request_type","SLAVEREQUEST");
    json_object_set_int_member(json_object, "subject_id", hub->signalling_url);
    json_object_set_int_member(json_object, "content", hub->signalling_url);
    hello = get_string_from_json_object(json_object);

    soup_websocket_connection_send_text(hub->ws, hello);
    g_free(hello);

    return TRUE;
}


/// <summary>
/// close
/// </summary>
/// <param name="G_GNUC_UNUSED"></param>
/// <param name="G_GNUC_UNUSED"></param>
void
on_server_closed(SoupWebsocketConnection* conn G_GNUC_UNUSED,
    SessionCore* core G_GNUC_UNUSED)
{
    g_object_set_property(core,"core-state",SERVER_CLOSED);
    session_core_end("Server connection closed",core, APP_STATE_ERROR);
}

/* Answer created by our pipeline, to be sent to the peer */
void
on_answer_created(GstPromise* promise,
    SessionCore* core)
{
    GstWebRTCSessionDescription* answer = NULL;
    const GstStructure* reply;
    Pipeline* pipe = session_core_get_pipeline(core);
    CoreState state;
    g_object_get_property(core, "core-state", state);

    g_assert_cmphex(state, == , PEER_CALL_NEGOTIATING);

    g_assert_cmphex(gst_promise_wait(promise), == , GST_PROMISE_RESULT_REPLIED);
    reply = gst_promise_get_reply(promise);
    gst_structure_get(reply, "answer",
        GST_TYPE_WEBRTC_SESSION_DESCRIPTION, &answer, NULL);
    gst_promise_unref(promise);

    promise = gst_promise_new();
    g_signal_emit_by_name(pipe->webrtcbin, "set-local-description", answer, promise);
    gst_promise_interrupt(promise);
    gst_promise_unref(promise);

    /* Send answer to peer */
    send_sdp_to_peer(core,answer);
    gst_webrtc_session_description_free(answer);
}

void
on_offer_set(GstPromise* promise,
    SessionCore* core)
{
    Pipeline* pipe = session_core_get_pipeline(core);

    gst_promise_unref(promise);
    promise = gst_promise_new_with_change_func(on_answer_created, pipe->webrtcbin, core);
    g_signal_emit_by_name(pipe->webrtcbin, "create-answer", NULL, promise);
}

void
on_offer_received(SessionCore* core, GstSDPMessage* sdp)
{
    GstWebRTCSessionDescription* offer = NULL;
    GstPromise* promise;

    Pipeline* pipe = session_core_get_pipeline(core);

    offer = gst_webrtc_session_description_new(GST_WEBRTC_SDP_TYPE_OFFER, sdp);
    g_assert_nonnull(offer);

    /* Set remote description on our pipeline */
    {
        promise = gst_promise_new_with_change_func(on_offer_set, pipe->webrtcbin, NULL);
        g_signal_emit_by_name(pipe->webrtcbin, "set-remote-description", offer,
            promise);
    }
    gst_webrtc_session_description_free(offer);
}



/// <summary>
/// *Connect to the signalling server. 
///  This is the entrypoint for everything else.
/// 
/// </summary>
/// <param name="core"></param>
void
connect_to_websocket_signalling_server_async(SessionCore* core)
{
    SoupLogger* logger;
    SoupMessage* message;
    SoupSession* session;
    const char* https_aliases[] = { "wss", NULL };
    JsonObject* json_object;
    WebRTCHub* hub = session_core_get_rtc_hub(core);
    CoreState state;
    gchar* text;

    WebRTCHub* hub = session_core_get_rtc_hub(core);


    g_object_get_property(core, "core-state", &state);

    while (state != SESSION_INFORMATION_SETTLED)
    {
        g_printerr("trying to connect to signalling server without information");

        sleep(1);
    }

    session =
        soup_session_new_with_options(SOUP_SESSION_SSL_STRICT,
            hub->disable_ssl,
            SOUP_SESSION_SSL_USE_SYSTEM_CA_FILE, TRUE,
            //SOUP_SESSION_SSL_CA_FILE, "/etc/ssl/certs/ca-bundle.crt",
            SOUP_SESSION_HTTPS_ALIASES, https_aliases, NULL);

    logger = soup_logger_new(SOUP_LOGGER_LOG_BODY, -1);
    soup_session_add_feature(session, SOUP_SESSION_FEATURE(logger));
    g_object_unref(logger);

    message = soup_message_new(SOUP_METHOD_GET, hub->signalling_url);


    g_print("Connecting to server...\n");
    /* Once connected, we will register */
    soup_session_websocket_connect_async(session,
        message, NULL, NULL, NULL,
        (GAsyncReadyCallback)on_server_connected, core);
    g_object_set_property(core, "core-state", SERVER_CONNECTING);
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

/// <summary>
/// callback function for signalling server message
/// 
/// </summary>
/// <param name="conn"></param>
/// <param name="type"></param>
/// <param name="message"></param>
/// <param name="core"></param>
void
on_server_message(SoupWebsocketConnection* conn,
    SoupWebsocketDataType type,
    GBytes* message,
    SessionCore* core)
{
    CoreState state;
    JsonNode* root;
    JsonObject* object, * child;
    gchar* request_type, * result;



    JsonParser* parser = json_parser_new();
    Pipeline* pipe = session_core_get_pipeline(core);

    g_object_get_property(core, "core-state", state);

    gchar* text = "ERROR";
    switch (type) {
    case SOUP_WEBSOCKET_DATA_BINARY:
        g_printerr("Received unknown binary message, ignoring\n");
        return;
    case SOUP_WEBSOCKET_DATA_TEXT: {
        gsize size;
        const char* data = g_bytes_get_data(message, &size);
        /* Convert to NULL-terminated string */
        text = g_strndup(data, size);
        break;
    }
    default:
        g_assert_not_reached();
    }


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


    if (json_object_has_member(object, "request_type"))
    {
        request_type = json_object_get_string_member(object, "request_type");
        if (request_type == "result")
        {
            result = json_object_get_string_member(object, "content");
             /*session id pair id found, set core-state to server registered and start pipeline*/
            if (result == "accepted")
            {
                if (state != SERVER_REGISTERING) 
                {
                    session_core_end("ERROR: Received SESSION_ACCEPTED signal when not registering", core, APP_STATE_ERROR);
                    goto out;
                }
                g_object_set_property(core, "core-state", SERVER_REGISTERED);
                g_print("Remote control accepted\n");
                /* Call has been setup by the server, now we can start negotiation */
            }
             /*denied message send from signalling server to slave, end session core*/
            if (result == "denied")
            {
                if (state != SERVER_REGISTERING)
                {
                    session_core_end("ERROR: Received SESSION_DENIED signal when not registering", core,
                        SESSION_DENIED);
                    goto out;
                }
                session_core_end("SessionID pair not found, session denied", core,
                    SESSION_DENIED);
            }
        }
        else
        {
            g_printerr("Ignoring unknown JSON message:\n%s\n", text);
        }
    }
    else if (json_object_has_member(object, "sdp"))
    {
        int ret;
        GstSDPMessage* sdp;
        const gchar* text, * sdptype;
        GstWebRTCSessionDescription* answer;
         g_assert_cmphex(state, == , PEER_CALL_NEGOTIATING);
         child = json_object_get_object_member(object, "sdp");
         if (!json_object_has_member(child, "type"))
        {
            session_core_end("ERROR: received SDP without 'type'", core,
                PEER_CALL_ERROR);
            goto out;
        }
        sdptype = json_object_get_string_member(child, "type");
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
                g_signal_emit_by_name(pipe->webrtcbin, "set-remote-description", answer,
                    promise);                         
                gst_promise_interrupt(promise);
                gst_promise_unref(promise);
            }
            g_object_set_property(core,"core-state",PEER_CALL_STARTED);
        }
        else
        {
            g_print("Received offer:\n%s\n", text);
            on_offer_received(pipe->webrtcbin,sdp);
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
        g_signal_emit_by_name(pipe->webrtcbin, "add-ice-candidate", sdpmlineindex,
            candidate);
    }
    else
    {
        g_printerr("Ignoring unknown JSON message:\n%s\n", text);
    }
    g_object_unref(parser);   

out:
    g_free(text);
    return;
}


void
on_server_connected(SoupSession* session,
    GAsyncResult* res,
    SessionCore* core)
{
    GError* error = NULL;

    WebRTCHub* hub = session_core_get_rtc_hub(core);

    hub->ws = soup_session_websocket_connect_finish(session, res, &error);
    if (error) {
        session_core_end(error->message,core, SERVER_CONNECTION_ERROR);
        g_error_free(error);
        return;
    }

    g_assert_nonnull(hub->ws);

    g_object_set_property(core,"core-state",SERVER_CONNECTED);

    g_print("Connected to signalling server\n");

    g_signal_connect(hub->ws, "closed", G_CALLBACK(on_server_closed), core);
    g_signal_connect(hub->ws, "message", G_CALLBACK(on_server_message), core);
    register_with_server(core);
    return;
}
///register to server after connect to signalling server





