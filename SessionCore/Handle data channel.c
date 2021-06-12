#include "Framework.h"
#include "Variable.h"
#include "Handle data channel.h"
#include "Handle pipeline.h"
#include "RC config.h"
#include "CorePipeSink.h"
#include "Signalling handling.h"


using namespace msgpack;

void
unknown_message(GObject* dc, gpointer user_data)
{
    g_printerr("unknown messsage");
}
void
data_channel_on_error(GObject* dc, gpointer user_data)
{

    cleanup_and_quit_loop("Data channel error", APP_STATE_ERROR);
}

void
data_channel_on_close(GObject* dc,
    gpointer user_data)
{
    cleanup_and_quit_loop("Data channel closed", APP_STATE_UNKNOWN);
}

void
data_channel_on_open(GObject* dc,
    gpointer user_data)
{
    g_print("data channel opened\n");
    g_signal_emit_by_name(dc, "send-string", "Connect data channel confirmed");
}

void
data_channel_on_message_data(GObject* dc,
    GBytes* message,
    gpointer user_data)
{

}

/*IPC here*/
void
data_channel_on_message_string(GObject* dc,
    gchar* message,
    gpointer user_data)
{
    /*unpack message using messagepack and assign to tmp tuple*/
    type::tuple<int, int> tmp;

    unpack(message, strlen(message)).get().convert(tmp);

   
    switch (std::get<0>(tmp))
    {
    case    CHANGE_MEDIA_MODE:
        if (std::get<1>(tmp) == 1)
        {
            mode = VIDEO_PIORITY;
        }
        else
        {
            mode = AUDIO_PIORITY;
        }
    case    COMPOSE_BITRATE:
        set_dynamic_bitrate(std::get<1>(tmp));
    case    TOGGLE_CURSOR:
        g_print("toggled cursor");
        toggle_cursor = TRUE;
    case    CHANGE_RESOLUTION:
        
    case    CHANGE_FRAMERATE:
        framerate = std::get<1>(tmp);
    }
}


void
connect_data_channel_signals(GObject* data_channel , const gchar* channel_type)
{

    if (g_strcmp0(channel_type, "SessionCore"))
    {
        g_signal_connect(data_channel, "on-error",
            G_CALLBACK(data_channel_on_error), NULL);
        g_signal_connect(data_channel, "on-open",
            G_CALLBACK(data_channel_on_open), NULL);
        g_signal_connect(data_channel, "on-close",
            G_CALLBACK(data_channel_on_close), NULL);
        g_signal_connect(data_channel, "on-message-string",
            G_CALLBACK(data_channel_on_message_string), NULL);
        g_signal_connect(data_channel, "on-message-data",
            G_CALLBACK(data_channel_on_message_data), NULL);
    }
    if (g_strcmp0(channel_type, "SessionLoader"))
    {
        g_signal_connect(data_channel, "on-error",
            G_CALLBACK(unknown_message), NULL);
        g_signal_connect(data_channel, "on-open",
            G_CALLBACK(unknown_message), NULL);
        g_signal_connect(data_channel, "on-close",
            G_CALLBACK(unknown_message), NULL);
        g_signal_connect(data_channel, "on-message-string",
            G_CALLBACK(pipe_byte), NULL);
        g_signal_connect(data_channel, "on-message-data",
            G_CALLBACK(pipe_string), NULL);
    }

}




void
on_data_channel(GstElement* webrtc,
    GObject* data_channel,
    gpointer user_data)
{
    char* dc_name;
    g_object_get(data_channel, "label", &dc_name);
    if (g_strcmp0(dc_name, "SessionCore") || g_strcmp0(dc_name, "SessionLoader"))
    {
        connect_data_channel_signals(data_channel, dc_name);
    }
    else
    {
        g_printerr("unknown data channel");
    }

}

void pipe_byte(GObject* dc,
    GBytes* message,
    gpointer user_data)
{
    send_byte(file_handle_byte, message);
}
void pipe_string(GObject* dc,
    gchar* message,
    gpointer user_data)
{
    send_string(file_handle_byte, message);
}