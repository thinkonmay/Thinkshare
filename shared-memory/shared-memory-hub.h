
#pragma once

#ifndef  __SHARED_MEMORY_HUB_H__
#define  __SHARED_MEMORY_HUB_H__
#include "frame-work.h"
#include "shared-memory-link.h"
#include "object.h"


G_BEGIN_DECLS








#define SHARED_MEMORY_TYPE_HUB shared_memory_hub_get_type()
G_DECLARE_DERIVABLE_TYPE (SharedMemoryHub, shared_memory_hub, SHARED_MEMORY, HUB, GObject)



struct _SharedMemoryHubClass
{
	GObjectClass parent_class;

	NamedPipe*
	(*atomic_create_pipe)(gint id, gsize pipe_size);

	MemoryBlock*
	(*atomic_create_block)(gint id, gsize block_size);
};




/*NECESSARY FUNCTION*/
SharedMemoryHub*
shared_memory_hub_new_default(gint hub_id);

SharedMemoryHub*
shared_memory_hub_new(gint hub_id,
	gint max_link,
	gboolean is_master);




/*USED BY SHARED-MEMORY-LINK FILE*/
MemoryBlock*
new_empty_block( gint id,gsize block_size);


/*Tôi cần tuấn và hiệp support tôi hoàn thiện method này
* Vai trò của method là tạo ra một cái namedpipe có name và id ngẫu nhiên
* Tuy nhiên vấn đề cần phải giải là làm sao 1 memory block có thể xác định hoàn toàn tên 
* thông qua số id này
*/
NamedPipe*
new_empty_pipe( gint id,gsize buffer_size);



void
shared_memory_hub_link_default_async(SharedMemoryHub* self,
	gint peer_hub_id,
	gint block_size,
	gint pipe_size,
	GCancellable* cancellable,
	GAsyncReadyCallback callback,
	gpointer user_data);

SharedMemoryLink*
shared_memory_hub_link_finish(SharedMemoryHub* self,
	GAsyncResult* result,
	GError** error);

gboolean
shared_memory_hub_perform_peer_transfer_request(SharedMemoryHub* hub,
	MemoryBlock* msg,
	gint destination);


G_END_DECLS
#endif   
