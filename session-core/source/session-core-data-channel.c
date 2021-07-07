#include <session-core-data-channel.h>
#include <gst\gst.h>
#include <glib-2.0\glib.h>
#include <gst/webrtc/webrtc_fwd.h>


#include <session-core.h>
#include <session-core-type.h>
#include <session-core-message.h>
#include <session-core-pipeline.h>













struct _WebRTCHub
{
    GstWebRTCDataChannel* hid;
    GstWebRTCDataChannel* control;
};





WebRTCHub* 
webrtchub_initialize()
{
    WebRTCHub* hub = malloc(sizeof(WebRTCHub));
    return hub;
}















/// <summary>
/// handle data message from client,
/// if destination is not session core, forward it using shared memory
/// </summary>
/// <param name="datachannel"></param>
/// <param name="byte"></param>
/// <param name="core"></param>
void
control_channel_on_message_data(GObject* datachannel,
    GBytes* byte,
    SessionCore* core)
{
    gsize size;

    gchar* text = g_bytes_get_data(byte,&size);
    gchar* message = g_strndup(text, size);

    session_core_on_message(core, message); 
}

/// <summary>
/// handle data from hid byte data channel,
/// this channel only responsible for parsing HID device 
/// </summary>
/// <param name="datachannel"></param>
/// <param name="data"></param>
/// <param name="core"></param>
void
hid_channel_on_message_data(GObject* datachannel,
    GBytes* data,
    SessionCore* core)
{
    gint size = g_bytes_get_size(data);
    gpointer buffer = g_bytes_get_data(data, size);

}

void
channel_on_message_string(GObject* dc,
    gchar* message,
    SessionCore* core){}

void
channel_on_open(GObject* dc,
    SessionCore* core){}


void
channel_on_close_and_error(GObject* dc,
    SessionCore* core)
{
    gchar* label;
    g_object_get_property(dc, "label", &label);


    Pipeline* pipe = session_core_get_pipeline(core);
    GstElement* webrtcbin = pipeline_get_webrtc_bin(pipe);

    WebRTCHub* hub = session_core_get_rtc_hub(core);

    if (label == "HID")
    {
        g_free(hub->hid);

        g_signal_emit_by_name(webrtcbin, "create-data-channel", "HID", NULL,
            &hub->hid);


        g_signal_connect(hub->hid, "on-error",
            G_CALLBACK(channel_on_close_and_error), core);
        g_signal_connect(hub->hid, "on-open",
            G_CALLBACK(channel_on_open), core);
        g_signal_connect(hub->hid, "on-close",
            G_CALLBACK(channel_on_close_and_error), core);
        g_signal_connect(hub->hid, "on-message-string",
            G_CALLBACK(channel_on_message_string), core);
        g_signal_connect(hub->hid, "on-message-data",
            G_CALLBACK(hid_channel_on_message_data), core);
    }
    else if(label == "Control")
    {
        g_free(hub->hid);

        g_signal_emit_by_name(webrtcbin, "create-data-channel", "HID", NULL,
            &hub->hid);

        g_signal_connect(hub->control, "on-error",
            G_CALLBACK(channel_on_close_and_error), core);
        g_signal_connect(hub->control, "on-open",
            G_CALLBACK(channel_on_open), core);
        g_signal_connect(hub->control, "on-close",
            G_CALLBACK(channel_on_close_and_error), core);
        g_signal_connect(hub->control, "on-message-string",
            G_CALLBACK(channel_on_message_string), core);
        g_signal_connect(hub->control, "on-message-data",
            G_CALLBACK(control_channel_on_message_data), core);
    }

}















/// <summary>
/// Connect webrtcbin to data channel, connect data channel signal to callback function
/// </summary>
/// <param name="core"></param>
/// <param name="user_data"></param>
/// <returns></returns>
gboolean
connect_data_channel_signals(SessionCore* core)
{

    WebRTCHub* hub = session_core_get_rtc_hub(core);
    Pipeline* pipe = session_core_get_pipeline(core);
    GstElement* webrtcbin = pipeline_get_webrtc_bin(pipe);


    g_signal_emit_by_name(webrtcbin, "create-data-channel", "HID", NULL,
        &hub->hid);
    g_signal_emit_by_name(webrtcbin, "create-data-channel", "Control", NULL,
        &hub->control);

    if (hub->control && hub->hid)
    {
        g_signal_connect(hub->control, "on-error",
            G_CALLBACK(channel_on_close_and_error), core);
        g_signal_connect(hub->control, "on-open",
            G_CALLBACK(channel_on_open), core);
        g_signal_connect(hub->control, "on-close",
            G_CALLBACK(channel_on_close_and_error), core);
        g_signal_connect(hub->control, "on-message-string",
            G_CALLBACK(channel_on_message_string), core);
        g_signal_connect(hub->control, "on-message-data",
            G_CALLBACK(control_channel_on_message_data), core);

        g_signal_connect(hub->hid, "on-error",
            G_CALLBACK(channel_on_close_and_error), core);
        g_signal_connect(hub->hid, "on-open",
            G_CALLBACK(channel_on_open), core);
        g_signal_connect(hub->hid, "on-close",
            G_CALLBACK(channel_on_close_and_error), core);
        g_signal_connect(hub->hid, "on-message-string",
            G_CALLBACK(channel_on_message_string), core);
        g_signal_connect(hub->hid, "on-message-data",
            G_CALLBACK(hid_channel_on_message_data), core);
        return TRUE;
    }
    else
        return FALSE;    
}








GstWebRTCDataChannel*
webrtc_hub_get_control_data_channel(WebRTCHub* hub)
{
    return hub->control;
}