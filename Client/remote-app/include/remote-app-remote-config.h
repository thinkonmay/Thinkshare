/// <summary>
/// @file remote-app-remote-config.h
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-05
/// 
/// @copyright Copyright (c) 2021
/// 
#include <remote-app-type.h>
#include "remote-app.h"
#include <gst/gst.h>
#include <qoe.h>





/// <summary>
/// initialize qoe metric measurement
/// </summary> 
/// <returns></returns>
QoE*			qoe_initialize						();

/// <summary>
/// setup qoe metric measurement
/// </summary> 
/// <param name="qoe"></param>
/// <param name="screen_width"></param>
/// <param name="screen_height"></param>
/// <param name="audio_codec"></param>
/// <param name="video_codec"></param>
/// <param name="qoe_mode"></param>
void			qoe_setup							(QoE* qoe,
		  											gint screen_width,
		  											gint screen_height,
		  											Codec audio_codec,
		  											Codec video_codec,
		  											QoEMode qoe_mode);


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
void			qoe_update_quality					(RemoteApp* core,
													 gint time,
													 gint framerate,
													 gint audio_latency,
													 gint video_latency,
													 gint audio_bitrate,
													 gint video_bitrate,
												     gint bandwidth,
													 gint packets_lost);

/// <summary>
/// get audio codec from current remote session
/// </summary>
/// <param name="qoe"></param>
/// <returns></returns>
Codec			qoe_get_audio_codec					(QoE* qoe);

/// <summary>
/// get video codec from current remote session
/// </summary>
/// <param name="qoe"></param>
/// <returns></returns>
Codec			qoe_get_video_codec					(QoE* qoe);