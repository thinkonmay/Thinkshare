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

#include <gst/gst.h>
#include <glib-2.0/glib.h>
#include <gst/webrtc/webrtc_fwd.h>
#include <Windows.h>
#include <general-constant.h>













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



WebRTCHub* 
webrtchub_initialize()
{
    static WebRTCHub hub;
    return &hub;
}















/// <summary>
/// handle data message from client,
/// if destination is not session core, forward it using shared memory
/// </summary>
/// <param name="datachannel"></param>
/// <param name="byte"></param>
/// <param name="core"></param>
static void
control_channel_on_message_data(GObject* datachannel,
    GBytes* byte,
    RemoteApp* core)
{
    return;
}

/// <summary>
/// handle data from hid byte data channel,
/// this channel only responsible for parsing HID device 
/// </summary>
/// <param name="datachannel"></param>
/// <param name="data"></param>
/// <param name="core"></param>
static void
hid_channel_on_message_data(GObject* datachannel,
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
    JsonObject* object = get_json_object_from_string(message,&error);
	if(!error == NULL || object == NULL) {return;}

    Opcode opcode = json_object_get_int_member(object, "Opcode");

    if (opcode == MOUSE_UP)
    {
        INPUT mouse;
        ZeroMemory(&mouse, sizeof(mouse));
        mouse.type = INPUT_MOUSE;
        mouse.mi.dx = (LONG)json_object_get_int_member(object, "dX");
        mouse.mi.dy = (LONG)json_object_get_int_member(object, "dY");
        gint button = json_object_get_int_member(object, "button");
        if(button == 0){
            mouse.mi.dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTUP | MOUSEEVENTF_VIRTUALDESK;
        }else if(button == 1){
            mouse.mi.dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MIDDLEUP | MOUSEEVENTF_VIRTUALDESK;
        }else if (button == 2) {
            mouse.mi.dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_RIGHTUP | MOUSEEVENTF_VIRTUALDESK;
        }

        mouse.mi.mouseData = 0;
        mouse.mi.time = 0;
        SendInput(1, &mouse, sizeof(mouse));
    }
    else if (opcode == MOUSE_DOWN)
    {
        INPUT mouse;
        ZeroMemory(&mouse, sizeof(mouse));
        mouse.type = INPUT_MOUSE;
        mouse.mi.dx = (LONG)json_object_get_int_member(object, "dX");
        mouse.mi.dy = (LONG)json_object_get_int_member(object, "dY");
        gint button = json_object_get_int_member(object, "button");
        if (button == 0) {
            mouse.mi.dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_VIRTUALDESK;
        }
        else if (button == 1) {
            mouse.mi.dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MIDDLEDOWN | MOUSEEVENTF_VIRTUALDESK;
        }
        else if (button == 2) {
            mouse.mi.dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_VIRTUALDESK;
        }

        mouse.mi.mouseData = 0;
        mouse.mi.time = 0;
        SendInput(1, &mouse, sizeof(mouse));
    }
    else if (opcode == MOUSE_MOVE)
    {
        INPUT mouse;
        ZeroMemory(&mouse, sizeof(mouse));
        mouse.type = INPUT_MOUSE;
        mouse.mi.dx = (LONG)json_object_get_int_member(object, "dX");
        mouse.mi.dy = (LONG)json_object_get_int_member(object, "dY");
        mouse.mi.dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE;
        SendInput(1, &mouse, sizeof(mouse));
    }
    else if(opcode == MOUSE_WHEEL)
    {
        INPUT mouse;
        ZeroMemory(&mouse, sizeof(mouse));
        mouse.type = INPUT_MOUSE;

        mouse.mi.dx = (LONG)json_object_get_int_member(object, "dX");
        mouse.mi.dy = (LONG)json_object_get_int_member(object, "dY");
        mouse.mi.dwFlags = MOUSEEVENTF_WHEEL;
        mouse.mi.mouseData = (LONG)json_object_get_int_member(object, "WheeldY");
        mouse.mi.time = 0;
        SendInput(1, &mouse, sizeof(mouse));
    }
    else if (opcode == KEYUP)
    {
        INPUT keyboard;
        ZeroMemory(&keyboard, sizeof(keyboard));
        keyboard.type = INPUT_KEYBOARD;
        keyboard.ki.wVk = convert_javascript_key_to_window_key(
            json_object_get_string_member(object, "wVk"));
        keyboard.ki.dwFlags = KEYEVENTF_KEYUP;
        keyboard.ki.time = 0;
        SendInput(1, &keyboard, sizeof(keyboard));
    }
    else if (opcode == KEYDOWN)
    {
        INPUT keyboard;
        ZeroMemory(&keyboard, sizeof(keyboard));
        keyboard.type =  INPUT_KEYBOARD;
        keyboard.ki.wVk = convert_javascript_key_to_window_key(
            json_object_get_string_member(object, "wVk"));
        keyboard.ki.time = 0;
        SendInput(1, &keyboard, sizeof(keyboard));
    }
    else if (opcode == POINTER_LOCK)
    {
        toggle_pointer(json_object_get_boolean_member(object,"Value"),core);
    }
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
    JsonObject* object = get_json_object_from_string(message,&error);
	if(!error == NULL || object == NULL) {return;}

	Module     from =		json_object_get_int_member(object, "From");

    if(from != CLIENT_MODULE)
    {
        /// warning if client try to send uknown package
        GError error;
        error.message = message;
        remote_app_finalize(core,UNKNOWN_PACKAGE_FROM_CLIENT,&error);
        return;
    }
    remote_app_on_message(core, message);
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
on_hid_data_channel(GstElement* webrtc,
    GstWebRTCDataChannel* channel,
    gpointer data)
{
    RemoteApp* core = (RemoteApp*)data;
    WebRTCHub* hub = remote_app_get_rtc_hub(core);
    hub->hid = channel;

    g_main_context_push_thread_default(remote_app_get_main_context(core));
    g_signal_connect(hub->hid, "on-error",
        G_CALLBACK(channel_on_close_and_error), core);
    g_signal_connect(hub->hid, "on-open",
        G_CALLBACK(channel_on_open), core);
    g_signal_connect(hub->hid, "on-close",
        G_CALLBACK(channel_on_close_and_error), core);
    g_signal_connect(hub->hid, "on-message-string",
        G_CALLBACK(hid_channel_on_message_string), core);
    g_signal_connect(hub->hid, "on-message-data",
        G_CALLBACK(hid_channel_on_message_data), core);
    g_main_context_pop_thread_default(remote_app_get_main_context(core));
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
    g_signal_emit_by_name(webrtcbin, "create-data-channel", "Control", NULL, &hub->control);
    g_signal_connect(webrtcbin, "on-data-channel", G_CALLBACK(on_hid_data_channel), core);
    g_signal_connect(hub->control, "on-error",
        G_CALLBACK(channel_on_close_and_error), core);
    g_signal_connect(hub->control, "on-open",
        G_CALLBACK(channel_on_open), core);
    g_signal_connect(hub->control, "on-close",
        G_CALLBACK(channel_on_close_and_error), core);
    g_signal_connect(hub->control, "on-message-string",
        G_CALLBACK(control_channel_on_message_string), core);
    g_signal_connect(hub->control, "on-message-data",
        G_CALLBACK(control_channel_on_message_data), core);
    return TRUE;
}



GstWebRTCDataChannel*
webrtc_hub_get_control_data_channel(WebRTCHub* hub)
{
    return hub->control;
}