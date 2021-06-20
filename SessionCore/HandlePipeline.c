#include "HandlePipeline.h"
#include "SignallingHandling.h"
#include "RcConfig.h"

gboolean
check_plugins()
{
    gboolean ret;
    GstPlugin* plugin;
    GstRegistry* registry;
    const gchar* needed[] = { "dx9screencapsrc",
    "nvh264enc","rtph264pay","pulsesrc",
    "opusencode","rtpopuspay","rtprtxqueue",
    "webrtcbin", NULL };

    registry = gst_registry_get();
    ret = TRUE;


    for (int i = 0; i < g_strv_length((gchar**)needed); i++) {
        plugin = gst_registry_find_plugin(registry, needed[i]);

        if (!plugin) 
{
            g_print("Required gstreamer plugin '%s' not found\n", needed[i]);
            ret = FALSE;
            continue;
        }
        gst_object_unref(plugin);
    }
    return ret;
}




/// <summary>
/// handle all media stream include both audio and video:
///add both audioand video pipe into webrtcbin, 
/// signal handler will we connect in connect_handler
/// </summary>
/// <param name="core"></param>
/// <param name="user_data"></param>
/// <returns></returns>
gboolean
setup_pipeline(SessionCore* core, 
    gpointer user_data)

{
    Pipeline* pipe = session_core_get_pipeline(core);
    WebRTCHub* hub = session_core_get_rtc_hub(core);
    SessionQoE* qoe = session_core_get_qoe(core);
    CoreState state;
    g_object_get_property(core, "core-state", &state);

    while (state != SERVER_REGISTERED)
    {
        g_print("Wating for server to connect");
        g_object_get_property(core, "core-state", &state);
    }

    if (!check_plugins())
    {
        session_core_end("plugin not found",core, APP_STATE_ERROR);
    }                                                                                                                                                                                                                   
    /*
    Adds the webrtcbin elments to the pipeline.
    The video and audio pipelines are linked to
    this in the build_video_pipline() and build_audio_pipeline() methods.
    */
    

    pipe->webrtcbin = gst_element_factory_make("webrtcbin", NULL);

    /*set policy-bundle*/
    g_object_set(pipe->webrtcbin, "bundle-policy", GST_WEBRTC_BUNDLE_POLICY_MAX_COMPAT);
    /* Add stun server */
    g_object_set(pipe->webrtcbin, "stun-server", hub->stun_server, NULL);

    /*Generate widnow screen capture element based */
    pipe->screencapture = gst_element_factory_make("dx9screencapsrc", NULL);

    /*create cappabilities for screen capture*/
    GstCaps* cap_screen_capture_src = gst_caps_new_simple
    ("video/x-raw",
        "format", "BGR",
        "width", qoe->screen_width,
        "height", qoe->screen_height,
        "framerate", qoe->framerate, 1, NULL);

    /*turn off screeen cursor*/
    g_object_set(pipe->screencapture, "cursor", FALSE, NULL);

    /*monitor to display*/
    g_object_set(pipe->screencapture, "monitor", 0, NULL);

    /*cuda upload responsible for upload memory from main memory to gpu memory*/
    pipe->videoupload = gst_element_factory_make("cudaupload", NULL);


    GstCaps* cap_video_upload_src = gst_caps_new_simple
    ("video/x-raw", NULL);

    /*convert BGR color space to I420 color space*/
    pipe->videoconvert = gst_element_factory_make("cuda-convert", NULL);

    GstCaps* cap_gpu_encode_sink;
    GstCaps* cap_gpu_encode_src;
    GstCaps* cap_rtp_packetize_src;



    if (g_strcmp0(qoe->video_encoder, "nvh264enc"))
    {
        /*
         ___________________________________________________________________
        gpu side
        */
        pipe->gpu_encode = gst_element_factory_make("nvh264enc", NULL);

        /*variable bitrate mode*/
        g_object_set(pipe->gpu_encode, "rc-mode", "vbr", NULL);

        /*low latency preset*/
        g_object_set(pipe->gpu_encode, "preset", "low-latency", NULL);

        /*set b-frame numbers property*/
        g_object_set(pipe->gpu_encode, "bframes", 0, NULL);

        /**/
        g_object_set(pipe->gpu_encode, "zerolatency", TRUE, NULL);

        /*handle dynamic controllable bitrate*/
        attach_bitrate_control(GST_OBJECT(pipe->gpu_encode), qoe->video_controller);

        /*create capability for encoder sink pad*/
        cap_gpu_encode_sink = gst_caps_new_simple
        ("video/x-raw",
            "format", "I420",
            "width", qoe->screen_width,
            "height",qoe->screen_height,
            "framerate", qoe->framerate, 1, "progressive", NULL);

        /*create capability for encoder source pad*/
        cap_gpu_encode_src = gst_caps_new_simple
        ("video/x-h264",
            "width", qoe->screen_width,
            "height", qoe->screen_height,
            "framerate", qoe->framerate,
            "stream-format", "byte-stream",
            "alignment", "au",
            "profile", "high", NULL);

        ///gst_video_encoder_get_latency() future realease 

        /*packetize video encoded byte-stream*/
        pipe->rtp_packetize = gst_element_factory_make("rtph264pay", NULL);

        cap_rtp_packetize_src = gst_caps_new_simple
        ("application/x-rtp",
            "media", "video",
            "payload", 123,
            "clock-rate", 90000,
            "encoding-name", "H264", NULL);


        /*set zero latency aggregate mode*/
        g_object_set(pipe->rtp_packetize, "aggregate-mote", "zero-latency", NULL);
    }
    if (g_strcmp0(qoe->video_encoder, "nvh265enc"))
    {
        /*Tôi cần Minh Quang bổ sung nvh265 encoder pipeline vào cho hệ thống
        *Tuy nhiên phần này sẽ cần được hoàn thành sau khi quang hoàn thiện xong 
        * thuật toán adaptive bitrate control
        */
        g_print("nvh265");
        
    }
    else
    {
        g_print("nvenc only");
    }


    /*sound capture part
    ______________________________________________________________________________
    */

    /*create and assign element named soundcapture*/
    pipe->soundcapture = gst_element_factory_make("pulsesrc", NULL);

    GstCaps* cap_sound_source = gst_caps_new_simple
    ("audio/x-raw",
        "format", "S16LE",
        "layout", "interleaved",
        ///"rate", abitrate,
        "channel", "2", NULL);


    /*create sound encoder*/
    pipe->soundencode = gst_element_factory_make("opusenc", NULL);

    /*handle dynamic control bitrate*/
    attach_bitrate_control(GST_OBJECT(pipe->soundencode), qoe->video_controller);

    /**/
    GstCaps* cap_sound_encoder_source = gst_caps_new_simple
    ("audio/x-raw"
        "format", "S16LE"
        "layout", "interleaved"
        ///"rate", abitrate,
        "channel", "2", NULL);

    /*create rtp packetizer forr audio stream*/
    pipe->rtp_sound = gst_element_factory_make("rtpopuspay", NULL);

    GstCaps* cap_rtp_opuspay_source = gst_caps_new_simple
    ("application/x-rtp",
        "media", "audio",
        "encoding-params", 2,
        "encoding-name", "OPUS", NULL);

    /*insert queue*/
    pipe->rtp_sound_queue = gst_element_factory_make("rtprtxqueue", NULL);

    GstCaps* cap_rtp_queue_source = gst_caps_new_simple
    ("application/x-rtp", NULL);


    /*
    # Set the queue max time to 16ms (16000000ns)
    # If the pipeline is behind by more than 1s, the packets
    # will be dropped.
    # This helps buffer out latency in the audio source.
    */
    g_object_set(pipe->rtp_sound_queue, "max-size-time", 16000000, NULL);

    /*
    Set the other queue sizes to 0 to make it only time-based.*/
    g_object_set(pipe->rtp_sound_queue, "max-size-packet", 0, NULL);

    /*capability for rtp sound payloader*/
    GstCaps* rtp_sound_pay_caps = gst_caps_new_simple(
        "applicaton/x-rtp",
        "media", "audio",
        "payload", 96,
        "clock-rate", 480000,
        "encoding-params", 2,
        "encoding-name", "OPUS", NULL
    );
    /*
    ________________________________________________________________________________________________________________________
    */


    /*link sound pipeline*/
    gst_bin_add_many(GST_BIN(pipe->pipeline), 
        pipe->screencapture,
        pipe->videoupload,
        pipe->videoconvert,
        pipe->gpu_encode,
        pipe->rtp_packetize,
        pipe->webrtcbin,
        pipe->soundcapture,
        pipe->soundencode,
        pipe->rtp_sound,
        pipe->rtp_sound_queue,
        pipe->rtp_sound_cap);
    gst_element_sync_state_with_parent(pipe->screencapture);
    gst_element_sync_state_with_parent(pipe->videoupload);
    gst_element_sync_state_with_parent(pipe->videoconvert);
    gst_element_sync_state_with_parent(pipe->gpu_encode);
    gst_element_sync_state_with_parent(pipe->rtp_packetize);
    gst_element_sync_state_with_parent(pipe->webrtcbin);
    gst_element_sync_state_with_parent(pipe->soundcapture);
    gst_element_sync_state_with_parent(pipe->soundencode);
    gst_element_sync_state_with_parent(pipe->rtp_sound);
    gst_element_sync_state_with_parent(pipe->rtp_sound_queue);
    gst_element_sync_state_with_parent(pipe->rtp_sound_cap);

    /*link sound pipeline to webrtcbin*/

    if (!gst_element_link_filtered(pipe->screencapture, pipe->videoupload, cap_screen_capture_src)) {
        g_printerr("cannot link ??and??");
        return;
    }
    if (!gst_element_link_filtered(pipe->videoupload, pipe->videoconvert, cap_video_upload_src)) {
        g_printerr("cannot link ??and??");
        return;
    }
    if (!gst_element_link_filtered(pipe->videoconvert, pipe->gpu_encode, cap_gpu_encode_sink)) {
        g_printerr("cannot link ??and??");
        return;
    }
    if (!gst_element_link_filtered(pipe->gpu_encode, pipe->rtp_packetize, cap_gpu_encode_src)) {
        g_printerr("cannot link ??and??");
        return;
    }
    if (!gst_element_link_filtered(pipe->rtp_packetize, pipe->webrtcbin, cap_rtp_packetize_src)) {
        g_printerr("cannot link ??and??");
        return;
    }
    if (!gst_element_link_filtered(pipe->soundcapture, pipe->soundencode, cap_sound_source)) {
        g_printerr("cannot link ??and??");
        return;
    }
    if (!gst_element_link_filtered(pipe->soundencode, pipe->rtp_sound, cap_sound_encoder_source)) {
        g_printerr("cannot link ??and??");
        return;
    }
    if (!gst_element_link_filtered(pipe->rtp_sound, pipe->rtp_sound_queue, cap_rtp_opuspay_source)) {
        g_printerr("cannot link ??and??");
        return;
    }
    if (!gst_element_link_filtered(pipe->rtp_sound_queue, pipe->webrtcbin, cap_rtp_queue_source)) {
        g_printerr("cannot link ??and??");
        return;
    }

    g_object_set_property(core, "core-state", PIPELINE_SETUP_DONE);
    
    g_signal_emit_by_name(core, "pipeline-ready", NULL);
}

gboolean
connect_WebRTCHub_handler(SessionCore* core)

{
    Pipeline* pipe = session_core_get_pipeline(core);
    CoreState state;

    g_object_get_property(core, "core-state", &state);

    if (state != PIPELINE_SETUP_DONE)
    {
        g_print("waiting for pipeline to setup");
        sleep(1);
    }

    /* This is the gstwebrtc entry point where we create the offer and so on. It
     * will be called when the pipeline goes to PLAYING. */
    g_signal_connect(pipe->webrtcbin, "on-negotiation-needed",
        G_CALLBACK(on_negotiation_needed), core);
    g_signal_connect(pipe->webrtcbin, "on-ice-candidate",
        G_CALLBACK(send_ice_candidate_message), core);
    g_signal_connect(pipe->webrtcbin, "notify::ice-gathering-state",
        G_CALLBACK(on_ice_gathering_state_notify), core);

    g_object_set_property(core, "core-state", HANDSHAKE_SIGNAL_CONNECTED);
    g_signal_emit_by_name(core, "handshake-signal-connected", NULL);
}


gboolean
start_pipeline(SessionCore* core)
{
    GstStateChangeReturn ret;
    Pipeline* pipe = session_core_get_pipeline(core);

    gst_element_set_state(pipe->pipeline, GST_STATE_READY);

    g_print("Starting pipeline\n");
    ret = gst_element_set_state(GST_ELEMENT(pipe->pipeline), GST_STATE_PLAYING);

    if (ret == GST_STATE_CHANGE_FAILURE)
    {
        return FALSE;
    }
    return TRUE;
}


void
stop_pipeline(SessionCore* core)
{

}