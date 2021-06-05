#include "Framework.h"
#include "Variable.h"
#include "Handle data channel.h"
#include "Handle pipeline.h"
#include "RC config.h"
#include "CorePipeSink.h"
#include "Signalling handling.h"

gboolean
check_plugins()
{
    int i;
    gboolean ret;
    GstPlugin* plugin;
    GstRegistry* registry;
    const gchar* needed[] = { "dx9screencapsrc",
    "nvh264enc","rtph264pay","pulsesrc",
    "opusencode","rtpopuspay","rtprtxqueue",
    "webrtcbin", NULL };

    registry = gst_registry_get();
    ret = TRUE;


    for (i = 0; i < g_strv_length((gchar**)needed); i++) {
        plugin = gst_registry_find_plugin(registry, needed[i]);

        if (!plugin) {
            g_print("Required gstreamer plugin '%s' not found\n", needed[i]);
            ret = FALSE;
            continue;
        }
        gst_object_unref(plugin);
    }
    return ret;
}

gboolean
cleanup_and_quit_loop(const gchar* msg, enum AppState state)
{
    if (msg)
        g_printerr("%s\n", msg);
    if (state > 0)
        app_state = state;

    if (ws_conn) {
        if (soup_websocket_connection_get_state(ws_conn) ==
            SOUP_WEBSOCKET_STATE_OPEN)
            /* This will call us again */
            soup_websocket_connection_close(ws_conn, 1000, "");
        else
            g_object_unref(ws_conn);
    }

    if (loop)
    {
        g_main_loop_quit(loop);
        loop = NULL;
    }

    /* To allow usage as a GSourceFunc */
    return FALSE;
}


/*handle all media stream include both audio and video:
add both audio and video pipe into webrtcbin, all webrtc bin signal handle will be connected at start_pipeline()
*/
void
handle_media_stream(gint screen_width,
    gint screen_height,
    gint framerate,
    const gchar* encoder = "nvh264enc")
{
    if (!check_plugins())
    {
        cleanup_and_quit_loop("plugin not found", APP_STATE_ERROR);
    }

    /*
    Adds the webrtcbin elments to the pipeline.
    The video and audio pipelines are linked to
    this in the build_video_pipline() and build_audio_pipeline() methods.
    */
    static GstElement
        * soundcapture, * soundencode, * rtp_sound, * rtp_sound_queue,
        * rtp_sound_cap, * screencapture, * videoupload,
        * videoconvert, * gpu_encode, * rtp_packetize;

    webrtcbin = gst_element_factory_make("webrtcbin", NULL);

    /*set policy-bundle*/
    g_object_set(webrtcbin, "bundle-policy", GST_WEBRTC_BUNDLE_POLICY_MAX_COMPAT);
    /* Add stun server */
    g_object_set(webrtcbin, "stun-server", stun_server, NULL);

    /*Generate widnow screen capture element based */
    screencapture = gst_element_factory_make("dx9screencapsrc", NULL);

    /*create cappabilities for screen capture*/
    GstCaps* cap_screen_capture_src = gst_caps_new_simple
    ("video/x-raw",
        "format", "BGR",
        "width", screen_width,
        "height", screen_height,
        "framerate", framerate, 1, NULL);

    /*turn off screeen cursor*/
    g_object_set(screencapture, "cursor", FALSE, NULL);

    /*monitor to display*/
    g_object_set(screencapture, "monitor", 0, NULL);

    /*cuda upload responsible for upload memory from main memory to gpu memory*/
    videoupload = gst_element_factory_make("cudaupload", NULL);


    GstCaps* cap_video_upload_src = gst_caps_new_simple
    ("video/x-raw", NULL);

    /*convert BGR color space to I420 color space*/
    videoconvert = gst_element_factory_make("cuda-convert", NULL);

    GstCaps* cap_gpu_encode_sink{};
    GstCaps* cap_gpu_encode_src{};
    GstCaps* cap_rtp_packetize_src{};



    if (g_strcmp0(encoder, "nvh264enc"))
    {
        /*
         ___________________________________________________________________
        gpu side
        */
        gpu_encode = gst_element_factory_make("nvh264enc", NULL);

        /*variable bitrate mode*/
        g_object_set(gpu_encode, "rc-mode", "vbr", NULL);

        /*low latency preset*/
        g_object_set(gpu_encode, "preset", "low-latency", NULL);

        /*set b-frame numbers property*/
        g_object_set(gpu_encode, "bframes", 0, NULL);

        /**/
        g_object_set(gpu_encode, "zerolatency", TRUE, NULL);

        /**/
        g_object_set(gpu_encode, "const_quality", vbitrate, NULL);

        /*create capability for encoder sink pad*/
        cap_gpu_encode_sink = gst_caps_new_simple
        ("video/x-raw",
            "format", "I420",
            "width", screen_width,
            "height", screen_height,
            "framerate", framerate, 1, "progressive", NULL);

        /*create capability for encoder source pad*/
        cap_gpu_encode_src = gst_caps_new_simple
        ("video/x-h264",
            "width", screen_width,
            "height", screen_height,
            "framerate", framerate,
            "stream-format", "byte-stream",
            "alignment", "au",
            "profile", "high", NULL);

        ///gst_video_encoder_get_latency() future realease 

        /*packetize video encoded byte-stream*/
        rtp_packetize = gst_element_factory_make("rtph264pay", NULL);

        cap_rtp_packetize_src = gst_caps_new_simple
        ("application/x-rtp",
            "media", "video",
            "payload", 123,
            "clock-rate", 90000,
            "encoding-name", "H264", NULL);


        /*set zero latency aggregate mode*/
        g_object_set(rtp_packetize, "aggregate-mote", "zero-latency", NULL);
    }
    if (g_strcmp0(encoder, "nvh265enc"))
    {
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
    soundcapture = gst_element_factory_make("pulsesrc", NULL);

    GstCaps* cap_sound_source = gst_caps_new_simple
    ("audio/x-raw",
        "format", "S16LE",
        "layout", "interleaved",
        "rate", abitrate,
        "channel", "2", NULL);


    /*create sound encoder*/
    soundencode = gst_element_factory_make("opusenc", NULL);

    /**/
    GstCaps* cap_sound_encoder_source = gst_caps_new_simple
    ("audio/x-raw"
        "format", "S16LE"
        "layout", "interleaved"
        "rate", abitrate,
        "channel", "2", NULL);

    /*create rtp packetizer forr audio stream*/
    rtp_sound = gst_element_factory_make("rtpopuspay", NULL);

    GstCaps* cap_rtp_opuspay_source = gst_caps_new_simple
    ("application/x-rtp",
        "media", "audio",
        "encoding-params", 2,
        "encoding-name", "OPUS", NULL);

    /*insert queue*/
    rtp_sound_queue = gst_element_factory_make("rtprtxqueue", NULL);

    GstCaps* cap_rtp_queue_source = gst_caps_new_simple
    ("application/x-rtp", NULL);


    /*
    # Set the queue max time to 16ms (16000000ns)
    # If the pipeline is behind by more than 1s, the packets
    # will be dropped.
    # This helps buffer out latency in the audio source.
    */
    g_object_set(rtp_sound_queue, "max-size-time", 16000000, NULL);

    /*
    Set the other queue sizes to 0 to make it only time-based.*/
    g_object_set(rtp_sound_queue, "max-size-packet", 0, NULL);

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
    gst_bin_add_many(GST_BIN(pipeline), screencapture, videoupload, videoconvert, gpu_encode, rtp_packetize, webrtcbin,
        soundcapture, soundencode, rtp_sound, rtp_sound_queue, rtp_sound_cap);
    gst_element_sync_state_with_parent(screencapture);
    gst_element_sync_state_with_parent(videoupload);
    gst_element_sync_state_with_parent(videoconvert);
    gst_element_sync_state_with_parent(gpu_encode);
    gst_element_sync_state_with_parent(rtp_packetize);
    gst_element_sync_state_with_parent(webrtcbin);
    gst_element_sync_state_with_parent(soundcapture);
    gst_element_sync_state_with_parent(soundencode);
    gst_element_sync_state_with_parent(rtp_sound);
    gst_element_sync_state_with_parent(rtp_sound_queue);
    gst_element_sync_state_with_parent(rtp_sound_cap);

    /*link sound pipeline to webrtcbin*/

    if (!gst_element_link_filtered(screencapture, videoupload, cap_screen_capture_src)) {
        g_printerr("cannot link ??and??");
        return;
    }
    if (!gst_element_link_filtered(videoupload, videoconvert, cap_video_upload_src)) {
        g_printerr("cannot link ??and??");
        return;
    }
    if (!gst_element_link_filtered(videoconvert, gpu_encode, cap_gpu_encode_sink)) {
        g_printerr("cannot link ??and??");
        return;
    }
    if (!gst_element_link_filtered(gpu_encode, rtp_packetize, cap_gpu_encode_src)) {
        g_printerr("cannot link ??and??");
        return;
    }
    if (!gst_element_link_filtered(rtp_packetize, webrtcbin, cap_rtp_packetize_src)) {
        g_printerr("cannot link ??and??");
        return;
    }
    if (!gst_element_link_filtered(soundcapture, soundencode, cap_sound_source)) {
        g_printerr("cannot link ??and??");
        return;
    }
    if (!gst_element_link_filtered(soundencode, rtp_sound, cap_sound_encoder_source)) {
        g_printerr("cannot link ??and??");
        return;
    }
    if (!gst_element_link_filtered(rtp_sound, rtp_sound_queue, cap_rtp_opuspay_source)) {
        g_printerr("cannot link ??and??");
        return;
    }
    if (!gst_element_link_filtered(rtp_sound_queue, webrtcbin, cap_rtp_queue_source)) {
        g_printerr("cannot link ??and??");
        return;
    }
}

gboolean
start_pipeline()
{
    GstStateChangeReturn ret;

    /*initialize global pipeline with paticular resolution and framerate and encoder*/
    handle_media_stream(screen_width, screen_height, framerate, "nvh264enc");

    /* This is the gstwebrtc entry point where we create the offer and so on. It
     * will be called when the pipeline goes to PLAYING. */
    g_signal_connect(webrtcbin, "on-negotiation-needed",
        G_CALLBACK(on_negotiation_needed), NULL);

    /* We need to transmit this ICE candidate to the browser via the websockets
     * signalling server. Incoming ice candidates from the browser need to be
     * added by us too, see on_server_message() */
    g_signal_connect(webrtcbin, "on-ice-candidate",
        G_CALLBACK(send_ice_candidate_message), NULL);
    g_signal_connect(webrtcbin, "notify::ice-gathering-state",
        G_CALLBACK(on_ice_gathering_state_notify), NULL);

    gst_element_set_state(pipeline, GST_STATE_READY);

    g_signal_emit_by_name(webrtcbin, "create-data-channel", "SessionCore", NULL,
        &SessionCore);
    g_signal_emit_by_name(webrtcbin, "create-data-channel", "SessionLoader", NULL,
        &SessionLoader);

    if (SessionCore && SessionLoader)
    {
        g_print("Created two data channels\n");
        connect_data_channel_signals(SessionCore,  "SessionCore");
        connect_data_channel_signals(SessionLoader, "SessionLoader");
    }
    else {
        g_print("Could not create  data channel!\n");
        goto err;
    }

    g_signal_connect(webrtcbin, "on-data-channel", G_CALLBACK(on_data_channel), NULL);

    /* Lifetime is the same as the pipeline itself */
    gst_object_unref(webrtcbin);

    g_print("Starting pipeline\n");
    ret = gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_PLAYING);
    if (ret == GST_STATE_CHANGE_FAILURE)
    {
        goto err;
        g_printerr("pipeline cannot failure state");
    }
    return TRUE;

err:
    if (pipeline)
        g_clear_object(&pipeline);
    if (webrtcbin)
    {
        webrtcbin = NULL;
    }
    cleanup_and_quit_loop("ERROR: failed to start pipeline", PEER_CALL_ERROR);
    return FALSE;
}

