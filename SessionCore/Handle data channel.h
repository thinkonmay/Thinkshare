#include "Framework.h"
#include "Session.h"
#include "session-core.h"




void
hid_channel_on_error(GObject* dc,
    SessionCore* core);

void
hid_channel_on_open(GObject* dc, 
    SessionCore* core);

void
hid_channel_on_close(GObject* dc, 
    SessionCore* core);

void
hid_channel_on_message_string(GObject* dc,
    gchar* str,
    SessionCore* core);

void
control_channel_on_error(GObject* dc, 
    SessionCore* core);

void
control_channel_on_open(GObject* dc, 
    SessionCore* core);

void
control_channel_on_close(GObject* dc, 
    SessionCore* core);

void
control_channel_on_message_string(GObject* dc,
    gchar* str,
    SessionCore* core);

gboolean
session_core_connect_data_channel_signals(SessionCore* core);



