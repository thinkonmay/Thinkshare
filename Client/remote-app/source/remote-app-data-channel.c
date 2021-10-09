#include <remote-app-data-channel.h>
#include <remote-app.h>
#include <remote-app-type.h>
#include <remote-app-message.h>
#include <remote-app-pipeline.h>
#include <remote-app-remote-config.h>


#include <logging.h>
#include <human-interface-opcode.h>
#include <exit-code.h>
#include <key-convert.h>
#include <module-code.h>

#include <gst/gst.h>
#include <glib-2.0/glib.h>
#include <gst/webrtc/webrtc_fwd.h>
#include <Windows.h>
#include <general-constant.h>
#include <json-glib/json-glib.h>













struct _WebRTCHub
{
    /// <summary>
    /// hid datachannel responsible for receive human interface device input from user,
    /// </summary>
    GstWebRTCDataChannel* hid;
    /// <summary>
    /// control datachannel responsible for receive other command from user include update current qoe metric
    /// to adjust video stream quality
    /// </summary>
    GstWebRTCDataChannel* control;
};


static WebRTCHub rtc_hub;

WebRTCHub* 
webrtchub_initialize()
{
    return &rtc_hub;
}















/// <summary>
/// handle data from hid byte data channel,
/// this channel only responsible for parsing HID device 
/// </summary>
/// <param name="datachannel"></param>
/// <param name="data"></param>
/// <param name="core"></param>
static void
data_channel_on_message_data(GObject* datachannel,
    GBytes* data,
    RemoteApp* core)
{
    return;
}


/// <summary>
/// handle message from hid datachannel  
/// </summary>
/// <param name="dc"></param>
/// <param name="message"></param>
/// <param name="core"></param>
static void
hid_channel_on_message_string(GObject* dc,
    gchar* message,
    RemoteApp* core)
{
    QoE* qoe = remote_app_get_qoe(core);

    GError* error = NULL;
    JsonParser* parser = json_parser_new();
    JsonObject* object = get_json_object_from_string(message,&error,parser);
	if(!error == NULL || object == NULL) {return;}

    Opcode opcode = json_object_get_int_member(object, "Opcode");
    g_object_unref(parser);
}


/// <summary>
/// handle message from user through control data channel
/// </summary>
/// <param name="dc"></param>
/// <param name="message"></param>
/// <param name="core"></param>
static void
control_channel_on_message_string(GObject* dc,
    gchar* message,
    RemoteApp* core)
{
    GError* error = NULL;
    JsonParser* parser = json_parser_new();
    JsonObject* object = get_json_object_from_string(message,&error,parser);
	if(!error == NULL || object == NULL) {return;}

    gint from =		json_object_get_int_member(object, "From");

    if(from != CLIENT_MODULE)
    {
        /// warning if client try to send uknown package
        GError error;
        error.message = message;
        remote_app_finalize(core,UNKNOWN_PACKAGE_FROM_CLIENT,&error);
        return;
    }
    remote_app_on_message(core, message);
    g_object_unref(parser);
}




static void
channel_on_open(GObject* dc,
                RemoteApp* core)
{
    return;
}


static void
channel_on_close_and_error(GObject* dc,
    RemoteApp* core)
{
    return;
}










/// <summary>
/// connect hid data channel with corresponding signal handler
/// </summary>
/// <param name="webrtc"></param>
/// <param name="channel"></param>
/// <param name="data"></param>
/// <returns></returns>
static gboolean
on_data_channel(GstElement* webrtc,
    GstWebRTCDataChannel* channel,
    gpointer data)
{
    RemoteApp* core = (RemoteApp*)data;
    WebRTCHub* hub = remote_app_get_rtc_hub(core);
    gchar* label;
    g_object_get(channel,"label",&label,NULL);

    if(!g_strcmp0(label,"Control"))
    {
        hub->control = channel;
        g_signal_connect(hub->control, "on-error",
            G_CALLBACK(channel_on_close_and_error), core);
        g_signal_connect(hub->control, "on-open",
            G_CALLBACK(channel_on_open), core);
        g_signal_connect(hub->control, "on-close",
            G_CALLBACK(channel_on_close_and_error), core);
        g_signal_connect(hub->control, "on-message-string",
            G_CALLBACK(control_channel_on_message_string), core);
        g_signal_connect(hub->control, "on-message-data",
            G_CALLBACK(data_channel_on_message_data), core);
    }
    else if (!g_strcmp0(label,"HID"))
    {
        hub->hid = channel;
        g_signal_connect(hub->hid, "on-error",
            G_CALLBACK(channel_on_close_and_error), core);
        g_signal_connect(hub->hid, "on-open",
            G_CALLBACK(channel_on_open), core);
        g_signal_connect(hub->hid, "on-close",
            G_CALLBACK(channel_on_close_and_error), core);
        g_signal_connect(hub->hid, "on-message-string",
            G_CALLBACK(hid_channel_on_message_string), core);
        g_signal_connect(hub->hid, "on-message-data",
            G_CALLBACK(data_channel_on_message_data), core);
    }
    else
    {
        return;
    }
}



/// <summary>
/// Connect webrtcbin to data channel, connect data channel signal to callback function
/// </summary>
/// <param name="core"></param>
/// <param name="user_data"></param>
/// <returns></returns>
gboolean
connect_data_channel_signals(RemoteApp* core)
{

    WebRTCHub* hub = remote_app_get_rtc_hub(core);
    Pipeline* pipe = remote_app_get_pipeline(core);
    GstElement* webrtcbin = pipeline_get_webrtc_bin(pipe);


    // connect data channel source
    g_signal_connect(webrtcbin, "on-data-channel",
        G_CALLBACK(on_data_channel), core);
    g_signal_connect(webrtcbin, "on-data-channel", 
        G_CALLBACK(on_data_channel), core);
    
}



GstWebRTCDataChannel*
webrtc_hub_get_control_data_channel(WebRTCHub* hub)
{
    return hub->control;
}