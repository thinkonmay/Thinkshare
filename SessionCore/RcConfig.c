#include "RcConfig.h"


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
	SessionQoE* qoe)
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
attach_bitrate_control(GstObject* encoder, 
	GstControlSource* controller)
{
	controller = gst_interpolation_control_source_new();

	g_object_set(controller, 
				"mode", 
				GST_INTERPOLATION_MODE_CUBIC_MONOTONIC, 
				NULL);

	gst_object_add_control_binding(encoder, 
		gst_direct_control_binding_new(encoder, "bitrate", controller));	

	return;
}



void
set_dynamic_bitrate(Pipeline* pipe, SessionQoE* qoe)
{
	GstTimedValueControlSource* tv_source_audio = 
		(GstTimedValueControlSource*)qoe->audio_controller;


	GstTimedValueControlSource* tv_source_video = 
		(GstTimedValueControlSource*)qoe->video_controller;

	gst_timed_value_control_source_set( tv_source_video, 
		gst_element_get_current_running_time(pipe->webrtcbin), 
		*process_bitrate_calculation(qoe->compose_bitrate,qoe));


	gst_timed_value_control_source_set( tv_source_audio, 
		gst_element_get_current_running_time(pipe->webrtcbin),
		*(process_bitrate_calculation(qoe->compose_bitrate,qoe)+1));

	g_free(tv_source_audio);
	g_free(tv_source_video);
}