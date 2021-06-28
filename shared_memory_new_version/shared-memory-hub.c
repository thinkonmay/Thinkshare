#pragma once
#include "shared-memory-hub.h"

/// <summary>
/// private struct 
/// </summary>
typedef struct
{
	SharedMemoryLink* link_array[MAX_LINK_HUB];

	gint hub_id_array[MAX_LINK_MASTER];

	gint id;
}SharedMemoryHubPrivate;

enum
{
	SIGNAL_ON_MESSAGE,

	SIGNAL_LAST
};

enum
{
	PROP_HUB_ID,

	PROP_LAST
};


static guint signals[SIGNAL_LAST] = { 0, };
static guint properties[PROP_LAST] = { 0, };



/// <summary>
/// function declaration stufff
/// </summary>
/// <param name="object"></param>
static void
shared_memory_hub_constructed(GObject* object);
static void
shared_memory_hub_get_property(GObject* object, 
	guint prop_id,
	const GValue* value, 
	GParamSpec* pspec);
static void
shared_memory_hub_set_property(GObject* object,
	guint prop_id,
	const GValue* value, 
	GParamSpec* pspec);
static void
shared_memory_hub_dispose(GObject * object);
static void
shared_memory_hub_finalize(GObject * object);

G_DEFINE_TYPE_WITH_PRIVATE(SharedMemoryHub,shared_memory_hub,G_TYPE_OBJECT)



/// <summary>
/// create new simple shared memory hub with hub id, maximum number of link
/// and authority of hub
/// </summary>
/// <param name="hub_id"></param>
/// <param name="max_link"></param>
/// <param name="is_master"></param>
/// <returns></returns>
SharedMemoryHub* 
shared_memory_hub_new(gint hub_id)
{
	return g_object_new(SHARED_MEMORY_TYPE_HUB,"hub-id", hub_id, NULL);
}



/// <summary>
/// class initialization:
/// override base gobject class method,
/// declare signal and properties
/// </summary>
/// <param name="klass"></param>
static void
shared_memory_hub_class_init(SharedMemoryHubClass* klass)
{
	GObjectClass* object_class = G_OBJECT_GET_CLASS(klass);

	object_class->constructed =  shared_memory_hub_constructed;
	object_class->get_property = shared_memory_hub_get_property;
	object_class->dispose =      shared_memory_hub_dispose;
	object_class->finalize =     shared_memory_hub_finalize;

	klass->send_data = shared_memory_hub_send_message;

	properties[PROP_HUB_ID] =
		g_param_spec_int("hub-id",
			"hubid",
			"hub id",
			0, G_MAXINT, 0, G_PARAM_READWRITE);

	signals[SIGNAL_ON_MESSAGE] = 
		g_signal_new("on-message", SHARED_MEMORY_TYPE_HUB, 
		G_SIGNAL_RUN_FIRST , 0 ,NULL,NULL,NULL, G_TYPE_NONE, 2,
		G_TYPE_INT, G_TYPE_POINTER );

	g_object_class_install_properties(object_class, PROP_LAST, properties);

}

/// <summary>
/// constructed method used by base class,
///  process after initialization process is done
/// </summary>
/// <param name="object"></param>
static void
shared_memory_hub_constructed(GObject* object)
{
	SharedMemoryHub* self = (SharedMemoryHub*)object;
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);

	memset(priv->link_array, 0,  sizeof(SharedMemoryLink*) * MAX_LINK_HUB);

	memset(priv->hub_id_array, 0,  sizeof(int) * MAX_LINK_HUB);

}





/// <summary>
/// set properties of object
/// </summary>
/// <param name="object"></param>
/// <param name="prop_id"></param>
/// <param name="value"></param>
/// <param name="pspec"></param>

static void
shared_memory_hub_set_property(GObject* object, 
	guint prop_id,
	const GValue *value, 
	GParamSpec* pspec)
{
	SharedMemoryHub* self = (SharedMemoryHub*)object;
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);

	switch (prop_id)
	{
	case PROP_HUB_ID:
		priv->id = g_value_get_int(value);
		break;
	}
}

static void
shared_memory_hub_get_property(GObject* object,
	guint prop_id,
	const GValue* value,
	GParamSpec* pspec)
{
	SharedMemoryHub* self = SHARED_MEMORY_HUB(object);
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);

	switch (prop_id)
	{
	case PROP_HUB_ID:
		g_value_set_int(value, priv->id);
		break;
	}
}

static void
shared_memory_hub_dispose(GObject* object)
{

	SharedMemoryHub* self = (SharedMemoryHub*)object;
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);

	for (int i = 0; i < MAX_LINK_HUB; i++)
	{
		g_free(priv->link_array[i]);
	}
}

static void
shared_memory_hub_finalize(GObject* object)
{

	SharedMemoryHub* self = (SharedMemoryHub*)object;
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);

	g_free(object);
}





SharedMemoryHub*
shared_memory_hub_initialize(gint argc, char* argv[])
{
	SharedMemoryHub* hub = g_object_new(SHARED_MEMORY_TYPE_HUB,NULL);
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(hub);


	if(argc < 2 )
		return NULL;
	
	HANDLE memory_handle = (HANDLE) argv[1];

	MemoryBlock* block = MapViewOfFile(memory_handle,
	FILE_MAP_ALL_ACCESS,0,0,0 );

 	/*get hub id in shared memory*/
	if(block->peer_id1 == MASTER_ID)
		priv->id = block->peer_id2;
	else	
		priv->id = block->peer_id1;
	


	shared_memory_link_new(hub,memory_handle);

	return hub;
}

///send data to a specific hub, based on shared_memory_link_send_message, establish new peer link if no 
gboolean
shared_memory_hub_send_message(SharedMemoryHub* self,
gint peer_id,
gpointer data,
gint data_size)
{
	gboolean check = FALSE;
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);
	while (TRUE)
	{
		for(int i = 0; i < MAX_LINK_HUB; i++)
		{
			gint temp;
			g_object_get_property(priv->link_array[i],"peer-id",&temp);
			if(temp = peer_id)
			{
				shared_memory_link_send_message(priv->link_array[i],MESSAGE,data,data_size);
				return TRUE;
			}
		}

		if(check = TRUE) {goto end;}

		/*if peer id found in hub_id_array, send link request*/
		for(int i = 0; i < MAX_LINK_MASTER; i++)
		{
			if (priv->hub_id_array == peer_id)
			{
				shared_memory_link_send_message(priv->link_array[0],PEER_LINK_REQUEST,&peer_id,sizeof(gint));
				check = TRUE;
				goto end;			
			}
		}
		g_printerr("peer-id not found");
		return FALSE;
		end:
	}


}


void
shared_memory_hub_add_link(SharedMemoryHub* self,
	SharedMemoryLink* link)
{

	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);
	for(int i = 0; i< MAX_LINK_HUB;i++)
	{
		if (priv->link_array[i]==NULL)
		{
			priv->link_array[i]= link;
			shared_memory_link_set_owned_hub(self, link);
		}

	}
}

void
shared_memory_hub_terminate_link(SharedMemoryHub* self,
	gint peer_id)
{
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);

	if(peer_id == 0)
	{
		g_printter("trying to terminate master link");
		return;
	}
	for (int i = 0; i< MAX_LINK_HUB;i++)
	{
		gint temp;
		g_object_get_property(priv->link_array[i], "peer-id",&temp );

		if(temp == peer_id)
		{
			g_unref(priv->link_array[i]);
			priv->link_array[i]=NULL;
		}
		g_free(&temp);
	}
}


void
shared_memory_hub_update_id_array(SharedMemoryHub* self,
	gpointer data)
	{
		SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);
		

		for(gint  i = 0; i< MAX_LINK_MASTER ; i++)
		{
			priv->hub_id_array[i] = (gint*)data + i;
		}
	}


SharedMemoryLink*
shared_memory_hub_get_link_by_id(SharedMemoryHub* self, gint peer_id)
{
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);
	for (gint i = 0; i < MAX_LINK_HUB; i++)
	{
		gint id;
		g_object_get_property(priv->link_array[i],"peer-id",&id);
		if (id = peer_id)
		{
			return priv->link_array[i];	
		}
	}

	return NULL;
}






