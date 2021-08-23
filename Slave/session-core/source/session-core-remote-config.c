#include <session-core-remote-config.h>
#include <session-core-type.h>
#include <session-core-pipeline.h>

#include <opcode.h>
#include <qoe.h>

#include <gst\gst.h>
#include <glib-2.0\glib.h>
#include <gst/controller/gstinterpolationcontrolsource.h>
#include <gst/controller/gstdirectcontrolbinding.h>

#define DEFAULT_WIDTH				1920
#define DEFAULT_HEIGHT				1080
#define DEFAULT_RAMERATE			60
#define DEFAULT_AUDIO_CODEC			OPUS_ENC
#define DEFAULT_VIDEO_CODEC			CODEC_H264
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

	ProcessBitrateCalculation algorithm;
};


QoE*
qoe_initialize()
{
	static QoE qoe;


	qoe.screen_width = DEFAULT_WIDTH;
	qoe.screen_height = DEFAULT_HEIGHT;
	qoe.framerate = DEFAULT_RAMERATE;
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
		  gint framerate,
		  Codec audio_codec,
		  Codec video_codec,
		  QoEMode qoe_mode)
{
	qoe->framerate = framerate;
	qoe->screen_width = screen_width;
	qoe->screen_height = screen_height;
	qoe->codec_audio = audio_codec;
	qoe->codec_video = video_codec;
	qoe->mode = qoe_mode;
}


void
non_over_sampling(SessionCore* core)
{
	Pipeline* pipe = session_core_get_pipeline(core);
	QoE* qoe = session_core_get_qoe(core);


	GstElement* video_encoder = pipeline_get_video_encoder(pipe,qoe->codec_video);
	GstElement* audio_encoder = pipeline_get_audio_encoder(pipe,qoe->codec_audio);

	if(qoe->current_reported_sample == 0)
	{
		g_object_set_property(video_encoder,"bitrate",qoe->sample[1024].video_bitrate);
		g_object_set_property(audio_encoder,"bitrate",qoe->sample[1024].audio_bitrate);
	}

	g_object_set_property(video_encoder,"bitrate",
		(gint)(qoe->sample[qoe->current_reported_sample - 1].available_bandwidth * 0.7));
	g_object_set_property(audio_encoder,"bitrate",
		(gint)(qoe->sample[qoe->current_reported_sample - 1].available_bandwidth * 0.2));
}



void
attach_bitrate_control(SessionCore* core)
{
	QoE* qoe = session_core_get_qoe(core);
	Pipeline* pipeline = session_core_get_pipeline(core);

	switch(qoe->mode)
	{
		case DEFAULT_MODE:
		{
			qoe->algorithm = non_over_sampling;
		}
		case CUSTOM_BITRATE_CONTROL:
		{

		}
		case NON_OVERSAMPLING:
		{
			qoe->algorithm = non_over_sampling;
		}
	}


	while(TRUE)
	{
		qoe->algorithm(core);
		Sleep(1000);
	}
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
	return qoe->sample[qoe->current_reported_sample].framerate;
}

gint
qoe_get_video_bitrate(QoE* qoe)
{
	return qoe->sample[qoe->current_reported_sample].video_bitrate;
}

gint
qoe_get_audio_bitrate(QoE* qoe)
{
	return qoe->sample[qoe->current_reported_sample].audio_bitrate;
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