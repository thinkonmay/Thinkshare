#include <remote-app-type.h>
#include "remote-app.h"

#include <glib.h>
#include "remote-app-type.h"
#include <gst/webrtc/webrtc_fwd.h>

/// <summary>
/// Connect data channel (derived type of gobject) to corresponding signal handler 
/// </summary>
/// <param name="core"></param>
/// <param name="user_data"></param>
/// <returns></returns>
gboolean                connect_data_channel_signals                (RemoteApp* core);

/// <summary>
/// intialize webrtc hub by assigning memory to webrtchub struct
/// </summary> 
/// <returns></returns>
WebRTCHub*				webrtchub_initialize						();





