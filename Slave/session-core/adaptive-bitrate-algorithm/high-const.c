/// <summary>
/// @file ultra-low-const.c
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-06
/// 
/// @copyright Copyright (c) 2021
/// 
#include <high-const.h>
#include <session-core-type.h>
#include <gst/gst.h>
#include <session-core-pipeline.h>
#include <qoe.h>
#include <session-core.h>
#include <session-core-remote-config.h>


#define HIGH_CONST_AUDIO_BITRATE						50000
#define HIGH_CONST_VIDEO_BITRATE						256000




/// <summary>
/// </summary>
/// 
/// <param name="core"></param>
void
high_const(SessionCore* core, 
            QualitySample sample)
{
	Pipeline* pipe = session_core_get_pipeline(core);
	QoE* qoe = session_core_get_qoe(core);

	Codec video = qoe_get_video_codec(qoe);
	Codec audio = qoe_get_audio_codec(qoe);


	GstElement* video_encoder = pipeline_get_video_encoder(pipe,video);
	GstElement* audio_encoder = pipeline_get_audio_encoder(pipe,audio);


	g_object_set(video_encoder,"bitrate",
		(gint)(HIGH_CONST_VIDEO_BITRATE),NULL);
	g_object_set(audio_encoder,"bitrate",
		(gint)(HIGH_CONST_AUDIO_BITRATE),NULL);

}