
#include <remote-app-signalling.h>
#include <remote-app.h>
#include <remote-app-pipeline.h>
#include <remote-app-type.h>

#include <logging.h>
#include <exit-code.h>
#include <error-code.h>
#include <signalling-message.h>
#include <general-constant.h>

#include <gst/gst.h>
#include <glib-2.0/glib.h>
#include <remote-app-type.h>
#include <libsoup/soup.h>
#include <json-glib/json-glib.h>

#include <gst/webrtc/webrtc.h>
#include <gst/rtp/gstrtppayloads.h>
#include <libsoup/soup.h>

#include <stdio.h>
#include <Windows.h>




struct _SignallingHub
{
    /// <summary>
    /// websocket connection object encapsulate websocket connection between slave and signalling server,
    /// we must provide message handler function.
    /// </summary>
    SoupWebsocketConnection* connection;

    /// <summary>
    /// soup session represent a session between session core and signalling server,
    /// it encapsulate signalling url and disable ssl option
    /// </summary>
    SoupSession* session;

    /// <summary>
    /// session slave id use to register session with signalling server
    /// </summary>
    gint SessionClientID;

    /// <summary>
    /// url of signalling server
    /// </summary>
	gchar* signalling_server;

    /// disable ssl option, should only be set to true in development environment
	gboolean disable_ssl;

    /// <summary>
    /// url of turn server
    /// </summary>
	gchar* turn;

    /// <summary>
    /// state of signalling connection,
    /// used to check if any state conflict is avilable
    /// </summary>
    SignallingServerState signalling_state;

    /// <summary>
    /// peer call state, include sdp and ice negotiation state
    /// </summary>
    PeerCallState peer_call_state;
};


void
on_server_connected(SoupSession* session,
    GAsyncResult* res,
    RemoteApp* core);

SignallingHub*
signalling_hub_initialize(RemoteApp* core)
{
    static SignallingHub hub;
    GFile* config = g_file_parse_name(HOST_CONFIG_FILE);
    GBytes* byte_config = g_file_load_bytes(config, NULL, NULL, NULL);


    JsonParser* parser_config = json_parser_new();
    json_parser_load_from_data(parser_config, g_bytes_get_data(byte_config, NULL), -1, NULL);
    JsonNode* root_config = json_parser_get_root(parser_config);
    JsonObject* object_config = json_node_get_object(root_config);

    hub.peer_call_state = PEER_CALL_NOT_READY;
    hub.signalling_state = SIGNALLING_SERVER_NOT_READY;

    hub.disable_ssl = json_object_get_boolean_member(object_config,DISABLE_SSL);
    return &hub;
}



void
signalling_hub_setup(SignallingHub* hub, 
                     gchar* turn,
                     gchar* url,
                     gint session_client_id)
{
    hub->signalling_server = url;
    hub->turn = turn;
    hub->SessionClientID= session_client_id;
    hub->signalling_state = SIGNALLING_SERVER_READY;
}




void
send_message_to_signalling_server(SignallingHub* signalling,
                                gchar* request_type,
                                gchar* content)
{
    JsonObject* json_object = json_object_new();
    json_object_set_string_member(json_object, REQUEST_TYPE, request_type);
    json_object_set_int_member(json_object, SUBJECT_ID, signalling->SessionClientID);
    json_object_set_string_member(json_object, CONTENT, content);
    json_object_set_string_member(json_object, RESULT, SESSION_ACCEPTED); 
    
    gchar* buffer = get_string_from_json_object(json_object);

    soup_websocket_connection_send_text(signalling->connection,buffer);
    g_free(buffer);
}



void
send_ice_candidate_message(GstElement* webrtc G_GNUC_UNUSED,
    guint mlineindex,
    gchar* candidate,
    RemoteApp* core G_GNUC_UNUSED)
{
    gchar* text;
    JsonObject* ice, * msg;

    SignallingHub* hub = remote_app_get_signalling_hub(core);

    //if (g_strcmp0(hub->peer_call_state, PEER_CALL_NEGOTIATING))
    //{
    //    GError error;
    //    error.message = "State conflict";
    //    remote_app_finalize(core, CORE_STATE_CONFLICT_EXIT, &error);
    //}

    ice = json_object_new();
    json_object_set_string_member(ice, "candidate", candidate);
    json_object_set_int_member(ice, "sdpMLineIndex", mlineindex);
    msg = json_object_new();
    json_object_set_object_member(msg, "ice", ice);
    text = get_string_from_json_object(msg);
    json_object_unref(msg);



    send_message_to_signalling_server(hub,OFFER_ICE,text);
    g_free(text);
}



void
send_sdp_to_peer(RemoteApp* core,
    GstWebRTCSessionDescription* desc)
{
    gchar* text;
    JsonObject* msg, * sdp;

    SignallingHub* hub = remote_app_get_signalling_hub(core);

    if (!hub->peer_call_state == PEER_CALL_NEGOTIATING) 
    {
        GError error;
        error.message = "State conflict";
        remote_app_finalize( core, CORE_STATE_CONFLICT_EXIT, &error);
    }
    text = gst_sdp_message_as_text(desc->sdp);
    sdp = json_object_new();

    if (desc->type == GST_WEBRTC_SDP_TYPE_OFFER) 
    {
        json_object_set_string_member(sdp, "type", "offer");
    }
    else if (desc->type == GST_WEBRTC_SDP_TYPE_ANSWER) 
    {
        json_object_set_string_member(sdp, "type", "answer");
    }
    else 
    {
        g_assert_not_reached();
    }

    json_object_set_string_member(sdp, "sdp", text);

    msg = json_object_new();
    json_object_set_object_member(msg, "sdp", sdp);
    text = get_string_from_json_object(msg);
    json_object_unref(msg);

    send_message_to_signalling_server(hub,OFFER_SDP,text);
    g_free(text);
}


/* Offer created by our pipeline, to be sent to the peer */
void
on_offer_created( GstPromise* promise, RemoteApp* core)
{
    GstWebRTCSessionDescription* offer = NULL;
    const GstStructure* reply;

    Pipeline* pipe = remote_app_get_pipeline(core);
    SignallingHub* hub = remote_app_get_signalling_hub(core);

    if (g_strcmp0(hub->peer_call_state, PEER_CALL_NEGOTIATING))
    {
        GError error;
        error.message = "State conflict";
        remote_app_finalize( core, CORE_STATE_CONFLICT_EXIT, &error);
    }

    g_assert_cmphex(gst_promise_wait(promise), == , GST_PROMISE_RESULT_REPLIED);

    reply = gst_promise_get_reply(promise);
    gst_structure_get(reply, "offer",
        GST_TYPE_WEBRTC_SESSION_DESCRIPTION, &offer, NULL);
    gst_promise_unref(promise);

    promise = gst_promise_new();
    g_signal_emit_by_name(pipeline_get_webrtc_bin(pipe),
        "set-local-description", offer, promise);

    gst_promise_interrupt(promise);

    /* Send offer to peer */
    send_sdp_to_peer(core,offer);
    gst_webrtc_session_description_free(offer);
}


void
on_negotiation_needed(GstElement* element, RemoteApp* core)
{
    Pipeline* pipe = remote_app_get_pipeline(core);
    SignallingHub* signalling = remote_app_get_signalling_hub(core);


    GstPromise* promise =
    gst_promise_new_with_change_func(on_offer_created, core, NULL);

    g_signal_emit_by_name(pipeline_get_webrtc_bin(pipe),
        "create-offer", NULL, promise);

    signalling->peer_call_state = PEER_CALL_NEGOTIATING;
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
}


/// <summary>
/// register with server by sending SLAVEREQUEST +{hub_id}
/// </summary>
/// <param name="core"></param>
/// <returns></returns>
gboolean
register_with_server(RemoteApp* core)
{
    gchar* hello;
    CoreState* state;
    JsonObject* json_object = json_object_new();
    SignallingHub* hub = remote_app_get_signalling_hub(core);

    
    if (g_strcmp0(hub->signalling_state, SIGNALLING_SERVER_CONNECTED) || 
        (soup_websocket_connection_get_state(hub->connection) != SOUP_WEBSOCKET_STATE_OPEN))
    {
        GError error;
        error.message = "State conflict";
        remote_app_finalize( core, CORE_STATE_CONFLICT_EXIT, &error);  
    }

    //gchar* buffer = malloc(10);
    //itoa(hub->SessionSlaveID, buffer, 10);
    hub->signalling_state = SIGNALLING_SERVER_REGISTERING;
    send_message_to_signalling_server(hub,CLIENT_REQUEST, SESSION_ACCEPTED);
    return TRUE;
}


/// <summary>
/// close
/// </summary>
/// <param name="G_GNUC_UNUSED"></param>
/// <param name="G_GNUC_UNUSED"></param>
void
on_server_closed(SoupWebsocketConnection* conn G_GNUC_UNUSED,
    RemoteApp* core G_GNUC_UNUSED)
{
    SignallingHub* hub = remote_app_get_signalling_hub(core);
    hub->connection = NULL;
    hub->session = NULL;
}

/* Answer created by our pipeline, to be sent to the peer */
void
on_answer_created(GstPromise* promise,
    RemoteApp* core)
{
    GstWebRTCSessionDescription* answer = NULL;
    const GstStructure* reply;

    Pipeline* pipe = remote_app_get_pipeline(core);
    SignallingHub* hub = remote_app_get_signalling_hub(core);


    g_assert_cmphex(gst_promise_wait(promise), == , GST_PROMISE_RESULT_REPLIED);
    reply = gst_promise_get_reply(promise);
    gst_structure_get(reply, "answer",
        GST_TYPE_WEBRTC_SESSION_DESCRIPTION, &answer, NULL);
    gst_promise_unref(promise);

    promise = gst_promise_new();

    g_signal_emit_by_name( pipeline_get_webrtc_bin(pipe),
        "set-local-description", answer, promise);

    gst_promise_interrupt(promise);

    /* Send answer to peer */
    send_sdp_to_peer(core,answer);
    gst_webrtc_session_description_free(answer);
}

void
on_offer_set(GstPromise* promise,
            RemoteApp* core)
{
    Pipeline* pipe = remote_app_get_pipeline(core);
    GstElement* webrtc = pipeline_get_webrtc_bin(pipe);

    gst_promise_unref(promise);
    promise = gst_promise_new_with_change_func(on_answer_created, 
        core, NULL);

    g_signal_emit_by_name(webrtc,
        "create-answer", NULL, promise);
}

void
on_offer_received(RemoteApp* core, GstSDPMessage* sdp)
{
    GstWebRTCSessionDescription* offer = NULL;
    GstPromise* promise;

    Pipeline* pipe = remote_app_get_pipeline(core);

    offer = gst_webrtc_session_description_new(GST_WEBRTC_SDP_TYPE_OFFER, sdp);
    g_assert_nonnull(offer);

    /* Set remote description on our pipeline */
    promise = gst_promise_new_with_change_func(on_offer_set, 
        core, NULL);

    g_signal_emit_by_name(pipeline_get_webrtc_bin(pipe), 
        "set-remote-description", offer,
        promise);

    gst_webrtc_session_description_free(offer);
}




void
remote_app_logger(SoupLogger* logger,
            SoupLoggerLogLevel  level,
            char                direction,
            const char         *data,
            gpointer            user_data)
{

}








/// <summary>
/// *Connect to the signalling server. 
///  This is the entrypoint for everything else.
/// 
/// </summary>
/// <param name="core"></param>
void
connect_to_websocket_signalling_server_async(RemoteApp* core)
{
    SoupLogger* logger;
    SoupMessage* message;

    const char* https_aliases[] = { "ws", NULL };
    JsonObject* json_object;

    SignallingHub* hub = remote_app_get_signalling_hub(core);

    gchar* text;



    hub->session =
        soup_session_new_with_options(SOUP_SESSION_SSL_STRICT, !hub->disable_ssl,
            SOUP_SESSION_SSL_USE_SYSTEM_CA_FILE, TRUE,
            SOUP_SESSION_HTTPS_ALIASES, https_aliases, NULL);

    logger = soup_logger_new(SOUP_LOGGER_LOG_BODY, -1);
    soup_session_add_feature(hub->session, SOUP_SESSION_FEATURE(logger));
    g_object_unref(logger);


    soup_logger_set_printer(logger,remote_app_logger,NULL,NULL);

    message = soup_message_new(SOUP_METHOD_GET, hub->signalling_server);


    hub->signalling_state = SIGNALLING_SERVER_CONNECTING;
    soup_session_websocket_connect_async(hub->session,
        message, NULL, NULL, NULL,
        (GAsyncReadyCallback)on_server_connected, core);
}


static void
on_registering_message(RemoteApp* core)
{
    SignallingHub* signalling = remote_app_get_signalling_hub(core);

    if (g_strcmp0(signalling->signalling_state, SIGNALLING_SERVER_REGISTERING))
    {
        GError error;
        error.message = "State conflict";
        remote_app_finalize( core, CORE_STATE_CONFLICT_EXIT, &error);
    }
    if (g_strcmp0( signalling->signalling_state , SIGNALLING_SERVER_REGISTERING))
    {
        GError error;
        error.message = "State conflict";
        remote_app_finalize( core, CORE_STATE_CONFLICT_EXIT, &error);
    }
    signalling->signalling_state = SIGNALLING_SERVER_REGISTER_DONE;
    signalling->peer_call_state = PEER_CALL_READY;
    /* Call has been setup by the server, now we can start negotiation */

    // send sdp request to slave
    JsonObject* object = json_object_new();
    json_object_set_string_member(object,"type","request");

    JsonObject* message = json_object_new();
    json_object_set_object_member(message,"sdp",object);
    gchar* text= get_string_from_json_object(message);

    send_message_to_signalling_server(signalling,OFFER_SDP,text);
}

static void
on_ice_exchange(gchar* text,RemoteApp* core)
{
    Pipeline* pipe = remote_app_get_pipeline(core);

    GError* error = NULL;
    JsonParser* parser = json_parser_new();
    JsonObject* object = get_json_object_from_string(text,&error,parser);
	if(!error == NULL || object == NULL) {return;}

    const gchar* candidate;
    gint sdpmlineindex;
    JsonObject* child = json_object_get_object_member(object, "ice");
    candidate = json_object_get_string_member(child, "candidate");
    sdpmlineindex = json_object_get_int_member(child, "sdpMLineIndex");
    /* Add ice candidate sent by remote peer */
    g_signal_emit_by_name(pipeline_get_webrtc_bin(pipe),
        "add-ice-candidate", sdpmlineindex, candidate);
    g_object_unref(parser);
}

static void
on_sdp_exchange(gchar* data, 
                RemoteApp* core)
{
    SignallingHub* hub = remote_app_get_signalling_hub(core);
    Pipeline* pipe = remote_app_get_pipeline(core);

    GError* error = NULL;
    JsonParser* parser = json_parser_new();
    JsonObject* object = get_json_object_from_string(data,&error,parser);
	if(!error == NULL || object == NULL) {remote_app_finalize(core,UNKNOWN_PACKAGE_FROM_CLIENT,error);}

    gint ret;
    GstSDPMessage* sdp;
    const gchar* text;
    GstWebRTCSessionDescription* answer;

    JsonObject* child = json_object_get_object_member(object, "sdp");
    gchar* sdptype = json_object_get_string_member(child, "type");

    if (!json_object_has_member(child, "type"))
    {
        return;
    }


    text = json_object_get_string_member(child, "sdp");
    ret = gst_sdp_message_new(&sdp);

    g_assert_cmphex(ret, == , GST_SDP_OK);
    ret = gst_sdp_message_parse_buffer((guint8*)text, strlen(text), sdp);
    g_assert_cmphex(ret, == , GST_SDP_OK);


    if (g_str_equal(sdptype, "answer"))
    {
        answer = gst_webrtc_session_description_new(GST_WEBRTC_SDP_TYPE_ANSWER,sdp);

        g_assert_nonnull(answer);
        /* Set remote description on our pipeline */
        {
            GstPromise* promise = gst_promise_new();
            g_signal_emit_by_name(pipeline_get_webrtc_bin(pipe),
                "set-remote-description", answer, promise);
            gst_promise_interrupt(promise);
            gst_promise_unref(promise);
        }
    }
    else
    {
        on_offer_received(core,sdp);
    }

    g_object_unref(parser);
}

/// <summary>
/// callback function for signalling server message
/// </summary>
/// <param name="conn"></param>
/// <param name="type"></param>
/// <param name="message"></param>
/// <param name="core"></param>
void
on_server_message(SoupWebsocketConnection* conn,
    SoupWebsocketDataType type,
    GBytes* message,
    RemoteApp* core)
{
    Pipeline* pipe = remote_app_get_pipeline(core);
    gchar* text;

    switch (type) 
    {
        case SOUP_WEBSOCKET_DATA_BINARY:
        {
            return;
        } 
        case SOUP_WEBSOCKET_DATA_TEXT: 
        {
            gsize size;
            const char* data = g_bytes_get_data(message, &size);
            /* Convert to NULL-terminated string */
            text = g_strndup(data, size);
            break;
        }
    }



    GError* error = NULL;
    JsonParser* parser = json_parser_new();
    JsonObject* object = get_json_object_from_string(text,&error,parser);
	if(!error == NULL || object == NULL) {return;}

    gchar* RequestType =    json_object_get_string_member(object, "RequestType");
    gint SubjectId =        json_object_get_int_member(object, "SubjectId");
    gchar* Content =        json_object_get_string_member(object, "Content");
    gchar* Result =         json_object_get_string_member(object, "Result");
    g_free(text);

    if (!g_strcmp0(Result, "SESSION_REJECTED") ||
        !g_strcmp0(Result, "SESSION_TIMEOUT"))
    {
        GError error;
        error.message = "Session has been rejected, this may due to security attack or signalling failure";
        remote_app_finalize( core, CORE_STATE_CONFLICT_EXIT, &error);
    }

    /*this is websocket message with signalling server and has nothing to do with 
    * json message format use to communicate with other module
    */
    if (!g_strcmp0(RequestType , "CLIENTREQUEST"))
    {
        on_registering_message(core);
    }
    else if (!g_strcmp0(RequestType, "OFFER_SDP"))
    {
        on_sdp_exchange(Content, core);
    }
    else if (!g_strcmp0(RequestType, "OFFER_ICE"))
    {
        on_ice_exchange(Content, core);
    }
    g_object_unref(parser);
}


void
on_server_connected(SoupSession* session,
    GAsyncResult* res,
    RemoteApp* core)
{
    GError* error = NULL;
    SignallingHub* hub = remote_app_get_signalling_hub(core);

    if (g_strcmp0(hub->signalling_state, SIGNALLING_SERVER_CONNECTING))  {  return;  }
    
    hub->connection = soup_session_websocket_connect_finish(session, res, &error);
    if (!error == NULL || hub->connection == NULL) 
    {
        remote_app_finalize(core, SIGNALLING_SERVER_CONNECTION_ERROR_EXIT,error);
    }

    hub->signalling_state = SIGNALLING_SERVER_CONNECTED;
    g_signal_connect(hub->connection, "closed", G_CALLBACK(on_server_closed), core);
    g_signal_connect(hub->connection, "message", G_CALLBACK(on_server_message), core);

    // register to server after connect to signalling server
    register_with_server(core);
    return;
}


gboolean
signalling_close(SignallingHub* hub)
{
    if (hub->connection)
    {
        if (soup_websocket_connection_get_state(hub->connection) == SOUP_WEBSOCKET_STATE_OPEN)
            soup_websocket_connection_close(hub->connection, 1000, "");
        else
            g_object_unref(hub->connection);
    }
}



/*START get-set function*/
gchar* 
signalling_hub_get_turn_server(SignallingHub* hub)
{
    return hub->turn;
}



SignallingServerState 
signalling_hub_get_signalling_state(SignallingHub* hub)
{
    return hub->signalling_state;
}

PeerCallState
signalling_hub_get_peer_call_state(SignallingHub* hub)
{
    return hub->peer_call_state;
}

void
signalling_hub_set_signalling_state(SignallingHub* hub,
                                    SignallingServerState state)
{
    hub->signalling_state = state;
}

void
signalling_hub_set_peer_call_state(SignallingHub* hub,
                                   PeerCallState state)
{
    hub->peer_call_state = state;
}