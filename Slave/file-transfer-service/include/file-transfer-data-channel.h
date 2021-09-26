#include "file-transfer.h"

#include <glib.h>
#include "file-transfer-type.h"
#include <gst/webrtc/webrtc_fwd.h>

/// <summary>
/// Connect data channel (derived type of gobject) to corresponding signal handler 
/// </summary>
/// <param name="core"></param>
/// <param name="user_data"></param>
/// <returns></returns>
gboolean                connect_data_channel_signals                (FileTransferSvc* core);

/// <summary>
/// intialize webrtc hub by assigning memory to webrtchub struct
/// </summary> 
/// <returns></returns>
WebRTChub*				webrtchub_initialize						();





