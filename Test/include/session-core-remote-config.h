#include "session-core-type.h"
#include "session-core.h"
#include <gst/gst.h>


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
													 gint framerate,
													 gint latency,
													 gint audio_bitrate,
													 gint video_bitrate);


gint			qoe_get_audio_bitrate				(QoE* qoe);

gint			qoe_get_video_bitrate				(QoE* qoe);