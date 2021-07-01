#include "Framework.h"
#include "Object.h"
#include "session-core.h"


gboolean
check_plugins();

gboolean
connect_WebRTCHub_handler(SessionCore* core);


gboolean
setup_pipeline(SessionCore* core,
	gpointer user_data);

gboolean
start_pipeline(SessionCore* core);

void
stop_pipeline(SessionCore* core);