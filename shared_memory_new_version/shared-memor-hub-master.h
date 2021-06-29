#pragma once
#ifndef  __SHARED_MEMORY_HUB_MASTER_H__
#define  __SHARED_MEMORY_HUB_MASTER_H__
#include "frame-work.h"
#include "object.h"
#include "shared-memory-link.h"
#include "shared-memory-hub.h"
#include "shared-memory-thread-pool.h"


G_BEGIN_DECLS








#define SHARED_MEMORY_TYPE_HUB_MASTER shared_memory_hub_master_get_type()
G_DECLARE_DERIVABLE_TYPE (SharedMemoryHubMaster, shared_memory_hub_master, SHARED_MEMORY, HUB_MASTER, SharedMemoryHub)



struct _SharedMemoryHubMasterClass
{
	SharedMemoryHubClass* parent_class;

    gint
    (*call_slave_process)(SharedMemoryHubMaster* self, gchar* app);

    void
    (*terminate_slave_process)(SharedMemoryHubMaster* self, gint id);

    gboolean
    (*establish_peer_link)(SharedMemoryHubMaster* self, gint id1 , gint id2);
};


/*NECESSARY FUNCTION*/
SharedMemoryHubMaster*
shared_memory_hub_master_new(void);


gint
shared_memory_hub_master_create_new_slave(SharedMemoryHubMaster* self,
    gchar* app);

gboolean
shared_memory_hub_master_create_link(SharedMemoryHubMaster* self,
    gint id1,
    gint id2);

gboolean
shared_memory_hub_master_terminate_slave(SharedMemoryHubMaster* self,
    gint slave_id);

void
shared_memory_link_send_message(SharedMemoryLink* self,
    SharedMemoryOpcode opcode,
    gpointer data_pointer,
    gint size)

G_END_DECLS 
#endif   
