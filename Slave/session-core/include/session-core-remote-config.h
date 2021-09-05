/// <summary>
/// @file session-core-remote-config.h
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-05
/// 
/// @copyright Copyright (c) 2021
/// 
#include "session-core-type.h"
#include "session-core.h"
#include <gst/gst.h>
#include <qoe.h>

void			attach_bitrate_control				(SessionCore* core);

void			set_dynamic_bitrate					(Pipeline* pipe, 
												     QoE* qoe);

QoE*			qoe_initialize						();

void			qoe_setup							(QoE* qoe,
		  											gint screen_width,
		  											gint screen_height,
		  											Codec audio_codec,
		  											Codec video_codec,
		  											QoEMode qoe_mode);

gint			qoe_get_screen_width				(QoE* qoe);

gint			qoe_get_screen_height				(QoE* qoe);

gint			qoe_get_framerate					(QoE* qoe);


/// <summary>
/// </summary>
/// update current qoe metric parameter 
/// (related to experience quality of the stream),
/// then, adaptive bitrate algorithm will be applied (if available)
/// <param name="core"></param>
/// <param name="time"></param>
/// <param name="framerate"></param>
/// <param name="audio_latency"></param>
/// <param name="video_latency"></param>
/// <param name="audio_bitrate"></param>
/// <param name="video_bitrate"></param>
/// <param name="bandwidth"></param>
/// <param name="packets_lost"></param>
void			qoe_update_quality					(SessionCore* core,
													 gint time,
													 gint framerate,
													 gint audio_latency,
													 gint video_latency,
													 gint audio_bitrate,
													 gint video_bitrate,
												     gint bandwidth,
													 gint packets_lost);


gint			qoe_get_audio_bitrate				(QoE* qoe);

gint			qoe_get_video_bitrate				(QoE* qoe);

Codec			qoe_get_audio_codec					(QoE* qoe);

Codec			qoe_get_video_codec					(QoE* qoe);

QoEMode			qoe_get_mode						(QoE* qoe);