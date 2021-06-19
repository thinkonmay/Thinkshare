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
	(*connect_shared_memory_hub)(SessionCore* self);

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
};


SessionCore*
session_core_new(gint id);

void
session_core_end(const gchar* msg, 
	SessionCore* core, 
	CoreState state);

 



gboolean
session_core_connect_shared_memory_hub(SessionCore* self);

gboolean
session_core_setup_pipeline(SessionCore* self);

gboolean
session_core_setup_data_channel(SessionCore* self);

gboolean
session_core_setup_webrtc_signalling(SessionCore* self);

gboolean
session_core_start_pipeline(SessionCore* self);

gboolean
session_core_stop_pipeline(SessionCore* self);

gboolean
session_core_connect_signalling_server(SessionCore* self);





void
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