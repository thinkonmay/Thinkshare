#include <file-transfer-webrtcbin.h>
#include <file-transfer-type.h>
#include <file-transfer-data-channel.h>
#include <file-transfer-signalling.h>


#include <general-constant.h>
#include <logging.h>
#include <qoe.h>
#include <exit-code.h>

#include <gst/gst.h>
#include <glib-2.0/glib.h>
#include <gst/webrtc\webrtc.h>
#include <gst/rtp\gstrtppayloads.h>

#include <Windows.h>


#define STUN_SERVER "stun://stun.thinkmay.net:3478"

struct _WebRTCbin
{
    GstElement* pipeline;

    GstElement* webrtcbin;

    gchar* turn;
};

static WebRTCbin webrtc_hub = {0};


WebRTCbin*
webrtcbin_initialize(FileTransferService* core)
{
    return &webrtc_hub;
}

static gboolean
start_file_transfer(FileTransferService* core)
{
    GstStateChangeReturn ret;
    WebRTCbin* bin = file_transfer_get_webrtcbin(core);
    ret = gst_element_set_state(GST_ELEMENT(bin->pipeline), GST_STATE_PLAYING);
    if (ret == GST_STATE_CHANGE_FAILURE)
    {
        GError error;
        error.message = "Fail to start pipeline, this may due to pipeline setup failure";
        file_transfer_finalize(core, PIPELINE_ERROR_EXIT,&error);
    }
    return TRUE;
}




/// <summary>
/// connect webrtc bin to ice and sdp signal handler
/// </summary>
/// <param name="core"></param>
static void
connect_signalling_handler(FileTransferService* service)
{
    WebRTCbin* bin = file_transfer_get_webrtcbin(service);
    FileTransferSignalling* hub = file_transfer_get_signalling_hub(service);

    /* Add stun server */
    g_object_set(bin->webrtcbin, "stun-server", 
       STUN_SERVER, NULL);
    g_signal_emit_by_name (bin->webrtcbin, "add-turn-server", 
        bin->turn, NULL);


    /* This is the gstwebrtc entry point where we create the offer and so on. It
     * will be called when the pipeline goes to PLAYING. */
    g_signal_connect(bin->webrtcbin, "on-negotiation-needed",
        G_CALLBACK(on_negotiation_needed), service);
    g_signal_connect(bin->webrtcbin, "on-ice-candidate",
        G_CALLBACK(send_ice_candidate_message), service);
    g_signal_connect(bin->webrtcbin, "notify::ice-gathering-state",
        G_CALLBACK(on_ice_gathering_state_notify), service);
}





void
setup_webrtcbin(FileTransferService* core)
{
    WebRTCbin* bin = file_transfer_get_webrtcbin(core);
    connect_signalling_handler(core);
    
    GError* error = NULL;
    bin->pipeline = gst_parse_launch("webrtcbin",&error);
    if(error != NULL)
        return NULL;

    gst_element_change_state(bin->pipeline, GST_STATE_READY);

    connect_data_channel_signals(core);

    start_file_transfer(core);
}



void
webrtcbin_get_turn_connection(WebRTCbin* hub, 
                              gchar* turn_connection)
{
    hub->turn = turn_connection;
}






GstElement*
webrtcbin_get_element(WebRTCbin* bin)
{
    return bin->webrtcbin;
}

GstElement*
webrtcbin_get_pipline(WebRTCbin* bin)
{
    return bin->pipeline;
}
