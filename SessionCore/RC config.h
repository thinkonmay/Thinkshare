#include "Framework.h"
#include "Session.h"



void
attach_bitrate_control(GstObject* encoder,
	GstControlSource* controller);

void
set_dynamic_bitrate(Pipeline* pipe, 
	SessionQoE* qoe);