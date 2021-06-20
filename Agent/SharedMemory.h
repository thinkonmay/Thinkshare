#pragma once
#include "Framework.h"
#include "Object.h"
#include "agent-object.h"


void
link_shared_memory_hub(GObject* object);

gboolean
send_message_through_shared_memory(AgentObject* self,
	Location location,
	Opcode opcode,
	gpointer data);