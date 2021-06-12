


#ifndef  __SHARED_MEMORY_HUB_H__
#define  __SHARED_MEMORY_HUB_H__
#include "frame-work.h"
#include "shared-memory-link.h"


G_BEGIN_DECLS

#define SHARED_MEMORY_TYPE_HUB shared_memory_hub_get_type()
G_DECLARE_DERIVABLE_TYPE (SharedMemoryHub, shared_memory_hub, SHARED_MEMORY, HUB, GObject)

struct _SharedMemoryHubClass
{
	GObject parent_class;

	gboolean
	(*atomic_create_pipe)(SharedMemoryHub* self);

	gboolean
	(*atomic_create_block)(SharedMemoryHub* self);
};




/*NECESSARY FUNCTION*/
SharedMemoryHub*
shared_memory_hub_new_default(gint hub_id);

void
shared_memory_hub_link_with_option_async(SharedMemoryHub* self,
	MemoryBlock* mem_block,
	Pipe* send_pipe,
	Pipe* receive_pipe,
	gboolean is_master,
	gint destination_id,
	GCancellable *cancellable,
	GAsyncReadyCallback	callback,
	gpointer user_data);

SharedMemoryLink*
shared_memory_hub_link_finish(SharedMemoryHub* self,
	GAsyncResult* result,
	GError **error);











/*USED BY SHARED-MEMORY-LINK FILE*/
MemoryBlock*
new_empty_block(gsize block_size);

Pipe*
new_empty_pipe(gsize buffer_size);

void
shared_memory_hub_link_default_async(SharedMemoryHub* self,
	gint peer_hub_id,
	GCancellable* cancellable,
	GAsyncReadyCallback callback,
	gpointer user_data);

void
shared_memory_hub_perform_peer_transfer_request(SharedMemoryHub* hub,
	SharedMemoryLink* link,
	PacketLite* pkg,
	gint destination);




G_END_DECLS
#endif   __SHARED_MEMORY_HUB_H__
