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
handle_media_stream (GstPad * pad, GstElement * pipe, const char *convert_name,
    const char *sink_name)
{
  GstPad *qpad;
  GstElement *q, *conv, *resample, *sink;
  GstPadLinkReturn ret;

  g_print ("Trying to handle stream with %s ! %s", convert_name, sink_name);

  q = gst_element_factory_make ("queue", NULL);
  g_assert_nonnull (q);
  conv = gst_element_factory_make (convert_name, NULL);
  g_assert_nonnull (conv);
  sink = gst_element_factory_make (sink_name, NULL);
  g_assert_nonnull (sink);

  if (g_strcmp0 (convert_name, "audioconvert") == 0) {
    /* Might also need to resample, so add it just in case.
     * Will be a no-op if it's not required. */
    resample = gst_element_factory_make ("audioresample", NULL);
    g_assert_nonnull (resample);
    gst_bin_add_many (GST_BIN (pipe), q, conv, resample, sink, NULL);
    gst_element_sync_state_with_parent (q);
    gst_element_sync_state_with_parent (conv);
    gst_element_sync_state_with_parent (resample);
    gst_element_sync_state_with_parent (sink);
    gst_element_link_many (q, conv, resample, sink, NULL);
  } else {
    gst_bin_add_many (GST_BIN (pipe), q, conv, sink, NULL);
    gst_element_sync_state_with_parent (q);
    gst_element_sync_state_with_parent (conv);
    gst_element_sync_state_with_parent (sink);
    gst_element_link_many (q, conv, sink, NULL);
  }

  qpad = gst_element_get_static_pad (q, "sink");

  ret = gst_pad_link (pad, qpad);
  g_assert_cmphex (ret, ==, GST_PAD_LINK_OK);
}

static void
on_incoming_decodebin_stream (GstElement * decodebin, GstPad * pad,
    GstElement * pipe)
{
  GstCaps *caps;
  const gchar *name;

  if (!gst_pad_has_current_caps (pad)) {
    g_printerr ("Pad '%s' has no caps, can't do anything, ignoring\n",
        GST_PAD_NAME (pad));
    return;
  }

  caps = gst_pad_get_current_caps (pad);
  name = gst_structure_get_name (gst_caps_get_structure (caps, 0));

  if (g_str_has_prefix (name, "video")) {
    handle_media_stream (pad, pipe, "videoconvert", "d3dvideosink");
  } else if (g_str_has_prefix (name, "audio")) {
    handle_media_stream (pad, pipe, "audioconvert", "autoaudiosink");
  } else {
    g_printerr ("Unknown pad %s, ignoring", GST_PAD_NAME (pad));
  }
}

static void
on_incoming_stream (GstElement * webrtc, GstPad * pad, GstElement * pipe)
{
  GstCaps* caps;
  const gchar* name ,* encoding;
  GstElement *decodebin;
  GstPad *sinkpad;
  
  caps = gst_pad_get_current_caps(pad);
  encoding = gst_structure_get_string(gst_caps_get_structure(caps, 0), "encoding-name");
  name = gst_structure_get_name(gst_caps_get_structure(caps, 0));

  if (GST_PAD_DIRECTION (pad) != GST_PAD_SRC)
    return;


  decodebin = gst_element_factory_make ("decodebin", NULL);
  g_signal_connect (decodebin, "pad-added",
      G_CALLBACK (on_incoming_decodebin_stream), pipe);
  gst_bin_add (GST_BIN (pipe), decodebin);
  gst_element_sync_state_with_parent (decodebin);

  sinkpad = gst_element_get_static_pad (decodebin, "sink");
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

    g_object_set(pipe->webrtcbin, "turn-server", 
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
        G_CALLBACK (on_incoming_stream),pipe->pipeline);
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
    pipe->pipeline = gst_parse_launch(
                    "webrtcbin name=webrtcbin "
                    "videotestsrc is-live=true wave=red-noise ! mfh264enc ! queue ! rtph264pay ! "
                    "queue ! " RTP_CAPS_OPUS "97 ! webrtcbin.",&error);
    pipe->webrtcbin =  gst_bin_get_by_name(GST_BIN(pipe->pipeline),"webrtcbin");

    g_object_set(pipe->webrtcbin, "latency", 50, NULL);

    connect_signalling_handler(core);
    gst_element_change_state(pipe->pipeline, GST_STATE_READY);
    // connect_data_channel_signals(core);
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


