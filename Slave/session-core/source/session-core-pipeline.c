#include <session-core-pipeline.h>
#include <session-core-data-channel.h>
#include <session-core-signalling.h>
#include <session-core-remote-config.h>
#include <session-core-logging.h>
#include <gst\gst.h>
#include <glib-2.0\glib.h>

#include <Windows.h>

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

    VP9_ENCODER,
    VP8_ENCODER,


    /*payload packetize*/
    RTP_H264_PAYLOAD,
    RTP_H265_PAYLOAD,
    RTP_VP9_PAYLOAD,
    RTP_VP8_PAYLOAD,

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

    GstCaps* video_caps[VIDEO_ELEMENT_LAST];
    GstCaps* audio_caps[AUDIO_ELEMENT_LAST];
};


void
setup_element_factory_h264_media_foundation(SessionCore* core);

Pipeline*
pipeline_initialize(SessionCore* core)
{
    SignallingHub* hub = session_core_get_signalling_hub(core);

    static Pipeline pipeline;
    ZeroMemory(&pipeline,sizeof(pipeline));
    

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
    write_to_log_file(core,"Starting pipeline\n");
    return TRUE;
}

#define STUN_SERVER " stun-server=stun://stun.l.google.com:19302 "
#define SCREEN_CAP    "video/x-raw,framerate=120/1"
#define RTP_CAPS_OPUS "application/x-rtp,media=audio,encoding-name=OPUS,payload="
#define RTP_CAPS_VIDEO "application/x-rtp,media=video,encoding-name=H264,payload="

void
setup_element_factory(Pipeline* pipe,
    gchar* video, 
    gchar* audio)
{
    GError* error = NULL;
    
    if (!g_strcmp0(video, "H264"))
    {
        if (!g_strcmp0(audio, "OPUS")) 
        {
            pipe->pipeline =
                gst_parse_launch("webrtcbin bundle-policy=max-bundle name=sendrecv "STUN_SERVER
                    "dx9screencapsrc name=screencap ! "SCREEN_CAP
                    " ! queue ! videoconvert ! queue ! mfh264enc name=videoencoder ! rtph264pay name=rtp ! "
                    "queue ! " RTP_CAPS_VIDEO "96 ! sendrecv. "
                    "audiotestsrc is-live=true wave=red-noise ! audioconvert ! audioresample ! queue ! opusenc name=audioencoder ! rtpopuspay ! "
                    "queue ! " RTP_CAPS_OPUS "97 ! sendrecv. ", &error);

            pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION] = gst_bin_get_by_name(GST_BIN(pipe->pipeline), "videoencoder");
            pipe->video_element[RTP_H264_PAYLOAD] = gst_bin_get_by_name(GST_BIN(pipe->pipeline), "rtp");
            pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE] = gst_bin_get_by_name(GST_BIN(pipe->pipeline), "screencap");
            pipe->audio_element[OPUS_ENCODER] = gst_bin_get_by_name(GST_BIN(pipe->pipeline), "audioencoder");
        }
    }
    else if (!g_strcmp0(video, "H265"))
    {
        if (!g_strcmp0(audio, "OPUS"))
        {
            pipe->pipeline =
                gst_parse_launch("webrtcbin bundle-policy=max-bundle name=sendrecv "
                    "dx9screencapsrc name=screencap ! "SCREEN_CAP
                    " ! queue ! videoconvert ! queue ! mfh265enc name=videoencoder ! rtph265pay name=rtp ! "
                    "queue ! " RTP_CAPS_VIDEO "96 ! sendrecv. "
                    "audiotestsrc is-live=true wave=red-noise ! audioconvert ! audioresample ! queue ! opusenc name=audioencoder ! rtpopuspay ! "
                    "queue ! " RTP_CAPS_OPUS "97 ! sendrecv. ", &error);

            pipe->video_element[NVIDIA_H265_MEDIA_FOUNDATION] = gst_bin_get_by_name(GST_BIN(pipe->pipeline), "videoencoder");
            pipe->video_element[RTP_H265_PAYLOAD] = gst_bin_get_by_name(GST_BIN(pipe->pipeline), "rtp");
            pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE] = gst_bin_get_by_name(GST_BIN(pipe->pipeline), "screencap");
            pipe->audio_element[OPUS_ENCODER] = gst_bin_get_by_name(GST_BIN(pipe->pipeline), "audioencoder");
        }
    }
    else if (!g_strcmp0(video, "VP9"))
    {
        if (!g_strcmp0(audio, "OPUS"))
        {
            pipe->pipeline =
                gst_parse_launch("webrtcbin bundle-policy=max-bundle name=sendrecv "
                    "dx9screencapsrc name=screencap ! "SCREEN_CAP
                    " ! queue ! videoconvert ! queue ! vp9enc name=videoencoder ! rtpvp9pay name=rtp ! "
                    "queue ! " RTP_CAPS_VIDEO "96 ! sendrecv. "
                    "audiotestsrc is-live=true wave=red-noise ! audioconvert ! audioresample ! queue ! opusenc name=audioencoder ! rtpopuspay ! "
                    "queue ! " RTP_CAPS_OPUS "97 ! sendrecv. ", &error);

            pipe->video_element[VP9_ENCODER] = gst_bin_get_by_name(GST_BIN(pipe->pipeline), "videoencoder");
            pipe->video_element[RTP_VP9_PAYLOAD] = gst_bin_get_by_name(GST_BIN(pipe->pipeline), "rtp");
            pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE] = gst_bin_get_by_name(GST_BIN(pipe->pipeline), "screencap");
            pipe->audio_element[OPUS_ENCODER] = gst_bin_get_by_name(GST_BIN(pipe->pipeline), "audioencoder");
        }
    }

    if (error != NULL) {
        g_error_free(error);
        return NULL;
    }

    pipe->webrtcbin = gst_bin_get_by_name(GST_BIN(pipe->pipeline), "sendrecv");

    g_assert_nonnull(pipe->webrtcbin);
}

gpointer
setup_pipeline(SessionCore* core)
{

    SignallingHub* signalling = session_core_get_signalling_hub(core);
    Pipeline* pipe = session_core_get_pipeline(core);
    QoE* qoe= session_core_get_qoe(core);


    pipe->state = PIPELINE_CREATING_ELEMENT;

    
    
    if (qoe_get_video_codec(qoe) == CODEC_NVH264)
    {
        if (qoe_get_audio_codec(qoe) == OPUS_ENC)
        {
            setup_element_factory(pipe,"H264","OPUS");
        }
    }





    
    pipe->state = PIPELINE_SETTING_UP_ELEMENT;
    setup_element_property( core);

    pipe->state = PIPELINE_CONNECT_ELEMENT_SIGNAL;

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
    SignallingHub* hub = session_core_get_signalling_hub(core);

    g_main_context_push_thread_default(session_core_get_main_context(core));

    /*set policy-bundle*/
    //if (pipe->webrtcbin) { 
    //    g_object_set(pipe->webrtcbin, "bundle-policy", "max-bundle", NULL); }
    /* Add stun server */
    //if (pipe->webrtcbin) { 
    //    g_object_set(pipe->webrtcbin, "stun-server", signalling_hub_get_stun_server(hub), NULL); }

    /* This is the gstwebrtc entry point where we create the offer and so on. It
     * will be called when the pipeline goes to PLAYING. */
    g_signal_connect(pipe->webrtcbin, "on-negotiation-needed",
        G_CALLBACK(on_negotiation_needed), core);
    g_signal_connect(pipe->webrtcbin, "on-ice-candidate",
        G_CALLBACK(send_ice_candidate_message), core);
    g_signal_connect(pipe->webrtcbin, "notify::ice-gathering-state",
        G_CALLBACK(on_ice_gathering_state_notify), core);
    g_main_context_pop_thread_default(session_core_get_main_context(core));
}


void
stop_pipeline(Pipeline* pipe)
{
    if (pipe->pipeline != NULL)
    {
        gst_element_change_state(pipe->pipeline, GST_STATE_NULL);
    }
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