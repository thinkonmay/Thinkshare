/// <summary>
/// @file ultra-low-const.c
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-06
/// 
/// @copyright Copyright (c) 2021
/// 
#include <session-core-type.h>
#include <gst/gst.h>



#define LOW_CONST_AUDIO_BITRATE							5000
#define LOW_CONST_VIDEO_BITRATE							64000



/// <summary>
/// </summary>
/// 
/// <param name="core"></param>
static void
ultra_low_const(SessionCore* core,
                QualitySample sample)
{
	Pipeline* pipe = session_core_get_pipeline(core);
	QoE* qoe = session_core_get_qoe(core);

	GstElement* video_encoder = pipeline_get_video_encoder(pipe,qoe->codec_video);
	GstElement* audio_encoder = pipeline_get_audio_encoder(pipe,qoe->codec_audio);

	g_object_set(video_encoder,"bitrate",
		(gint)(LOW_CONST_VIDEO_BITRATE),NULL);
	g_object_set(audio_encoder,"bitrate",
		(gint)(LOW_CONST_AUDIO_BITRATE),NULL);

}