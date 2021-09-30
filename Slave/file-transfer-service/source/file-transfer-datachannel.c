#include <file-transfer-data-channel.h>
#include <file-transfer.h>
#include <file-transfer-type.h>
#include <file-transfer-webrtcbin.h>


#include <logging.h>
#include <human-interface-opcode.h>
#include <exit-code.h>
#include <key-convert.h>

#include <gst/gst.h>
#include <glib-2.0/glib.h>
#include <gst/webrtc/webrtc_fwd.h>
#include <Windows.h>
#include <general-constant.h>






#define DC_POOL_SIZE 8






struct _WebRTCDataChannelPool
{
    GstWebRTCDataChannel* data_channel[DC_POOL_SIZE];
};


static WebRTCDataChannelPool pool_init;

WebRTCDataChannelPool* 
init_datachannel_pool()
{
    memset(&pool_init,0,sizeof(pool_init));
    return &pool_init;
}








/// <summary>
/// Connect webrtcbin to data channel, connect data channel signal to callback function
/// </summary>
/// <param name="core"></param>
/// <param name="user_data"></param>
/// <returns></returns>
gboolean
connect_data_channel_signals(FileTransferSvc* core)
{
    


    return TRUE;
}

