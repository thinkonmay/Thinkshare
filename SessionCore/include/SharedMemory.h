#include "Framework.h"
#include "Object.h"
#include "session-core.h"



gboolean
send_message_through_shared_memory(SessionCore* self,
	Message* message);

void
on_shared_memory_message(SharedMemoryLink* self,
	gint from,
	gpointer data,
	SessionCore* core);