#include <remote-app-pipeline.h>
#include <remote-app-type.h>
#include <remote-app-data-channel.h>
#include <remote-app-signalling.h>
#include <remote-app-remote-config.h>


#include <general-constant.h>
#include <logging.h>
#include <qoe.h>
#include <exit-code.h>

#include <gst\gst.h>
#include <glib-2.0\glib.h>
#include <gst\webrtc\webrtc.h>
#include <gst\rtp\gstrtppayloads.h>
#include <gst\base\gstbasesink.h>
#include <gst\video\navigation.h>

#include <Windows.h>





/// <summary>
/// gstreamer video element enumaration,
/// the order of element in enum must follow the 
/// </summary>
enum
{
    /*screen capture source*/
    VIDEO_SINK,

    /*video encoder*/
    VIDEO_DECODER,

    /*payload packetize*/
    VIDEO_DEPAYLOAD,

    VIDEO_ELEMENT_LAST
};

/// <summary>
/// gstreamer audio element enumaration,
/// the order of element in enum must follow the 
/// </summary>
enum
{
    /*audio capture source*/
    PULSE_SINK,
    WASAPI_SINK,

    AUDIO_CONVERT,

    /*audio encoder*/
    OPUS_DECODER,
    AAC_DECODER,

    /*rtp packetize and queue*/
    AUDIO_DEPAYLOAD,

    AUDIO_ELEMENT_LAST
};


struct _Pipeline
{
	GstElement* pipeline;
	GstElement* webrtcbin;

    GstElement* video_element[VIDEO_ELEMENT_LAST];
    GstElement* audio_element[AUDIO_ELEMENT_LAST];

    GstCaps* video_caps[VIDEO_ELEMENT_LAST];
    GstCaps* audio_caps[AUDIO_ELEMENT_LAST];
};


void
setup_video_sink_navigator(RemoteApp* core);



Pipeline*
pipeline_initialize(RemoteApp* core)
{
    SignallingHub* hub = remote_app_get_signalling_hub(core);

    static Pipeline pipeline;
    ZeroMemory(&pipeline,sizeof(pipeline));
   

    return &pipeline;
}

static gboolean
start_pipeline(RemoteApp* core)
{
    GstStateChangeReturn ret;
    Pipeline* pipe = remote_app_get_pipeline(core);

    ret = GST_IS_ELEMENT(pipe->pipeline);    

    ret = gst_element_set_state(GST_ELEMENT(pipe->pipeline), GST_STATE_PLAYING);
    if (ret == GST_STATE_CHANGE_FAILURE)
    {
        GError error;
        error.message = "Fail to start pipeline, this may due to pipeline setup failure";
        remote_app_finalize(core, PIPELINE_ERROR_EXIT,&error);
    }
    return TRUE;
}


#define DIRECTX_PAD "video/x-raw(memory:D3D11Memory)"
#define RTP_CAPS_OPUS "application/x-rtp,media=audio,payload=96,encoding-name="
#define RTP_CAPS_VIDEO "application/x-rtp,media=video,payload=97,encoding-name="

static void
on_incoming_stream (GstElement * webrtc, GstPad * pad, RemoteApp* core)
{
    Pipeline* pipeline = remote_app_get_pipeline(core); 
    QoE* qoe = remote_app_get_qoe(core);
    GstPad* sinkpad;

    if (GST_PAD_DIRECTION (pad) != GST_PAD_SRC)
        return;
    
    switch(qoe_get_video_codec(qoe))
    {
    case CODEC_H264: 
        pipeline->video_element[VIDEO_DEPAYLOAD] = gst_element_factory_make("rtph264depay","depay");
        sinkpad = gst_element_get_static_pad (pipeline->video_element[VIDEO_DEPAYLOAD], "sink");
        pipeline->video_element[VIDEO_DECODER] = gst_element_factory_make("d3d11h264enc","decoder");
        pipeline->video_element[VIDEO_SINK] = gst_element_factory_make("d3d11videosink","videosink");
        break;
    case CODEC_H265:
        pipeline->video_element[VIDEO_DEPAYLOAD] = gst_element_factory_make("rtph265depay","depay");
        sinkpad = gst_element_get_static_pad (pipeline->video_element[VIDEO_DEPAYLOAD], "sink");
        pipeline->video_element[VIDEO_DECODER] = gst_element_factory_make("d3d11h265enc","decoder");
        pipeline->video_element[VIDEO_SINK] = gst_element_factory_make("d3d11videosink","videosink");
        break;
    case CODEC_VP9:
        pipeline->video_element[VIDEO_DEPAYLOAD] = gst_element_factory_make("rtpvp9depay","depay");
        sinkpad = gst_element_get_static_pad (pipeline->video_element[VIDEO_DEPAYLOAD], "sink");
        pipeline->video_element[VIDEO_DECODER] = gst_element_factory_make("d3d11vp9dec","decoder");
        pipeline->video_element[VIDEO_SINK] = gst_element_factory_make("d3d11videosink","videosink");
        break; 
    default:
        break;
    }

    switch(qoe_get_audio_codec(qoe))
    {
    case OPUS_ENC: 
        pipeline->audio_element[AUDIO_DEPAYLOAD] = gst_element_factory_make("rtpopusdepay","depay");
        sinkpad = gst_element_get_static_pad (pipeline->audio_element[AUDIO_DEPAYLOAD], "sink");
        pipeline->audio_element[AUDIO_CONVERT] = gst_element_factory_make("audioconvert","convert");
        pipeline->audio_element[OPUS_DECODER] = gst_element_factory_make("opusdec","decoder");
        pipeline->audio_element[WASAPI_SINK] = gst_element_factory_make("autovideosink","audiosink");
        break;
    case CODEC_H265:
        pipeline->audio_element[AUDIO_DEPAYLOAD] = gst_element_factory_make("rtpopusdepay","depay");
        sinkpad = gst_element_get_static_pad (pipeline->audio_element[AUDIO_DEPAYLOAD], "sink");
        pipeline->audio_element[AUDIO_CONVERT] = gst_element_factory_make("audioconvert","convert");
        pipeline->audio_element[OPUS_DECODER] = gst_element_factory_make("opusdec","decoder");
        pipeline->audio_element[WASAPI_SINK] = gst_element_factory_make("autovideosink","audiosink");
        break;
    case CODEC_VP9:
        pipeline->audio_element[AUDIO_DEPAYLOAD] = gst_element_factory_make("rtpopusdepay","depay");
        sinkpad = gst_element_get_static_pad (pipeline->audio_element[AUDIO_DEPAYLOAD], "sink");
        pipeline->audio_element[AUDIO_CONVERT] = gst_element_factory_make("audioconvert","convert");
        pipeline->audio_element[OPUS_DECODER] = gst_element_factory_make("opusdec","decoder");
        pipeline->audio_element[WASAPI_SINK] = gst_element_factory_make("autovideosink","audiosink");
        break; 
    default:
        break;
    }
    for(gint i = 0;i < AUDIO_ELEMENT_LAST;i++ )
    {
        
        if(pipeline->audio_element[i])
        {
            gst_bin_add (GST_BIN (pipeline->pipeline), pipeline->audio_element[i]);
            gst_element_sync_state_with_parent (pipeline->audio_element[i]);
        }
    }

    for(gint i = 0;i < VIDEO_ELEMENT_LAST;i++ )
    {
        
        if(pipeline->video_element[i])
        {
            gst_bin_add (GST_BIN (pipeline->pipeline), pipeline->video_element[i]);
            gst_element_sync_state_with_parent (pipeline->video_element[i]);
        }
    }
    gst_element_link_many(
        pipeline->video_element[VIDEO_DEPAYLOAD],
        pipeline->video_element[VIDEO_DECODER],
        pipeline->video_element[VIDEO_SINK],NULL);
    
    
    gst_element_link_many(
        pipeline->audio_element[AUDIO_DEPAYLOAD],
        pipeline->audio_element[OPUS_DECODER],
        pipeline->audio_element[AUDIO_CONVERT],
        pipeline->audio_element[WASAPI_SINK],NULL);

    setup_video_sink_navigator(core);
    gst_pad_link (pad, sinkpad);
    gst_object_unref (sinkpad);
}



/// <summary>
/// connect webrtc bin to ice and sdp signal handler
/// </summary>
/// <param name="core"></param>
static void
connect_signalling_handler(RemoteApp* core)
{
    Pipeline* pipe = remote_app_get_pipeline(core);
    SignallingHub* hub = remote_app_get_signalling_hub(core);

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
    /* Incoming streams will be exposed via this signal */
    g_signal_connect(pipe->webrtcbin, "pad-added",
        G_CALLBACK (on_incoming_stream),core);
}


EventHandler default_handler;

gboolean      
handle_navigator(GstBaseSink *sink, GstEvent *event)
{
    // overwrite default handler by adding more step to the handler
    default_handler(sink,event);

    switch (gst_navigation_event_get_type(event))
    {
    case GST_NAVIGATION_EVENT_INVALID: 
        return; 
    case GST_NAVIGATION_EVENT_KEY_PRESS: 
        return; 
    case GST_NAVIGATION_EVENT_KEY_RELEASE: 
        return;
    case GST_NAVIGATION_EVENT_MOUSE_MOVE: 
        return; 
    case GST_NAVIGATION_EVENT_MOUSE_SCROLL: 
        return; 
    case GST_NAVIGATION_EVENT_MOUSE_BUTTON_PRESS: 
        return; 
    case GST_NAVIGATION_EVENT_MOUSE_BUTTON_RELEASE: 
        return; 
    default:
        break;
    }

}


static void
setup_video_sink_navigator(RemoteApp* core)
{
    Pipeline* pipeline = remote_app_get_pipeline(core);
    GstBaseSink* basesink = (GstBaseSink*) pipeline->video_element[VIDEO_SINK];


    GstBaseSinkClass* klass = GST_BASE_SINK_GET_CLASS(basesink);
    default_handler = klass->event;
    klass->event = handle_navigator;
}





gpointer
setup_pipeline(RemoteApp* core)
{
    SignallingHub* signalling = remote_app_get_signalling_hub(core);
    Pipeline* pipe = remote_app_get_pipeline(core);

    GError* error = NULL;
    pipe->pipeline = gst_parse_launch("webrtcbin name=webrtc",&error);
    pipe->webrtcbin =  gst_bin_get_by_name(GST_BIN(pipe->pipeline),"webrtc");

    connect_signalling_handler(core);
    gst_element_change_state(pipe->pipeline, GST_STATE_READY);
    connect_data_channel_signals(core);
    start_pipeline(core);


    signalling_hub_set_peer_call_state(signalling, PEER_CALL_DONE);
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


