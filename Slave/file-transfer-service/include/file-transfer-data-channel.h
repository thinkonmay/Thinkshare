#ifndef __FILE_TRANSFER_DATA_CHANNEL_H__
#define __FILE_TRANSFER_DATA_CHANNEL_H__
#include "file-transfer.h"
#include "file-transfer-type.h"

#include <glib.h>
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
WebRTCDataChannelPool*	init_datachannel_pool						();


/// <summary>
/// 
/// </summary>
/// <param name="channel"></param>
/// <param name="file"></param>
void                    webrtc_data_channel_get_file                (WebRTCDataChannelPool* channel,
                                                                     gchar* file);

#endif



