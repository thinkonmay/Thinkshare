#include <session-core-remote-config.h>
#include <session-core-type.h>
#include <session-core-pipeline.h>

#include <gst\gst.h>
#include <glib-2.0\glib.h>
#include <gst/controller/gstinterpolationcontrolsource.h>
#include <gst/controller/gstdirectcontrolbinding.h>

#define DEFAULT_WIDTH				1920
#define DEFAULT_HEIGHT				1080
#define DEFAULT_RAMERATE			60
#define DEFAULT_BITRATE				100000
#define DEFAULT_AUDIO_CODEC			OPUS_ENC
#define DEFAULT_VIDEO_CODEC			CODEC_NVH264
#define DEFAULT_MODE				VIDEO_PIORITY


struct _QoE
{
	gint screen_height;
	gint screen_width;
	gint framerate;

	gint compose_bitrate;

	QoEMode mode;

	Codec codec_audio;
	Codec codec_video;

	GstTimedValueControlSource* audio_controller;
	GstTimedValueControlSource* video_controller;

	GstControlSource* audio;
	GstControlSource* video;
};


QoE*
qoe_initialize()
{
	QoE* qoe = malloc(sizeof(QoE));
	memset(qoe, 0, sizeof(QoE));


	qoe->screen_width = DEFAULT_WIDTH;
	qoe->screen_height = DEFAULT_HEIGHT;
	qoe->framerate = DEFAULT_RAMERATE;
	qoe->compose_bitrate = DEFAULT_BITRATE;
	qoe->mode = DEFAULT_MODE;
	
	qoe->audio = gst_interpolation_control_source_new();
	qoe->video = gst_interpolation_control_source_new();


	return qoe;
}


void
qoe_setup(QoE* qoe,
		  gint screen_width,
		  gint screen_height,
		  gint framerate,
		  gint bitrate)
{
	qoe->framerate = framerate;
	qoe->screen_width = screen_width;
	qoe->screen_height = screen_height;
	qoe->compose_bitrate = bitrate;
}




/// <summary>
/// perform adaptive bitrate calculation here
/// the algorithm should base on the QoE mode to compensate video and audio bitrate,
/// further more, some work will related to adjust framerate and resolution.
/// </summary>
/// <param name="compose_bitrate"></param>
/// <param name="qoe"></param>
/// <returns></returns>
gdouble*
process_bitrate_calculation(gint compose_bitrate, 
							QoE* qoe)
{
	static gdouble temp[2];
	QoEMode mode = qoe->mode;
	switch (mode)
	{
	case (VIDEO_PIORITY):
		temp[0] = compose_bitrate * 0.8;
		temp[1] = compose_bitrate * 0.1;

	case (AUDIO_PIORITY):
		temp[0] = compose_bitrate * 0.7;
		temp[1] = compose_bitrate * 0.2;
	}
	return temp;
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
		g_object_set(qoe->audio, "mode",
			GST_INTERPOLATION_MODE_CUBIC_MONOTONIC, NULL);

		gst_object_add_control_binding(audio_encoder,
			gst_direct_control_binding_new(audio_encoder, "bitrate", qoe->audio));


		g_object_set(qoe->video, "mode",
			GST_INTERPOLATION_MODE_CUBIC_MONOTONIC, NULL);

		gst_object_add_control_binding(video_encoder,
			gst_direct_control_binding_new(video_encoder, "bitrate", qoe->video));

	}
	}
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
		*process_bitrate_calculation(qoe->compose_bitrate,qoe));


	gst_timed_value_control_source_set( tv_source_audio, 
		gst_element_get_current_running_time(pipeline_get_webrtc_bin(pipe)),
		*(process_bitrate_calculation(qoe->compose_bitrate,qoe)+1));

	g_free(tv_source_audio);
	g_free(tv_source_video);
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