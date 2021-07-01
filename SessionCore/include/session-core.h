#ifndef __SESSION_CORE_H__
#define __SESSION_CORE_H__

#include "Framework.h"
#include "Object.h"

G_BEGIN_DECLS

#define SESSION_TYPE_CORE session_core_get_type()
 G_DECLARE_DERIVABLE_TYPE (SessionCore, session_core, SESSION, CORE, GObject)


struct _SessionCoreClass
{
	GObject parent_class;

	gboolean
	(*setup_pipeline)(SessionCore* self);

	gboolean
	(*setup_data_channel)(SessionCore* self);

	gboolean  
	(*setup_webrtc_signalling)(SessionCore* self);

	gboolean
	(*start_pipeline)(SessionCore* self);

	gboolean
	(*stop_pipeline)(SessionCore* self);

	gboolean
	(*connect_signalling_server)(SessionCore* self);
	
	gboolean
	(*send_message)(SessionCore* self, Message* message)
};


SessionCore*
session_core_initialize(gint argc, gchar* argv[]);

void
session_core_end(const gchar* msg, 
	SessionCore* core, 
	CoreState state);

gboolean
session_core_send_message(GObject* object,
	Message* msg);








Pipeline*
session_core_get_pipeline(SessionCore* self);

WebRTCHub*
session_core_get_rtc_hub(SessionCore* self);

SessionQoE*
session_core_get_qoe(SessionCore* self);

IPC*
session_core_get_ipc(SessionCore* self);





G_END_DECLS
#endif 