#ifndef __SESSION_CORE__
#define __SESSION_CORE__

#include "Framework.h"
#include "Session.h"
#include "Session.h"

G_BEGIN_DECLS

#define SESSION_TYPE_CORE session_core_get_type()
 G_DECLARE_FINAL_TYPE (SessionCore, session_core, SESSION, CORE, GObject)


SessionCore*
session_core_new(gint id);

void
session_core_end(const gchar* msg, 
	SessionCore* core, 
	CoreState state);

void
session_core_link_shared_memory_hub(GObject* object);

struct _SessionCoreClass
{
	GObject parent_class;
};



Pipeline*
session_core_get_pipeline(SessionCore* self);

WebRTCHub*
session_core_get_rtc_hub(SessionCore* self);

SessionQoE*
session_core_get_qoe(SessionCore* self);

G_END_DECLS
#endif __SESSION_CORE__