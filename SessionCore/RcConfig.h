#include "Framework.h"
#include "Object.h"
#include "session-core.h"



void
attach_bitrate_control(GstObject* encoder,
	GstControlSource* controller);

void
set_dynamic_bitrate(Pipeline* pipe, 
	SessionQoE* qoe);