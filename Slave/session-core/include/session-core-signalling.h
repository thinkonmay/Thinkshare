#ifndef __SESSION_CORE_SIGNALLING_H__
#define __SESSION_CORE_SIGNALLING_H__

#include "session-core-type.h"
#include "session-core.h"
#include "session-core-type.h"


#include <message-form.h>

#include <libsoup/soup.h>
#include <glib.h>
#include <gst/gst.h>

/*Used as webrtcbin callback function*/
void                send_ice_candidate_message              (GstElement* webrtc G_GNUC_UNUSED,
                                                             guint mlineindex,
                                                             gchar* candidate,
                                                             SessionCore* core G_GNUC_UNUSED);

void                on_negotiation_needed                   (GstElement* element,
                                                             SessionCore* core);


void                on_ice_gathering_state_notify           (GstElement* webrtcbin,
                                                             GParamSpec* pspec,
                                                             gpointer user_data);


SignallingHub*      signalling_hub_initialize               (SessionCore* core);


gchar*              get_string_from_json_object             (JsonObject* object);

void                on_server_closed                        (SoupWebsocketConnection* conn G_GNUC_UNUSED,
                                                             SessionCore* core G_GNUC_UNUSED);

/// <summary>
/// Answer created by our pipeline, to be sent to the peer 
/// </summary>
/// <param name="promise"></param>
/// <param name="core"></param>
void                on_answer_created                       (GstPromise* promise,
                                                             SessionCore* core);

void                on_server_message                       (SoupWebsocketConnection* conn,
                                                             SoupWebsocketDataType type,
                                                             GBytes* message,
                                                             SessionCore* core);

void                on_server_connected                     (SoupSession* session,
                                                             GAsyncResult* res,
                                                             SessionCore* core);

void                connect_to_websocket_signalling_server_async(SessionCore* core);

gboolean            register_with_server                    (SessionCore* core);

void                signalling_hub_setup                    (SignallingHub* hub,
                                                             gchar* url,
                                                             gboolean client_offer,
                                                             gchar* stun_server,
                                                             gint session_slave_id);

gboolean            signalling_close                        (SignallingHub* hub);





void                signalling_hub_set_stun_server          (SignallingHub* hub, 
                                                             gchar* stun);

gchar*              signalling_hub_get_stun_server          (SignallingHub* hub);

PeerCallState       signalling_hub_get_peer_call_state      (SignallingHub* hub);

SignallingServerState signalling_hub_get_signalling_state   (SignallingHub* hub);

void                signalling_hub_set_peer_call_state      (SignallingHub* hub,
                                                             PeerCallState state);

void                signalling_hub_set_signalling_state     (SignallingHub* hub,
                                                             SignallingServerState state);

SoupWebsocketConnection*
                    signalling_hub_get_websocket_connection (SignallingHub* hub);


#endif // !__SESSION_CORE_SIGNALLING_H