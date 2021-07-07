#include <session-core-pipeline.h>
#include <session-core-data-channel.h>
#include <session-core-signalling.h>
#include <session-core-remote-config.h>

#include <gst\gst.h>
#include <glib-2.0\glib.h>

#include <gst\webrtc\webrtc.h>
#include <gst\rtp\gstrtppayloads.h>

#include <session-core-type.h>


/// <summary>
/// gstreamer video element enumaration,
/// the order of element in enum must follow the 
/// </summary>
enum
{
    /*screen capture source*/
    DX9_SCREEN_CAPTURE_SOURCE,

    /*preprocess before encoding*/
    CUDA_UPLOAD,
    CUDA_CONVERT,

    /*video encoder*/
    NVIDIA_H264_ENCODER,
    NVIDIA_H265_ENCODER,

    /*payload packetize*/
    RTP_H264_PAYLOAD,

    VIDEO_ELEMENT_LAST
};

/// <summary>
/// gstreamer audio element enumaration,
/// the order of element in enum must follow the 
/// </summary>
enum
{
    /*audio capture source*/
    PULSE_SOURCE_SOUND,

    /*audio encoder*/
    OPUS_ENCODER,

    /*rtp packetize and queue*/
    RTP_OPUS_PAYLOAD,
    RTP_RTX_QUEUE,

    AUDIO_ELEMENT_LAST
};


struct _Pipeline
{
    PipelineState state;

	GstElement* pipeline;
	GstElement* webrtcbin;

    GstElement* video_element[VIDEO_ELEMENT_LAST];
    GstElement* audio_element[AUDIO_ELEMENT_LAST];

    GstCaps* video_caps[VIDEO_ELEMENT_LAST];
    GstCaps* audio_caps[AUDIO_ELEMENT_LAST];
};



Pipeline*
pipeline_initialize()
{
    Pipeline* pipeline = malloc(sizeof(Pipeline));
	memset(pipeline,0,sizeof(Pipeline));

    pipeline->webrtcbin = gst_element_factory_make("webrtcbin", NULL);
    pipeline->pipeline = gst_pipeline_new("video_capture_pipeline");
    pipeline->state = PIPELINE_INITIALIZED;

    return pipeline;
}



gpointer
setup_pipeline(SessionCore* core)
{
    Pipeline* pipe = session_core_get_pipeline(core);
    QoE* qoe= session_core_get_qoe(core);

    pipe->state = PIPELINE_SETTING_UP;

    pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE] =    gst_element_factory_make("dx9screencapsrc", NULL);
    pipe->video_element[CUDA_UPLOAD] =                  gst_element_factory_make("cudaupload", NULL);                      /*cuda upload responsible for upload memory from main memory to gpu memory*/
    pipe->video_element[CUDA_CONVERT] =                 gst_element_factory_make("cuda-convert", NULL);                   /*convert BGR color space to I420 color space*/
    pipe->video_element[NVIDIA_H264_ENCODER] =          gst_element_factory_make("nvh264enc", NULL);
    pipe->video_element[RTP_H264_PAYLOAD] =             gst_element_factory_make("rtph264pay", NULL);

    pipe->audio_element[PULSE_SOURCE_SOUND] =           gst_element_factory_make("pulsesrc", NULL);
    pipe->audio_element[OPUS_ENCODER] =                 gst_element_factory_make("opusenc", NULL);
    pipe->audio_element[RTP_OPUS_PAYLOAD] =             gst_element_factory_make("rtpopuspay", NULL);
    pipe->audio_element[RTP_RTX_QUEUE] =                gst_element_factory_make("rtprtxqueue", NULL);

    
    setup_element_cap( core);
    setup_element_property( core);

    for (gint i = 0; i < VIDEO_ELEMENT_LAST; i++)
    {
        if (pipe->video_element[i] != NULL)
        {
            gst_bin_add(GST_BIN(pipe->pipeline), pipe->video_element[i]);
            gst_element_sync_state_with_parent(pipe->video_element[i]);
        }
    }
    for (gint i = 0; i < AUDIO_ELEMENT_LAST; i++)
    {
        if (pipe->audio_element[i] != NULL)
        {
            gst_bin_add(GST_BIN(pipe->pipeline), pipe->audio_element[i]);
            gst_element_sync_state_with_parent(pipe->audio_element[i]);
        }
    }
    connect_element(core);
    pipe->state = PIPELINE_ELEMENT_LINKED;

    connect_signalling_handler(core);
    connect_data_channel_signals(core);

    pipe->state = PIPELINE_SETUP_DONE;

    if (!start_pipeline(core))
        session_core_finalize(core, CORE_STATE_ERROR);

    session_core_set_state(core, REMOTE_CONNECT_STARTED);
}

void
setup_element_property(SessionCore* core)
{
    Pipeline* pipe = session_core_get_pipeline(core);
    SignallingHub* hub = session_core_get_signalling_hub(core);

    /*set policy-bundle*/
    g_object_set(pipe->webrtcbin, "bundle-policy", GST_WEBRTC_BUNDLE_POLICY_MAX_COMPAT);
    /* Add stun server */
    g_object_set(pipe->webrtcbin, "stun-server",signalling_hub_get_stun_server(hub), NULL);

    /*turn off screeen cursor*/
    g_object_set(pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE], "cursor", FALSE, NULL);

    /*monitor to display*/
    g_object_set(pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE], "monitor", 0, NULL);

    /*variable bitrate mode*/
    g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "rc-mode", "vbr", NULL);

    /*low latency preset*/
    g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "preset", "low-latency", NULL);

    /*set b-frame numbers property*/
    g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "bframes", 0, NULL);

    /**/
    g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "zerolatency", TRUE, NULL);

    /*set zero latency aggregate mode*/
    g_object_set(pipe->video_element[RTP_H264_PAYLOAD], "aggregate-mote", "zero-latency", NULL);

    /*handle dynamic control bitrate*/
    attach_bitrate_control(
        GST_OBJECT(pipe->audio_element[OPUS_ENCODER]),
        GST_OBJECT(pipe->video_element[NVIDIA_H264_ENCODER]),
        core);

    /*
    * Set the queue max time to 16ms (16000000ns)
    * If the pipeline is behind by more than 1s, the packets
    * will be dropped.
    * This helps buffer out latency in the audio source.
    */
    g_object_set(pipe->audio_element[RTP_RTX_QUEUE], "max-size-time", 16000000, NULL);

    /*
    Set the other queue sizes to 0 to make it only time-based.*/
    g_object_set(pipe->audio_element[RTP_RTX_QUEUE], "max-size-packet", 0, NULL);


}

void
setup_element_cap(SessionCore* core)
{
    Pipeline* pipe = session_core_get_pipeline(core);
    QoE* qoe = session_core_get_qoe(core);



    /*create cappabilities for screen capture*/
    pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE] = gst_caps_new_simple
    ("video/x-raw",
        "format", "BGR",
        "width", qoe_get_screen_width(qoe),
        "height", qoe_get_screen_height(qoe),
        "framerate", qoe_get_framerate(qoe), 1, NULL);

    pipe->video_caps[CUDA_UPLOAD] = gst_caps_new_simple
    ("video/x-raw", NULL);



    pipe->audio_caps[PULSE_SOURCE_SOUND] = gst_caps_new_simple
    ("audio/x-raw",
        "format", "S16LE",
        "layout", "interleaved",
        ///"rate", abitrate,
        "channel", "2", NULL);

    /**/
    pipe->audio_caps[OPUS_ENCODER] = gst_caps_new_simple
    ("audio/x-raw"
        "format", "S16LE"
        "layout", "interleaved"
        ///"rate", abitrate,
        "channel", "2", NULL);

    /*create rtp packetizer forr audio stream*/


    pipe->audio_caps[RTP_OPUS_PAYLOAD] = gst_caps_new_simple
    ("application/x-rtp",
        "media", "audio",
        "encoding-params", 2,
        "encoding-name", "OPUS", NULL);


    pipe->audio_caps[RTP_RTX_QUEUE] = gst_caps_new_simple
    ("application/x-rtp", NULL);




    /*capability for rtp sound payloader*/
    pipe->audio_caps[RTP_OPUS_PAYLOAD] = gst_caps_new_simple(
        "applicaton/x-rtp",
        "media", "audio",
        "payload", 96,
        "clock-rate", 480000,
        "encoding-params", 2,
        "encoding-name", "OPUS", NULL
    );

    /*create capability for encoder sink pad*/
    pipe->video_caps[CUDA_CONVERT] = gst_caps_new_simple
    ("video/x-raw",
        "format", "I420",
        "width", qoe_get_screen_width(qoe),
        "height", qoe_get_screen_height(qoe),
        "framerate", qoe_get_framerate(qoe), 1,
        "progressive", NULL);

    /*create capability for encoder source pad*/
    pipe->video_caps[NVIDIA_H264_ENCODER] = gst_caps_new_simple
    ("video/x-h264",
        "width", qoe_get_screen_width(qoe),
        "height", qoe_get_screen_height(qoe),
        "framerate", qoe_get_framerate(qoe),
        "stream-format", "byte-stream",
        "alignment", "au",
        "profile", "high", NULL);

    /*packetize video encoded byte-stream*/
    pipe->audio_caps[RTP_H264_PAYLOAD] = gst_caps_new_simple
    ("application/x-rtp",
        "media", "video",
        "payload", 123,
        "clock-rate", 90000,
        "encoding-name", "H264", NULL);

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
connect_element(SessionCore* core)

{
    WebRTCHub* hub = session_core_get_rtc_hub(core);
    SignallingHub* signalling = session_core_get_signalling_hub(core);
    Pipeline* pipe = session_core_get_pipeline(core);

    gint i = 0;
    while(i<VIDEO_ELEMENT_LAST)
    {
        if (pipe->video_element[i] != NULL)
        {
            gint j = i + 1;
            while(j<VIDEO_ELEMENT_LAST)
            {
                if (pipe->video_element[j] != NULL)
                {
                    gint ret = gst_element_link_filtered(
                        pipe->video_element[i], pipe->video_element[j], pipe->video_caps[i]);
                    if (!ret)
                        return FALSE;
                    g_free(ret);
                    i = j;
                    goto end;
                }
                j++;
            }
            /*link element with webrtcbin if this is the last element*/
            gst_element_link_filtered
                (pipe->video_element[i], pipe->webrtcbin, pipe->video_caps[i]); 
        }
        i++;
    end:;
    }


    gint a = 0;
    while (a < AUDIO_ELEMENT_LAST)
    {
        if (pipe->video_element[a] != NULL)
        {
            gint b = a + 1;
            while (b < AUDIO_ELEMENT_LAST)
            {
                if (pipe->video_element[b] != NULL)
                {
                    gint ret = gst_element_link_filtered(
                        pipe->video_element[a], pipe->video_element[b], pipe->video_caps[a]);
                    if (!ret)
                        return FALSE;
                    g_free(ret);
                    a = b;
                    goto end_;
                }
                b++;
            }
            /*link element with webrtcbin if this is the last element*/
            gst_element_link_filtered
            (pipe->video_element[a], pipe->webrtcbin, pipe->video_caps[a]);
        }
        a++;
    end_:;
    }
    return TRUE;

} 

gboolean
connect_signalling_handler(SessionCore* core)
{
    Pipeline* pipe = session_core_get_pipeline(core);

    /* This is the gstwebrtc entry point where we create the offer and so on. It
     * will be called when the pipeline goes to PLAYING. */
    g_signal_connect(pipe->webrtcbin, "on-negotiation-needed",
        G_CALLBACK(on_negotiation_needed), core);
    g_signal_connect(pipe->webrtcbin, "on-ice-candidate",
        G_CALLBACK(send_ice_candidate_message), core);
    g_signal_connect(pipe->webrtcbin, "notify::ice-gathering-state",
        G_CALLBACK(on_ice_gathering_state_notify), core);

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











GstElement*
pipeline_get_webrtc_bin(Pipeline* pipe)
{
    return pipe->webrtcbin;
}

GstElement*
pipeline_get_pipline(Pipeline* pipe)
{
    return pipe->pipeline;
}