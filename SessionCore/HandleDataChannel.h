#include "Framework.h"
#include "Object.h"
#include "session-core.h"



/*hid channel signal handler*/
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


/*control channel handler*/
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


/// <summary>
/// Connect data channel (derived type of gobject) to corresponding signal handler 
/// </summary>
/// <param name="core"></param>
/// <param name="user_data"></param>
/// <returns></returns>
gboolean
connect_data_channel_signals(SessionCore* core, 
    gpointer user_data);



