#include "Framework.h"
#include "Variable.h"


void
data_channel_on_error(GObject* dc, gpointer user_data);

void
data_channel_on_open(GObject* dc, gpointer user_data);

void
data_channel_on_close(GObject* dc, gpointer user_data);

void
data_channel_on_message_string(GObject* dc,
                               gchar* str,
                               gpointer user_data);

void
connect_data_channel_signals(GObject* data_channel, const gchar* channel_type);

void
on_data_channel(GstElement* webrtc,
                GObject* data_channel,
                gpointer user_data);

void pipe_byte(GObject* dc,
    GBytes* message,
    gpointer user_data);

void pipe_string(GObject* dc,
    gchar* message,
    gpointer user_data);