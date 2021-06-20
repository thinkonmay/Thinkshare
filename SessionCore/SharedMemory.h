#include "Framework.h"
#include "Object.h"
#include "session-core.h"



void
link_shared_memory_hub(GObject* object);

gboolean
send_message_through_shared_memory(SessionCore* self,
	Location location,
	Opcode opcode,
	gpointer data);