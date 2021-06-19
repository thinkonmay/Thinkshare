#if defined __cplusplus
extern "C"
{
#endif


#ifndef  __SHARED_MEMORY_LINK_H__
#define  __SHARED_MEMORY_LINK_H__
#include "frame-work.h"
#include "shared-memory-hub.h"
#include "object.h"

G_BEGIN_DECLS





#define SHARED_MEMORY_TYPE_LINK shared_memory_link_get_type()
G_DECLARE_DERIVABLE_TYPE(SharedMemoryLink, shared_memory_link, SHARED_MEMORY, LINK, GObject)

struct _SharedMemoryLinkClass
{
	GObject parent_class;

	gboolean
	(*atomic_mem_write)(SharedMemoryLink* self,
		MemoryBlock* block,
		gpointer data,
		gsize data_size);

	gpointer
	(*atomic_mem_read)(SharedMemoryLink* self,
		MemoryBlock* block);

	gboolean
	(*atomic_pipe_send)(SharedMemoryLink* self,
		gpointer* data,
		gsize data_size);

	gboolean
	(*establish_link)(GTask* task,
		gpointer data,
		gpointer user_data);
	gboolean
	(*send_link_signal)(SharedMemoryLink* self,
		gint opcode);
};

gboolean
establish_link(GTask* task,
	gpointer source_object,
	gpointer data,
	GCancellable* cancellable);







/*NECESSARY FUNCTION FOR APPLICATION DEVELOPMENT*/
gboolean
shared_memory_link_send_message(SharedMemoryLink* self,
	Message* msg);

gboolean
shared_memory_link_send_message_lite(SharedMemoryLink* self,
	Opcode opcode,
	gpointer data);


G_END_DECLS
#endif 


#if defined __cplusplus
}
#endif