#include "session-core-type.h"
#include "session-core.h"
#include <gst/gst.h>
#

void			attach_bitrate_control				(GstObject* audio_encoder,
													 GstObject* video_encoder,
													 SessionCore* core);

void			set_dynamic_bitrate					(Pipeline* pipe, 
												     SessionQoE* qoe);

QoE*			qoe_initialize						();

void			qoe_setup							(QoE* qoe,
													 gint screen_width,
													 gint screen_height,
													 gint framerate,
													 gint bitrate);

gint			qoe_get_screen_width				(QoE* qoe);

gint			qoe_get_screen_height				(QoE* qoe);

gint			qoe_get_framerate					(QoE* qoe);

void			qoe_update_quality					(QoE* qoe,
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