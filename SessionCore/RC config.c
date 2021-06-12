#include "Framework.h"
#include "Variable.h"
#include "Handle data channel.h"
#include "Handle pipeline.h"
#include "RC config.h"
#include "CorePipeSink.h"
#include "Signalling handling.h"




gdouble*
process_bitrate_calculation(gint compose_bitrate)
{
	static gdouble temp[1];
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

void
attach_bitrate_control(GstObject* encoder, GstControlSource* controller)
{
	controller = gst_interpolation_control_source_new();

	g_object_set(controller, 
				"mode", 
				GST_INTERPOLATION_MODE_CUBIC_MONOTONIC, 
				NULL);

	gst_object_add_control_binding(encoder, gst_direct_control_binding_new(encoder, "bitrate", controller));	
}

void
set_dynamic_bitrate(gint compose_bitrate)
{
	GstTimedValueControlSource* tv_source_audio = (GstTimedValueControlSource*)audio_controller;
	GstTimedValueControlSource* tv_source_video = (GstTimedValueControlSource*)video_controller;

	gst_timed_value_control_source_set( tv_source_video, gst_element_get_current_running_time(webrtcbin), *process_bitrate_calculation(compose_bitrate));
	gst_timed_value_control_source_set( tv_source_audio, gst_element_get_current_running_time(webrtcbin), *(process_bitrate_calculation(compose_bitrate)+1));

	g_free(tv_source_audio);
	g_free(tv_source_video);
}