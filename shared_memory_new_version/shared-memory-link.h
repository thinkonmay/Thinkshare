#if defined __cplusplus
extern "C"
{
#endif


#ifndef  __SHARED_MEMORY_LINK_H__
#define  __SHARED_MEMORY_LINK_H__
#include "frame-work.h"
#include "shared-memor-hub-master.h"
#include "shared-memory-thread-pool.h"
#include "object.h"

G_BEGIN_DECLS





#define SHARED_MEMORY_TYPE_LINK shared_memory_link_get_type()
G_DECLARE_DERIVABLE_TYPE(SharedMemoryLink, shared_memory_link, SHARED_MEMORY, LINK, GObject)

struct _SharedMemoryLinkClass
{
	GObject parent_class;

	void
	(*push_message)(SharedMemoryLink* link, gpointer data, gint size);
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

G_END_DECLS
#endif 


#if defined __cplusplus
}
#endif