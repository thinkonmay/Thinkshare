#include "Framework.h"
#include "Variable.h"


extern enum AppState app_state;

void
send_ice_candidate_message(GstElement* webrtc G_GNUC_UNUSED,
    guint mlineindex,
    gchar* candidate,
    gpointer user_data G_GNUC_UNUSED);


void
send_sdp_to_peer(GstWebRTCSessionDescription* desc);

/* Offer created by our pipeline, to be sent to the peer */
void
on_offer_created(GstPromise* promise,
    gpointer user_data);

void
on_negotiation_needed(GstElement* element,
    gpointer user_data);


void
on_ice_gathering_state_notify(GstElement* webrtcbin,
    GParamSpec* pspec,
    gpointer user_data);

gchar*
get_string_from_json_object(JsonObject* object);

void
on_server_closed(SoupWebsocketConnection* conn G_GNUC_UNUSED,
    gpointer user_data G_GNUC_UNUSED);

/* Answer created by our pipeline, to be sent to the peer */
void
on_answer_created(GstPromise* promise,
    gpointer user_data);

void
on_offer_set(GstPromise* promise,
    gpointer user_data);

void
on_offer_received(GstSDPMessage* sdp);

void
on_server_message(SoupWebsocketConnection* conn,
    SoupWebsocketDataType type,
    GBytes* message,
    gpointer user_data);

void
on_server_connected(SoupSession* session,
    GAsyncResult* res,
    SoupMessage* msg);

void
connect_to_websocket_server_async(void);

gboolean
register_with_server(void);
