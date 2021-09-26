#include <file-transfer-pipeline.h>
#include <file-transfer-type.h>
#include <file-transfer-data-channel.h>
#include <file-transfer-signalling.h>


#include <general-constant.h>
#include <logging.h>
#include <qoe.h>
#include <exit-code.h>

#include <gst\gst.h>
#include <glib-2.0\glib.h>
#include <gst\webrtc\webrtc.h>
#include <gst\rtp\gstrtppayloads.h>

#include <Windows.h>





WebRTChub*
webrtcbin_initialize(FileTransferSvc* core)
{
    SignallingHub* hub = file_transfer_get_signalling_hub(core);

    

    return &pipeline;
}

static gboolean
start_file_transfer(FileTransferSvc* core)
{
    GstStateChangeReturn ret;
    WebRTChub* pipe = file_transfer_get_pipeline(core);

    ret = GST_IS_ELEMENT(pipe->pipeline);    

    ret = gst_element_set_state(GST_ELEMENT(pipe->pipeline), GST_STATE_PLAYING);
    if (ret == GST_STATE_CHANGE_FAILURE)
    {
        GError error;
        error.message = "Fail to start pipeline, this may due to pipeline setup failure";
        file_transfer_finalize(core, PIPELINE_ERROR_EXIT,&error);
    }
    write_to_log_file(SESSION_CORE_GENERAL_LOG,"Starting pipeline");
    return TRUE;
}




/// <summary>
/// connect webrtc bin to ice and sdp signal handler
/// </summary>
/// <param name="core"></param>
static void
connect_signalling_handler(FileTransferSvc* core)
{
    WebRTChub* pipe = file_transfer_get_pipeline(core);
    SignallingHub* hub = file_transfer_get_signalling_hub(core);

    g_main_context_push_thread_default(file_transfer_get_main_context(core));
    /* Add stun server */
    g_object_set(pipe->webrtcbin, "stun-server", 
       "stun://stun.thinkmay.net:3478", NULL);

    g_signal_emit_by_name (pipe->webrtcbin, "add-turn-server", 
        signalling_hub_get_turn_server(hub), NULL);


    /* This is the gstwebrtc entry point where we create the offer and so on. It
     * will be called when the pipeline goes to PLAYING. */
    g_signal_connect(pipe->webrtcbin, "on-negotiation-needed",
        G_CALLBACK(on_negotiation_needed), core);
    g_signal_connect(pipe->webrtcbin, "on-ice-candidate",
        G_CALLBACK(send_ice_candidate_message), core);
    g_signal_connect(pipe->webrtcbin, "notify::ice-gathering-state",
        G_CALLBACK(on_ice_gathering_state_notify), core);
    g_main_context_pop_thread_default(file_transfer_get_main_context(core));
}





gpointer
setup_pipeline(FileTransferSvc* core)
{
    SignallingHub* signalling = file_transfer_get_signalling_hub(core);
    WebRTChub* pipe = file_transfer_get_pipeline(core);

    connect_signalling_handler(core);
    
    GError* error = NULL
    gst_parse_launch("webrtcbin",&error);
    if(error != NULL)
        return NULL;

    gst_element_change_state(pipe->pipeline, GST_STATE_READY);

    connect_data_channel_signals(core);

    start_file_transfer(core);

    file_transfer_set_state(core, REMOTE_CONNECT_STARTED);
    signalling_hub_set_peer_call_state(signalling, PEER_CALL_DONE);
}







GstElement*
pipeline_get_webrtc_bin(WebRTChub* pipe)
{
    return pipe->webrtcbin;
}

GstElement*
pipeline_get_pipline(WebRTChub* pipe)
{
    return pipe->pipeline;
}
