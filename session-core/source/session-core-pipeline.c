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
    VIDEO_CONVERT,
    /*video encoder*/

    NVIDIA_H264_ENCODER,
    NVIDIA_H265_ENCODER,
    NVIDIA_H264_DIRECTX,
    NVIDIA_H265_DIRECTX,
    NVIDIA_H264_MEDIA_FOUNDATION,
    NVIDIA_H265_MEDIA_FOUNDATION,


    /*payload packetize*/
    RTP_H264_PAYLOAD,
    RTP_H265_PAYLOAD,

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
    WASAPI_SOURCE_SOUND,

    /*audio encoder*/
    OPUS_ENCODER,
    MP3_ENCODER,
    AAC_ENCODER,

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

    GstElement* video_element_filtered[VIDEO_ELEMENT_LAST];
    GstElement* audio_element_filtered[AUDIO_ELEMENT_LAST];

    GstCaps* video_caps[VIDEO_ELEMENT_LAST];
    GstCaps* audio_caps[AUDIO_ELEMENT_LAST];
};


#define STUN " stun-server=stun://stun.l.google.com:19302 "
#define RTP_CAPS_OPUS "application/x-rtp,media=audio,encoding-name=OPUS,payload="
#define RTP_CAPS_VP8 "application/x-rtp,media=video,encoding-name=H264,payload="

Pipeline*
pipeline_initialize(SessionCore* core)
{
    SignallingHub* hub = session_core_get_signalling_hub(core);

    static Pipeline pipeline;
    GError* error = NULL;
    gchar* stun =          signalling_hub_get_stun_server(hub);

    pipeline.pipeline =
        gst_parse_launch("webrtcbin bundle-policy=max-bundle name=sendrecv " 
            STUN
            "dx9screencapsrc ! videoconvert ! queue ! nvh264enc ! rtph264pay ! "
            "queue ! " RTP_CAPS_VP8 "96 ! sendrecv. "
            "audiotestsrc is-live=true wave=red-noise ! audioconvert ! audioresample ! queue ! opusenc ! rtpopuspay ! "
            "queue ! " RTP_CAPS_OPUS "97 ! sendrecv. ", &error);

    if (error != NULL) {
        g_printerr("Failed to parse launch: %s\n", error->message);
        g_error_free(error);
        return NULL;
    }

    pipeline.webrtcbin = gst_bin_get_by_name(GST_BIN(pipeline.pipeline), "sendrecv");
    g_assert_nonnull(pipeline.webrtcbin);
    pipeline.state = PIPELINE_NOT_READY;

    return &pipeline;
}

gboolean
start_pipeline(SessionCore* core)
{
    GstStateChangeReturn ret;
    Pipeline* pipe = session_core_get_pipeline(core);

    ret = GST_IS_ELEMENT(pipe->pipeline);

    

    ret = gst_element_set_state(pipe->pipeline, GST_STATE_PLAYING);

    if (ret == GST_STATE_CHANGE_FAILURE)
    {
        return FALSE;
    }
    g_print("Starting pipeline\n");
    return TRUE;
}



void
setup_element_factory_h264_media_foundation(SessionCore* core)
{
    Pipeline* pipe = session_core_get_pipeline(core);

    pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE] =    gst_element_factory_make("dx9screencapsrc", NULL);                 /*cuda upload responsible for upload memory from main memory to gpu memory*/
    pipe->video_element[VIDEO_CONVERT] =                gst_element_factory_make("videoconvert", NULL);                   /*convert BGR color space to I420 color space*/
    pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION] = gst_element_factory_make("mfh264enc", NULL);
    pipe->video_element[RTP_H264_PAYLOAD] =             gst_element_factory_make("rtph264pay", NULL);

    pipe->audio_element[WASAPI_SOURCE_SOUND] =          gst_element_factory_make("wasapisrc", NULL);
    pipe->audio_element[OPUS_ENCODER] =                 gst_element_factory_make("opusenc", NULL);
    pipe->audio_element[RTP_OPUS_PAYLOAD] =             gst_element_factory_make("rtpopuspay", NULL);
}

gpointer
setup_pipeline(SessionCore* core)
{
    SignallingHub* signalling = session_core_get_signalling_hub(core);
    Pipeline* pipe = session_core_get_pipeline(core);
    QoE* qoe= session_core_get_qoe(core);


    pipe->state = PIPELINE_CREATING_ELEMENT;

    /*
    setup_element_factory_h264_media_foundation(core);
    pipe->state = PIPELINE_SETTING_UP_ELEMENT;
    setup_element_cap( core);
    setup_element_property( core);





    pipe->state = PIPELINE_CONNECT_ELEMENT_SIGNAL;

    if (!connect_element(core))
    {
        session_core_finalize(core, PIPELINE_ERROR);
    }*/




    connect_signalling_handler(core);

    gst_element_change_state(pipe->pipeline, GST_STATE_READY);

    if (!connect_data_channel_signals(core)) 
    {
        session_core_finalize(core, DATA_CHANNEL_ERROR);
    }

    pipe->state = PIPELINE_LINKING_ELEMENT;
    pipe->state = PIPELINE_SETUP_DONE;

    if (!start_pipeline(core))
    {
        session_core_finalize(core, PIPELINE_ERROR);
    }
    session_core_set_state(core, REMOTE_CONNECT_STARTED);
    signalling_hub_set_peer_call_state(signalling, PEER_CALL_DONE);
}





void
setup_element_property(SessionCore* core)
{
    Pipeline* pipe = session_core_get_pipeline(core);
    SignallingHub* hub = session_core_get_signalling_hub(core);
    QoE* qoe = session_core_get_qoe(core);

    /*set policy-bundle*/
    if (pipe->webrtcbin) { g_object_set(pipe->webrtcbin, "bundle-policy", GST_WEBRTC_BUNDLE_POLICY_MAX_COMPAT, NULL); }
    /* Add stun server */
    if (pipe->webrtcbin) { g_object_set(pipe->webrtcbin, "stun-server", signalling_hub_get_stun_server(hub), NULL); }

    /*turn off screeen cursor*/
    if (pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE]) { g_object_set(pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE], "cursor", FALSE, NULL); }

    /*monitor to display*/
    if (pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE]) { g_object_set(pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE], "monitor", 0, NULL);}

    /*variable bitrate mode*/
    if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "rc-mode", "cbr", NULL);}

    if (pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION]) { g_object_set(pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION], "rc-mode", 0, NULL);}

    if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "bitrate", 20000, NULL);}

    if (pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION]) { g_object_set(pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION], "bitrate", 20000, NULL);}

    if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "qos", TRUE, NULL);}

    if (pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION]) { g_object_set(pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION], "qos", TRUE, NULL);}

    /*low latency preset*/
    if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "preset", "low-latency", NULL);}

    if (pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION]) { g_object_set(pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION], "low-latency", TRUE, NULL);}

    if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "bitrate", qoe_get_video_bitrate(qoe), NULL);}

    if (pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION]) { g_object_set(pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION], "bitrate", qoe_get_video_bitrate(qoe), NULL);}


    /*set b-frame numbers property*/
    if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "bframes", 0, NULL);}

    /**/
    if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "zerolatency", TRUE, NULL);}

    /*set zero latency aggregate mode*/
    if (pipe->video_element[RTP_H264_PAYLOAD]) { g_object_set(pipe->video_element[RTP_H264_PAYLOAD], "aggregate-mode", 1, NULL);}

    /*handle dynamic control bitrate*/
    //if (!pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION])
    //{
    //   attach_bitrate_control(
    //        GST_OBJECT(pipe->audio_element[OPUS_ENCODER]),
    //        GST_OBJECT(pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION]),
    //        core);
    //}

    if (pipe->audio_element[WASAPI_SOURCE_SOUND]) { g_object_set(pipe->audio_element[WASAPI_SOURCE_SOUND], "low-latency", TRUE, NULL);}

//    if (pipe->audio_element[OPUS_ENCODER]) { g_object_set(pipe->audio_element[OPUS_ENCODER], "bitrate", qoe_get_audio_bitrate(qoe), NULL);}

    /*
    * Set the queue max time to 16ms (16000000ns)
    * If the pipeline is behind by more than 1s, the packets
    * will be dropped.
    * This helps buffer out latency in the audio source.
    */
    if (pipe->audio_element[RTP_RTX_QUEUE]) { g_object_set(pipe->audio_element[RTP_RTX_QUEUE], "max-size-time", 16000000, NULL);}

    /*
    Set the other queue sizes to 0 to make it only time-based.*/
    if (pipe->audio_element[RTP_RTX_QUEUE]) { g_object_set(pipe->audio_element[RTP_RTX_QUEUE], "max-size-packet", 0, NULL);}


}

void
setup_element_cap(SessionCore* core)
{
    Pipeline* pipe = session_core_get_pipeline(core);
    QoE* qoe = session_core_get_qoe(core);



    /*create cappabilities for screen capture*/
    pipe->video_caps[DX9_SCREEN_CAPTURE_SOURCE] = gst_caps_new_simple
    ("video/x-raw",
        "format", G_TYPE_STRING,"BGR",
        "width",G_TYPE_INT ,qoe_get_screen_width(qoe),
        "height", G_TYPE_INT,qoe_get_screen_height(qoe),
        "framerate",GST_TYPE_FRACTION ,qoe_get_framerate(qoe), 1, NULL);

    pipe->video_caps[VIDEO_CONVERT] = gst_caps_new_simple
    ("video/x-raw",
        "format", G_TYPE_STRING, "NV12",
        "width", G_TYPE_INT, qoe_get_screen_width(qoe),
        "height" ,G_TYPE_INT, qoe_get_screen_height(qoe),NULL );

    pipe->video_caps[CUDA_UPLOAD] = gst_caps_new_simple
    ("video/x-raw", NULL);

    /*create capability for encoder source pad*/
    pipe->video_caps[NVIDIA_H264_ENCODER] = gst_caps_new_simple
    ("video/x-h264",
        "width", G_TYPE_INT, qoe_get_screen_width(qoe),
        "height", G_TYPE_INT, qoe_get_screen_height(qoe),
        "framerate", G_TYPE_INT, qoe_get_framerate(qoe),
        "stream-format", G_TYPE_STRING, "byte-stream",
        "alignment", G_TYPE_STRING, "au",
        "profile", G_TYPE_STRING, "high", NULL);

    /*create capability for encoder source pad*/
    pipe->video_caps[NVIDIA_H264_MEDIA_FOUNDATION] = gst_caps_new_simple
    ("video/x-h264",
        "width", G_TYPE_INT, qoe_get_screen_width(qoe),
        "height", G_TYPE_INT, qoe_get_screen_height(qoe),
        "framerate", G_TYPE_INT, qoe_get_framerate(qoe),
        "profile", G_TYPE_STRING, "high",
        "stream-format", G_TYPE_STRING, "byte-stream",
        "alignment", G_TYPE_STRING, "au", NULL);


    pipe->audio_caps[PULSE_SOURCE_SOUND] = gst_caps_new_simple
    ("audio/x-raw",
        "format", G_TYPE_STRING,"S16LE",
        "layout", G_TYPE_STRING, "interleaved",
        "rate", G_TYPE_INT, qoe_get_audio_bitrate(qoe),
        "channels", G_TYPE_INT, 2, NULL);

    pipe->audio_caps[WASAPI_SOURCE_SOUND] = gst_caps_new_simple
    ("audio/x-raw",
        "format", G_TYPE_STRING, "S16LE",
        "layout", G_TYPE_STRING, "interleaved",
        "rate", G_TYPE_INT, qoe_get_audio_bitrate(qoe),
        "channels", G_TYPE_INT, 2, NULL);

    /**/
    pipe->audio_caps[OPUS_ENCODER] = gst_caps_new_simple
    ("audio/x-raw",
        "format", G_TYPE_STRING, "S16LE",
        "layout", G_TYPE_STRING, "interleaved",
        //"rate", G_TYPE_INT, qoe_get_audio_bitrate(qoe),
        "channels", G_TYPE_INT, 2, NULL);

    /*create rtp packetizer forr audio stream*/    
    pipe->audio_caps[RTP_OPUS_PAYLOAD] = gst_caps_new_simple(
        "applicaton/x-rtp",
        "media", G_TYPE_STRING, "audio",
        "payload", G_TYPE_INT, 96,
        "encoding-params", G_TYPE_INT, 2,
        "encoding-name", G_TYPE_STRING, "OPUS", NULL);

    pipe->audio_caps[RTP_RTX_QUEUE] = gst_caps_new_simple
    ("application/x-rtp", NULL);




    /*create capability for encoder sink pad*/
    pipe->video_caps[CUDA_CONVERT] = gst_caps_new_simple
    ("video/x-raw",
        "format", G_TYPE_STRING, "I420",
        "width", G_TYPE_INT, qoe_get_screen_width(qoe),
        "height", G_TYPE_INT, qoe_get_screen_height(qoe),
        "framerate", GST_TYPE_FRACTION, qoe_get_framerate(qoe), 1,
        /*"progressive",*/ NULL);



    /*packetize video encoded byte-stream*/
    pipe->video_caps[RTP_H264_PAYLOAD] = gst_caps_new_simple
    ("application/x-rtp",
        "media", G_TYPE_STRING, "video",
        "payload", G_TYPE_INT,123,
        "clock-rate", G_TYPE_INT, 90000,
        "encoding-name", G_TYPE_STRING, "H264", NULL);


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


    for (gint x = 0; x < VIDEO_ELEMENT_LAST; x++)
    {


        if (pipe->video_element[x] != NULL )
        {
            // g_object_set_property((GObject*)pipe->video_element_filtered[x], "caps", pipe->video_caps[x]);
            //pipe->video_element_filtered[x] = gst_element_factory_make("capsfilter", "filter");
            //gst_element_sync_state_with_parent(pipe->video_element_filtered[x]);
            //gst_bin_add(GST_BIN(pipe->pipeline), pipe->video_element_filtered[x]);
            gst_bin_add(GST_BIN(pipe->pipeline), pipe->video_element[x]);
            //gst_element_sync_state_with_parent(pipe->video_element[x]);

        }
    }
    for (gint x = 0; x < AUDIO_ELEMENT_LAST; x++)
    {

        if (pipe->audio_element[x] != NULL )
        { 
            //((GObject*)pipe->audio_element_filtered[x], "caps", pipe->audio_caps[x]);
            //pipe->audio_element_filtered[x] = gst_element_factory_make("capsfilter", "filter");
            //gst_element_sync_state_with_parent(pipe->audio_element_filtered[x]);
            //gst_bin_add(GST_BIN(pipe->pipeline), pipe->audio_element_filtered[x]);
            gst_bin_add(GST_BIN(pipe->pipeline), pipe->audio_element[x]);
            //gst_element_sync_state_with_parent(pipe->audio_element[x]);
        }
    }

    gint i = 0;
    for (gint m = 0; m < AUDIO_ELEMENT_LAST; m++)
    {
        if (pipe->audio_element[m] != NULL)
        {
            i = m;
            break;
        }
    }
    while(i<VIDEO_ELEMENT_LAST)
    {
        if (pipe->video_element[i] != NULL)
        {
            gint j = i + 1;
            while(j<VIDEO_ELEMENT_LAST)
            {
                if (pipe->video_element[j] != NULL)
                {
                    if (!gst_element_link(pipe->video_element[i], pipe->video_element[j]))// ||
                         //gst_element_link(pipe->video_element[i], pipe->video_element_filtered[j]))
                        return FALSE;
                    i = j;
                    goto end;
                }
                j++;
            }
            /*link element with webrtcbin if this is the last element*/
            if (!gst_element_link(pipe->video_element[i], pipe->webrtcbin))
                return FALSE;
        }
        i++;
    end:;
    }


    gint a = 0;
    for (gint l = 0; l < AUDIO_ELEMENT_LAST; l++)
    {
        if (pipe->audio_element[l] != NULL)
        {
            a = l;
            break;
        }
    }
    while (a < AUDIO_ELEMENT_LAST)
    {
        if (pipe->audio_element[a] != NULL)
        {
            gint b = a + 1;
            while (b < AUDIO_ELEMENT_LAST)
            {
                if (pipe->audio_element[b] != NULL)
                {
                    if (!gst_element_link(pipe->audio_element[a], pipe->audio_element[b]))// ||
                         //gst_element_link(pipe->audio_element[a], pipe->audio_element_filtered[b]))
                        return FALSE;
                    a = b;
                    goto end_;
                }
                b++;
            }
            /*link element with webrtcbin if this is the last element*/
            if (!gst_element_link(pipe->audio_element[a], pipe->webrtcbin))
                return FALSE;
        }
        a++;
    end_:;
    }
    return TRUE;

} 

void
connect_signalling_handler(SessionCore* core)
{
    Pipeline* pipe = session_core_get_pipeline(core);


    g_main_context_push_thread_default(sessioin_core_get_main_context(core));
    /* This is the gstwebrtc entry point where we create the offer and so on. It
     * will be called when the pipeline goes to PLAYING. */
    g_signal_connect(pipe->webrtcbin, "on-negotiation-needed",
        G_CALLBACK(on_negotiation_needed), core);
    g_signal_connect(pipe->webrtcbin, "on-ice-candidate",
        G_CALLBACK(send_ice_candidate_message), core);
    g_signal_connect(pipe->webrtcbin, "notify::ice-gathering-state",
        G_CALLBACK(on_ice_gathering_state_notify), core);
    g_main_context_pop_thread_default(sessioin_core_get_main_context(core));
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

PipelineState
pipeline_get_state(Pipeline* pipe)
{
    return pipe->state;
}

void
pipeline_set_state(Pipeline* pipe,
                    PipelineState state)
{
    pipe->state = state;
}