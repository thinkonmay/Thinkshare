#include <session-core-data-channel.h>
#include <gst\gst.h>
#include <glib-2.0\glib.h>
#include <gst/webrtc/webrtc_fwd.h>
#include <Windows.h>

#include <session-core.h>
#include <session-core-type.h>
#include <session-core-message.h>
#include <session-core-pipeline.h>
#include <session-core-remote-config.h>













struct _WebRTCHub
{
    GstWebRTCDataChannel* hid;
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
    gsize size;
    gchar* buffer = g_bytes_get_data(data, &size);
    gchar* message = g_strndup(buffer, size);

    JsonNode* root;
    JsonObject* object;

    JsonParser* parser = json_parser_new();
    json_parser_load_from_data(parser, data, -1, NULL);

    root = json_parser_get_root(parser);
    object = json_node_get_object(root);

    Opcode opcode = json_object_get_int_member(object, "HidOpcode");

    if (opcode == MOUSE)
    {
        INPUT mouse[1];
        ZeroMemory(&mouse, sizeof(mouse[0]));
        mouse[0].type = INPUT_MOUSE;
        mouse[0].mi.dx = (LONG)json_object_get_int_member(object, "dX");
        mouse[0].mi.dy = (LONG)json_object_get_int_member(object, "dY");
        if(json_object_get_boolean_member(object,"Relative"))
            mouse[0].mi.mouseData = (DWORD)json_object_get_int_member(object, "mouseData");
        else
            mouse[0].mi.mouseData = (DWORD)json_object_get_int_member(object, "mouseData") || MOUSEEVENTF_ABSOLUTE;
        mouse[0].mi.dwFlags = (DWORD)json_object_get_int_member(object, "dwFlags");
        mouse[0].mi.time = 0;
        SendInput(1, mouse, sizeof(mouse[0]));
    }
    else if (opcode == KEYBOARD)
    {
        INPUT keyboard[1];
        ZeroMemory(&keyboard, sizeof(keyboard[0]));
        keyboard[0].type = INPUT_KEYBOARD;
        keyboard[0].ki.wVk = (WORD)json_object_get_int_member(object,"wVk");
        if (json_object_get_boolean_member(object, "IsUp"))
            keyboard[0].ki.dwFlags = KEYEVENTF_KEYUP;
        keyboard[0].ki.time = 0;
        SendInput(1, keyboard, sizeof(keyboard[0]));
    }
    else if (opcode == QOE_REPORT)
    {
        QoE* qoe = session_core_get_qoe(core);
        qoe_update_quality(qoe,
            json_object_get_int_member(object, "FrameRate"),
            json_object_get_int_member(object, "Latency"),
            json_object_get_int_member(object, "AudioBitrate"),
            json_object_get_int_member(object, "VideoBitrate"));
    }


    g_free(buffer);
}

void
channel_on_message_string(GObject* dc,
    gchar* message,
    SessionCore* core)
{
    return;
}

void
channel_on_open(GObject* dc,
    SessionCore* core)
{
    return;
}


void
channel_on_close_and_error(GObject* dc,
    SessionCore* core)
{
    gchar* label;
    g_object_get_property(dc, "label", &label);

    report_session_core_error(core, DATA_CHANNEL_ERROR);

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

    if ((hub->control == NULL) || 
        (hub->hid == NULL))
    {
        return FALSE;
    }


    g_main_context_push_thread_default(sessioin_core_get_main_context(core));
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
    g_main_context_pop_thread_default(sessioin_core_get_main_context(core));

    return TRUE;
}








GstWebRTCDataChannel*
webrtc_hub_get_control_data_channel(WebRTCHub* hub)
{
    return hub->control;
}