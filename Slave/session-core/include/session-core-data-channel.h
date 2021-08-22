#include <session-core-type.h>
#include "session-core.h"

#include <glib.h>
#include "session-core-type.h"
#include <gst/webrtc/webrtc_fwd.h>

/// <summary>
/// Connect data channel (derived type of gobject) to corresponding signal handler 
/// </summary>
/// <param name="core"></param>
/// <param name="user_data"></param>
/// <returns></returns>
gboolean                connect_data_channel_signals                (SessionCore* core);


WebRTCHub*				webrtchub_initialize						();





