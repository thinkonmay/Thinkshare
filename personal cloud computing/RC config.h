#include "Framework.h"
#include "Variable.h"

extern GstControlSource* audio_controller;
extern GstControlSource* video_controller;

gdouble*
process_bitrate_calculation(gint compose_bitrate);

void
attach_bitrate_control(GstObject* encoder, GstControlSource* controller);


void
set_dynamic_bitrate(gint compose_bitrate);

