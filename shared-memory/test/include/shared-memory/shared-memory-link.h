#ifndef  __SHARED_MEMORY_LINK_H__
#define  __SHARED_MEMORY_LINK_H__
#include "frame-work.h"
#include "object.h"
#include "shared-memory-hub.h"



G_BEGIN_DECLS



#define SHARED_MEMORY_TYPE_LINK shared_memory_link_get_type()
G_DECLARE_DERIVABLE_TYPE(SharedMemoryLink, shared_memory_link, SHARED_MEMORY, LINK, GObject)

struct _SharedMemoryLinkClass
{
	GObject parent_class;

	void
	(*push_message)(SharedMemoryLink* link,
	SharedMemoryOpcode opcode, 
	gpointer data, 
	gint size);
};




/*NECESSARY FUNCTION FOR APPLICATION DEVELOPMENT*/

SharedMemoryLink*
shared_memory_link_new(SharedMemoryHub* hub, HANDLE memory_handle);

/*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*/

ThreadPool*
shared_memory_link_get_thread_pool(SharedMemoryLink* self);

MemoryBlock*
shared_memory_link_get_memory_block(SharedMemoryLink * self);

SharedMemoryHub*
shared_memory_link_get_owned_hub(SharedMemoryLink* self);

void
shared_memory_link_set_owned_hub(SharedMemoryLink* link, SharedMemoryHub* hub)

G_END_DECLS
#endif 
