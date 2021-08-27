#include <session-core-pipeline.h>
#include <session-core-type.h>
#include <session-core-data-channel.h>
#include <session-core-signalling.h>
#include <session-core-remote-config.h>


#include <general-constant.h>
#include <logging.h>
#include <qoe.h>
#include <exit-code.h>

#include <gst\gst.h>
#include <glib-2.0\glib.h>
#include <gst\webrtc\webrtc.h>
#include <gst\rtp\gstrtppayloads.h>

#include <Windows.h>




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

    ret = gst_element_set_state(GST_ELEMENT(pipe->pipeline), GST_STATE_PLAYING);
    if (ret == GST_STATE_CHANGE_FAILURE)
    {
        session_core_finalize(core, PIPELINE_ERROR_EXIT,NULL);
    }
    write_to_log_file(SESSION_CORE_GENERAL_LOG,"Starting pipeline\n");
    return TRUE;
}

#define SCREEN_CAP    "video/x-raw,framerate=120/1"
#define RTP_CAPS_OPUS "application/x-rtp,media=audio,payload=96,encoding-name="
#define RTP_CAPS_VIDEO "application/x-rtp,media=video,payload=97,encoding-name="

void
setup_element_factory(SessionCore* core,
    Codec video, 
    Codec audio)
{
    Pipeline* pipe = session_core_get_pipeline(core);
    GError* error = NULL;
    
    if (video == CODEC_H264)
    {
        if (OPUS_ENC) 
        {
            pipe->pipeline =
                gst_parse_launch("webrtcbin bundle-policy=max-bundle name=sendrecv "
                    "dx9screencapsrc name=screencap ! "SCREEN_CAP
                    " ! queue ! videoconvert ! queue ! mfh264enc name=videoencoder ! queue ! rtph264pay name=rtp ! "
                    "queue ! " RTP_CAPS_VIDEO "H264 ! sendrecv. "
                    "audiotestsrc is-live=true wave=red-noise ! audioconvert ! audioresample ! queue ! opusenc name=audioencoder ! rtpopuspay ! "
                    "queue ! " RTP_CAPS_OPUS "OPUS ! sendrecv. ", &error);


            pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "videoencoder");
            pipe->video_element[RTP_H264_PAYLOAD] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "rtp");
            pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "screencap");
            pipe->audio_element[OPUS_ENCODER] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "audioencoder");
        }
    }
    else if (video == CODEC_H265)
    {
        if (audio == OPUS_ENC)
        {
            pipe->pipeline =
                gst_parse_launch("webrtcbin bundle-policy=max-bundle name=sendrecv "
                    "dx9screencapsrc name=screencap ! "SCREEN_CAP
                    " ! queue ! videoconvert ! queue ! mfh265enc name=videoencoder ! rtph265pay name=rtp ! "
                    "queue ! " RTP_CAPS_VIDEO "96 ! sendrecv. "
                    "audiotestsrc is-live=true wave=red-noise ! audioconvert ! audioresample ! queue ! opusenc name=audioencoder ! rtpopuspay ! "
                    "queue ! " RTP_CAPS_OPUS "97 ! sendrecv. ", &error);

            pipe->video_element[NVIDIA_H265_MEDIA_FOUNDATION] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "videoencoder");
            pipe->video_element[RTP_H265_PAYLOAD] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "rtp");
            pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "screencap");
            pipe->audio_element[OPUS_ENCODER] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "audioencoder");
        }
    }
    else if (video == CODEC_VP9)
    {
        if (audio == OPUS_ENC)
        {
            pipe->pipeline =
                gst_parse_launch("webrtcbin bundle-policy=max-bundle name=sendrecv "
                    "dx9screencapsrc name=screencap ! "SCREEN_CAP
                    " ! queue ! videoconvert ! queue ! vp9enc name=videoencoder ! rtpvp9pay name=rtp ! "
                    "queue ! " RTP_CAPS_VIDEO "VP9 ! sendrecv. "
                    "audiotestsrc is-live=true wave=red-noise ! audioconvert ! audioresample ! queue ! opusenc name=audioencoder ! rtpopuspay ! "
                    "queue ! " RTP_CAPS_OPUS "OPUS ! sendrecv. ", &error);

            pipe->video_element[VP9_ENCODER] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "videoencoder");
            pipe->video_element[RTP_VP9_PAYLOAD] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "rtp");
            pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "screencap");
            pipe->audio_element[OPUS_ENCODER] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "audioencoder");
        }
    }

    if (!error == NULL) {
        session_core_finalize(core,PIPELINE_ERROR_EXIT,error);
    }
    pipe->webrtcbin =
        gst_bin_get_by_name(GST_BIN(pipe->pipeline), "sendrecv");

}

gpointer
setup_pipeline(SessionCore* core)
{

    SignallingHub* signalling = session_core_get_signalling_hub(core);
    Pipeline* pipe = session_core_get_pipeline(core);
    QoE* qoe= session_core_get_qoe(core);


    pipe->state = PIPELINE_CREATING_ELEMENT;

    
    
    setup_element_factory(core, 
        qoe_get_video_codec(qoe),
        qoe_get_audio_codec(qoe));

    pipe->state = PIPELINE_CONNECT_ELEMENT_SIGNAL;
    connect_signalling_handler(core);
    
    pipe->state = PIPELINE_SETTING_UP_ELEMENT;
    setup_element_property(core);
    //attach_bitrate_control(core);



    gst_element_change_state(pipe->pipeline, GST_STATE_READY);

    connect_data_channel_signals(core);
    pipe->state = PIPELINE_SETUP_DONE;

    start_pipeline(core);

    session_core_set_state(core, REMOTE_CONNECT_STARTED);
    signalling_hub_set_peer_call_state(signalling, PEER_CALL_DONE);
}





void
setup_element_property(SessionCore* core)
{
    Pipeline* pipe = session_core_get_pipeline(core);
    SignallingHub* hub = session_core_get_signalling_hub(core);
    QoE* qoe = session_core_get_qoe(core);



    // /*turn off screeen cursor*/
    // if (pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE]) { g_object_set(pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE], "cursor", FALSE, NULL); }

    // /*monitor to display*/
    // if (pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE]) { g_object_set(pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE], "monitor", 0, NULL);}

    // /*variable bitrate mode*/
    // if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "rc-mode", "cbr", NULL);}

    if (pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION]) { g_object_set(pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION], "rc-mode", 0, NULL);}

    // if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "bitrate", 20000, NULL);}

    // if (pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION]) { g_object_set(pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION], "bitrate", 200000, NULL);}

    // if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "qos", TRUE, NULL);}

    // if (pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION]) { g_object_set(pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION], "qos", TRUE, NULL);}

    // /*low latency preset*/
    // if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "preset", "low-latency", NULL);}

    if (pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION]) { g_object_set(pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION], "low-latency", TRUE, NULL);}

    // if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "bitrate", qoe_get_video_bitrate(qoe), NULL);}

    // if (pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION]) { g_object_set(pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION], "bitrate", qoe_get_video_bitrate(qoe), NULL);}


    // /*set b-frame numbers property*/
    // if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "bframes", 0, NULL);}

    // /**/
    // if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "zerolatency", TRUE, NULL);}

    // /*set zero latency aggregate mode*/
    // if (pipe->video_element[RTP_H264_PAYLOAD]) { g_object_set(pipe->video_element[RTP_H264_PAYLOAD], "aggregate-mode", 1, NULL);}



    

    // if (pipe->audio_element[WASAPI_SOURCE_SOUND]) { g_object_set(pipe->audio_element[WASAPI_SOURCE_SOUND], "low-latency", TRUE, NULL);}

    // if (pipe->audio_element[OPUS_ENCODER]) { g_object_set(pipe->audio_element[OPUS_ENCODER], "bitrate", qoe_get_audio_bitrate(qoe), NULL);}

    // /*
    // * Set the queue max time to 16ms (16000000ns)
    // * If the pipeline is behind by more than 1s, the packets
    // * will be dropped.
    // * This helps buffer out latency in the audio source.
    // */
    // if (pipe->audio_element[RTP_RTX_QUEUE]) { g_object_set(pipe->audio_element[RTP_RTX_QUEUE], "max-size-time", 16000000, NULL);}

    // /*
    // Set the other queue sizes to 0 to make it only time-based.*/
    // if (pipe->audio_element[RTP_RTX_QUEUE]) { g_object_set(pipe->audio_element[RTP_RTX_QUEUE], "max-size-packet", 0, NULL);}


}


void
incoming_stream(GstElement* element, GstPad* pad, gpointer data)
{
    return;
}



void
connect_signalling_handler(SessionCore* core)
{
    Pipeline* pipe = session_core_get_pipeline(core);
    SignallingHub* hub = session_core_get_signalling_hub(core);

    //g_main_context_push_thread_default(session_core_get_main_context(core));

    /* Add stun server */
    g_object_set(pipe->webrtcbin, "stun-server", 
       signalling_hub_get_stun_server(hub), NULL); 

    /* This is the gstwebrtc entry point where we create the offer and so on. It
     * will be called when the pipeline goes to PLAYING. */
    g_signal_connect(pipe->webrtcbin, "on-negotiation-needed",
        G_CALLBACK(on_negotiation_needed), core);
    g_signal_connect(pipe->webrtcbin, "on-ice-candidate",
        G_CALLBACK(send_ice_candidate_message), core);
    g_signal_connect(pipe->webrtcbin, "notify::ice-gathering-state",
        G_CALLBACK(on_ice_gathering_state_notify), core);
    //g_signal_connect(pipe->webrtcbin, "pad-added", 
    //    G_CALLBACK(incoming_stream), NULL);
    //g_main_context_pop_thread_default(session_core_get_main_context(core));
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

GstElement*
pipeline_get_video_encoder(Pipeline* pipe, Codec video)
{
    
    if (video == CODEC_H264) { return pipe->video_element[NVIDIA_H264_MEDIA_FOUNDATION];}
    else if (pipe->video_element[CODEC_VP9]) { return pipe->video_element[VP9_ENCODER];}
    else if (pipe->video_element[CODEC_VP8]) { return pipe->video_element[VP8_ENCODER];}
    return NULL;
}

GstElement*
pipeline_get_audio_encoder(Pipeline* pipe, Codec audio)
{
    
    if (audio == OPUS_ENC) { return pipe->video_element[OPUS_ENCODER];}
    else if (audio == AAC_ENC) { return pipe->audio_element[AAC_ENCODER];}
    return NULL;
}

