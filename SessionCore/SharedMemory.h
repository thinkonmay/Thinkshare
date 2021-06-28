#include "Framework.h"
#include "Object.h"
#include "session-core.h"


void
on_link_connected(GObject* object,
	GAsyncResult* res,
	gpointer user_data);

void
link_shared_memory_hub(GObject* object);

gboolean
send_message_through_shared_memory(SessionCore* self,
	Message* message);