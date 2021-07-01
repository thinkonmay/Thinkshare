#ifndef __SHARED_MEMORY_THREAD_POOL_H__
#define __SHARED_MEMORY_THREAD_POOL_H__
#include "frame-work.h"
#include "object.h"
#include "shared-memory-link.h"

/*foward declaration*/


ThreadPool* 
thread_pool_new(SharedMemoryLink* link);


gboolean 
shared_memory_create_thread(SharedMemoryLink* link , 
    MemorySegment* segment,
    GError** error);


void 
thread_pool_push_message(ThreadPool* pool,
SharedMemoryOpcode opcode,
gpointer data,
gint data_size);

#endif
