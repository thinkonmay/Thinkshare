#include "Framework.h"
#include "Handle data channel.h"
#include "session-core.h"
#include "Session.h"




void
control_channel_on_message_data(GObject* datachannel,
    GBytes* byte,
    SessionCore* core)
{
    gpointer data = g_bytes_get_data(byte,sizeof(byte));




}

void
control_channel_on_message_string(GObject* dc,
    gchar* message,
    SessionCore* core)
{
    g_printerr("unknown messsage");
}
void
control_channel_on_error(GObject* dc,
    SessionCore* core)
{

    session_core_end("Data channel error",core, APP_STATE_ERROR);
}

void
control_channel_on_close(GObject* dc,
    SessionCore* core)
{
    session_core_end("Data channel closed",core, APP_STATE_UNKNOWN);
}

void
control_channel_on_open(GObject* dc,
    SessionCore* core)
{
    g_print("data channel opened\n");
    g_signal_emit_by_name(dc, "send-string", "Connect data channel confirmed");
}








void
hid_channel_on_message_data(GObject* datachannel,
    GBytes* data,
    SessionCore* core)
{
    gint size = g_bytes_get_size(data);
    gpointer buffer = g_bytes_get_data(data,size);

    


}

void
hid_channel_on_message_string(GObject* dc,
    gchar* message,
    SessionCore* core)
{
    g_printerr("unknown messsage");
}
void
hid_channel_on_error(GObject* dc, 
    SessionCore* core)
{

    session_core_end("Data channel error",core, APP_STATE_ERROR);
}

void
hid_channel_on_close(GObject* dc,
    SessionCore* core)
{
    session_core_end("Data channel closed",core, APP_STATE_UNKNOWN);
}

void
hid_channel_on_open(GObject* dc,
    SessionCore* core)
{
    g_print("data channel opened\n");
    g_signal_emit_by_name(dc, "send-string", "Connect data channel confirmed");
}






gboolean
session_core_connect_data_channel_signals(SessionCore* core,
    gpointer user_data)
{
    /* We need to transmit this ICE candidate to the browser via the websockets
 * signalling server. Incoming ice candidates from the browser need to be
 * added by us too, see on_server_message() */

    WebRTCHub* hub = session_core_get_rtc_hub(core);
    Pipeline* pipe = session_core_get_pipeline(core);

    CoreState state;

    g_object_get_property(core, "core-state", &state);
    if (state != HANDSHAKE_SIGNAL_CONNECTED)
    {
        g_print("waiting for handshake connected signal");
        sleep(1);
    }

    g_signal_emit_by_name(pipe->webrtcbin, "create-data-channel", "HID", NULL,
        &hub->hid);
    g_signal_emit_by_name(pipe->webrtcbin, "create-data-channel", "Control", NULL,
        &hub->control);

    if (hub->control && hub->hid)
    {
        g_print("Created two data channels\n");

        g_signal_connect(hub->control, "on-error",
            G_CALLBACK(control_channel_on_error), core);
        g_signal_connect(hub->control, "on-open",
            G_CALLBACK(control_channel_on_open), core);
        g_signal_connect(hub->control, "on-close",
            G_CALLBACK(control_channel_on_close), core);
        g_signal_connect(hub->control, "on-message-string",
            G_CALLBACK(control_channel_on_message_string), core);
        g_signal_connect(hub->control, "on-message-data",
            G_CALLBACK(control_channel_on_message_data), core);

        g_signal_connect(hub->hid, "on-error",
            G_CALLBACK(hid_channel_on_error), core);
        g_signal_connect(hub->hid, "on-open",
            G_CALLBACK(hid_channel_on_open), core);
        g_signal_connect(hub->hid, "on-close",
            G_CALLBACK(hid_channel_on_close), core);
        g_signal_connect(hub->hid, "on-message-string",
            G_CALLBACK(hid_channel_on_message_string), core);
        g_signal_connect(hub->hid, "on-message-data",
            G_CALLBACK(hid_channel_on_message_data), core);
    }
    else {
        g_print("Could not create  data channel!\n");
        return FALSE;
    }
    return TRUE;
}