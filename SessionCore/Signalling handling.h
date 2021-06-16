#include "Framework.h"
#include "session-core.h"
#include "Session.h"



/*Used as webrtcbin callback function*/
void
send_ice_candidate_message(GstElement* webrtc G_GNUC_UNUSED,
    guint mlineindex,
    gchar* candidate,
    SessionCore* core G_GNUC_UNUSED);

void
on_negotiation_needed(GstElement* element,
    SessionCore* core);


void
on_ice_gathering_state_notify(GstElement* webrtcbin,
    GParamSpec* pspec,
    gpointer user_data);










gchar*
get_string_from_json_object(JsonObject* object);

void
on_server_closed(SoupWebsocketConnection* conn G_GNUC_UNUSED,
    SessionCore* core G_GNUC_UNUSED);

/* Answer created by our pipeline, to be sent to the peer */
void
on_answer_created(GstPromise* promise,
    SessionCore* core);

void
on_server_message(SoupWebsocketConnection* conn,
    SoupWebsocketDataType type,
    GBytes* message,
    SessionCore* core);

void
on_server_connected(SoupSession* session,
    GAsyncResult* res,
    SessionCore* core);

void
connect_to_websocket_signalling_server_async(SessionCore* core,
    gpointer user_data);

gboolean
register_with_server(SessionCore* core);
