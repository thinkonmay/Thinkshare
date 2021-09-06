/// <summary>
/// @file session-core-remote-config.c
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-06
/// 
/// @copyright Copyright (c) 2021
/// 
#include <session-core-remote-config.h>
#include <session-core-type.h>
#include <session-core-pipeline.h>

#include <opcode.h>
#include <qoe.h>

#include <gst\gst.h>
#include <glib-2.0\glib.h>
#include <gst/controller/gstinterpolationcontrolsource.h>
#include <gst/controller/gstdirectcontrolbinding.h>


#include <stdio.h>
#include <windows.h>
#include <winuser.h>

#include <ultra-low-const.h>
#include <low-const.h>
#include <medium-const.h>
#include <high-const.h>
#include <very-high-const.h>
#include <ultra-high-const.h>



struct _QoE
{
	/*non volatile value, 
	*determine in session initialize time*/
	gint screen_height;
	gint screen_width;

	/*quality control mode*/
	QoEMode mode;

	/*codec audio*/
	Codec codec_audio;
	Codec codec_video;

	/// <summary>
	/// adaptive bitrate algorithm, this function should be called every time an quality sample is 
	/// reported, 
	/// all resources required by this algorithm should be self-declared (allocate memory and thread)
	/// </summary>
	ProcessBitrateCalculation algorithm;
};


QoE*
qoe_initialize()
{
	static QoE qoe;


	qoe.screen_width = DEFAULT_WIDTH;
	qoe.screen_height = DEFAULT_HEIGHT;
	qoe.codec_video = DEFAULT_VIDEO_CODEC;
	qoe.codec_audio = DEFAULT_AUDIO_CODEC;
	qoe.mode = DEFAULT_MODE;
	
	for(int i =0; i<SAMPLE_QUEUE_LENGTH + 1; i++)
	{
		qoe.sample[i].time = 0;
		qoe.sample[i].framerate = DEFAULT_RAMERATE;
		qoe.sample[i].video_bitrate = 2048;
		qoe.sample[i].audio_bitrate = 2048;
		qoe.sample[i].video_latency = 0;
		qoe.sample[i].audio_latency = 0;
		qoe.sample[i].available_bandwidth = 2048;
		qoe.sample[i].packets_lost = 0;
	}
	qoe.current_reported_sample = 0;
	qoe.current_sample = 0;
	return &qoe;
}



void
qoe_setup(QoE* qoe,
		  gint screen_width,
		  gint screen_height,
		  Codec audio_codec,
		  Codec video_codec,
		  QoEMode qoe_mode)
{
	DEVMODE devmode;
    devmode.dmPelsWidth = screen_width;
    devmode.dmPelsHeight = screen_height;
    devmode.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT;
    devmode.dmSize = sizeof(DEVMODE);

    long result = ChangeDisplaySettings(&devmode, 0);


	qoe->screen_width = screen_width;
	qoe->screen_height = screen_height;

	qoe->codec_audio = audio_codec;
	qoe->codec_video = video_codec;

	qoe->mode = qoe_mode;

	switch (qoe->mode)
	{
	case ULTRA_LOW_CONST:
		qoe->algorithm = ultra_low_const;
		break;
	case LOW_CONST:
		qoe->algorithm = low_const;
		break;
	case MEDIUM_CONST:
		qoe->algorithm = medium_const;
		break;
	case HIGH_CONST:
		qoe->algorithm = hight_const
		break;
	case VERY_HIGH_CONST:
		qoe->algorithm = very_hight_const;
		break;
	case ULTRA_HIGH_CONST:
		qoe->algorithm = ultra_hight_const;
		break;
	case SEGMENTED_ADAPTIVE_BITRATE:
		/* code */
		break;
	case NON_OVER_SAMPLING_ADAPTIVE_BITRATE:
		/* code */
		break;
	case OVER_SAMPLING_ADAPTIVE_BITRATE:
		/* code */
		break;
	case PREDICTIVE_ADAPTIVE_BITRATE:
		/* code */
		break;	
	default:
		break;
	}
}










void
qoe_update_quality(SessionCore* core,
					gint time,
					gint framerate,
					gint audio_latency,
					gint video_latency,
					gint audio_bitrate,
					gint video_bitrate,
					gint bandwidth,
					gint packets_lost)
{
	// implement circular buffer of quality sample
	QoE* qoe = session_core_get_qoe(core);
	QualitySample sample;

	sample.available_bandwidth = bandwidth;
	sample.packets_lost = packets_lost;
	sample.framerate = framerate;
	sample.time = time;
	sample.video_latency = video_latency;
	sample.audio_latency = audio_latency;
	sample.audio_bitrate = audio_bitrate;
	sample.video_bitrate = video_bitrate;
	qoe->algorithm(core, sample);
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
