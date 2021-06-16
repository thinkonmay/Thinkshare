#include "Framework.h"
#include "Session.h"
#include "session-core.h"


gboolean
check_plugins();

void
connect_WebRTCHub_handler(SessionCore* core);


void
session_core_setup_pipeline(SessionCore* core,
	gpointer user_data);

gboolean
session_core_start_pipeline(SessionCore* core)
