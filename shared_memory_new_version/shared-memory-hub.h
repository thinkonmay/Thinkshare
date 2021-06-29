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

	gboolean
	(*send_data)(SharedMemoryHub* self,
		gint destination_hub, 
		gpointer data);

};




void
shared_memory_hub_update_id_array(SharedMemoryHub* self,
	gpointer data);

SharedMemoryHub*
shared_memory_hub_initialize(gint argc, char* argv[]);

gboolean
shared_memory_hub_send_message(SharedMemoryHub* self,
	gint peer_id,
	gpointer data,
	gint data_size);

void
shared_memory_hub_add_link(SharedMemoryHub* self,
	SharedMemoryLink* link);

void
shared_memory_hub_terminate_link(SharedMemoryHub* self,
	gint peer_id);

SharedMemoryLink* 
shared_memory_hub_get_link_by_id(SharedMemoryHub* self,
	gint peer_id);

G_END_DECLS
#endif   
