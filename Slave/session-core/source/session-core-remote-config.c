#include <session-core-remote-config.h>
#include <session-core-type.h>
#include <session-core-pipeline.h>

#include <opcode.h>
#include <qoe.h>
#include <codec.h>

#include <gst\gst.h>
#include <glib-2.0\glib.h>
#include <gst/controller/gstinterpolationcontrolsource.h>
#include <gst/controller/gstdirectcontrolbinding.h>

#define DEFAULT_WIDTH				1920
#define DEFAULT_HEIGHT				1080
#define DEFAULT_RAMERATE			60
#define DEFAULT_VIDEO_BITRATE		1000
#define DEFAULT_AUDIO_BITRATE		1000
#define DEFAULT_AUDIO_CODEC			OPUS_ENC
#define DEFAULT_VIDEO_CODEC			CODEC_NVH264
#define DEFAULT_MODE				VIDEO_PIORITY


#define SAMPLE_QUEUE_LENGTH			1024


typedef struct _QualitySample
{
	gint time;
	gint framerate;

	gint video_bitrate;
	gint audio_bitrate;
	
	gint video_latency;
	gint audio_latency;

	gint available_bandwidth;
	gint packets_lost;

}QualitySample;

struct _QoE
{
	/*non volatile value, 
	*determine in session initialize time*/
	gint screen_height;
	gint screen_width;

	gint audio_bitrate;
	gint video_bitrate;


	gint framerate;


	/*sample 1024 value*/
	QualitySample sample[SAMPLE_QUEUE_LENGTH];
	gint current_sample;
	gint current_reported_sample;

	/*quality control mode*/
	QoEMode mode;

	/*codec audio*/
	Codec codec_audio;
	Codec codec_video;

	/*control bind stuff*/
	GstTimedValueControlSource* audio_controller;
	GstTimedValueControlSource* video_controller;

	GstControlSource* audio_control_source;
	GstControlSource* video_control_source;
};


QoE*
qoe_initialize()
{
	static QoE qoe;


	qoe.screen_width = DEFAULT_WIDTH;
	qoe.screen_height = DEFAULT_HEIGHT;
	qoe.framerate = DEFAULT_RAMERATE;
	qoe.audio_bitrate = DEFAULT_AUDIO_BITRATE;
	qoe.audio_bitrate = DEFAULT_VIDEO_BITRATE;
	qoe.mode = DEFAULT_MODE;
	
	qoe.audio_control_source = gst_interpolation_control_source_new();
	qoe.video_control_source = gst_interpolation_control_source_new();

	qoe.current_reported_sample = 0;
	qoe.current_sample = 0;
	return &qoe;
}


void
qoe_setup(QoE* qoe,
		  gint screen_width,
		  gint screen_height,
		  gint framerate,
		  gint bitrate,
		  Codec audio_codec,
		  Codec video_codec,
		  QoEMode qoe_mode)
{
	qoe->framerate = framerate;
	qoe->screen_width = screen_width;
	qoe->screen_height = screen_height;
	qoe->video_bitrate = bitrate;
	qoe->audio_bitrate = DEFAULT_AUDIO_BITRATE;
	qoe->codec_audio = audio_codec;
	qoe->codec_video = video_codec;
	qoe->mode = qoe_mode;
}



/// <summary>
/// attach bitrate controller with element in pipeline, 
/// more information can be found in gstreamer-dynamic controllable variable
/// </summary>
/// <param name="encoder"></param>
/// <param name="controller"></param>
void
attach_bitrate_control(GstObject* audio_encoder, 
					   GstObject* video_encoder,
					   SessionCore* core)
{
	QoE* qoe = session_core_get_qoe(core);
	switch(qoe->mode)
	{
	case DEFAULT_MODE:
	{
		g_object_set(qoe->audio_control_source, "mode",
			GST_INTERPOLATION_MODE_CUBIC_MONOTONIC, NULL);

		gst_object_add_control_binding(audio_encoder,
			gst_direct_control_binding_new(audio_encoder, "bitrate", qoe->audio_control_source));


		g_object_set(qoe->video_control_source, "mode",
			GST_INTERPOLATION_MODE_CUBIC_MONOTONIC, NULL);

		gst_object_add_control_binding(video_encoder,
			gst_direct_control_binding_new(video_encoder, "bitrate", qoe->video_control_source));

	}
	}
}

process_bitrate_calculation(gint bitrate, QoE* qoe)
{
	return bitrate;
}

void
set_dynamic_bitrate(Pipeline* pipe, QoE* qoe)
{


	GstTimedValueControlSource* tv_source_audio = 
		(GstTimedValueControlSource*)qoe->audio_controller;


	GstTimedValueControlSource* tv_source_video = 
		(GstTimedValueControlSource*)qoe->video_controller;

	gst_timed_value_control_source_set( tv_source_video, 
		gst_element_get_current_running_time(pipeline_get_webrtc_bin(pipe)), 
		process_bitrate_calculation(qoe->video_bitrate,qoe));


	gst_timed_value_control_source_set( tv_source_audio, 
		gst_element_get_current_running_time(pipeline_get_webrtc_bin(pipe)),
		process_bitrate_calculation(qoe->audio_bitrate,qoe));

	g_free(tv_source_audio);
	g_free(tv_source_video);
}


void
qoe_update_quality(QoE* qoe,
	gint time,
	gint framerate,
	gint audio_latency,
	gint video_latency,
	gint audio_bitrate,
	gint video_bitrate,
	gint bandwidth,
	gint packets_lost)
{
	qoe->sample[qoe->current_reported_sample].available_bandwidth = bandwidth;
	qoe->sample[qoe->current_reported_sample].packets_lost = packets_lost;
	qoe->sample[qoe->current_reported_sample].framerate = framerate;
	qoe->sample[qoe->current_reported_sample].time = time;
	qoe->sample[qoe->current_reported_sample].video_latency = video_latency;
	qoe->sample[qoe->current_reported_sample].audio_latency = audio_latency;
	qoe->sample[qoe->current_reported_sample].audio_bitrate = audio_bitrate;
	qoe->sample[qoe->current_reported_sample].video_bitrate = video_bitrate;
	qoe->current_reported_sample++;
}


gint
qoe_get_screen_width(QoE* qoe)
{
	return qoe->screen_width;
}

gint
qoe_get_screen_height(QoE* qoe)
{
	return qoe->screen_height;
}

gint
qoe_get_framerate(QoE* qoe)
{
	return qoe->framerate;
}

gint
qoe_get_video_bitrate(QoE* qoe)
{
	return qoe->video_bitrate;
}

gint
qoe_get_audio_bitrate(QoE* qoe)
{
	return qoe->audio_bitrate;
}

Codec
qoe_get_audio_codec(QoE* qoe)
{
	return qoe->codec_audio;
}

Codec
qoe_get_video_codec(QoE* qoe)
{
	return qoe->codec_video;
}


QoEMode
qoe_get_mode(QoE* qoe)
{
	return qoe->mode;
}