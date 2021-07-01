#include <shared-memory-link.h>
#include <shared-memory-hub-master.h>
#include <shared-memory-thread-pool.h>


/// <summary>
/// private struct
/// </summary>


typedef struct
{
	ThreadPool* pool;

	SharedMemoryHub* owned_hub;
	gint peer_id;

	MemoryBlock* block;
}SharedMemoryLinkPrivate;


enum
{
	PROP_PEER_ID,
	PROP_OWNED_HUB_ID,

	PROP_LAST
};
static GParamSpec* properties[PROP_LAST] = {NULL, };

static void
shared_memory_link_get_property(GObject* object,
	guint prop_id,
	GValue* value,
	GParamSpec* pspec);

static void
shared_memory_link_dispose(GObject * object);

static void
shared_memory_link_finalize(GObject * object);

void
shared_memory_link_send_large_data(SharedMemoryLink* self,
	gpointer data_pointer,
	gint size);

void
shared_memory_link_send_message(SharedMemoryLink* self,
	SharedMemoryOpcode opcode,
	gpointer data_pointer,
	gint size);

/*define gtype (glib standard)*/
G_DEFINE_TYPE_WITH_PRIVATE(SharedMemoryLink, shared_memory_link, G_TYPE_OBJECT)






/// <summary>
/// class initialization: override gobject base class method
/// </summary>
/// <param name="klass"></param>
static void
shared_memory_link_class_init(SharedMemoryLinkClass* klass)
{
	/*get object class from shared memory class*/
	GObjectClass* object_class = G_OBJECT_CLASS(klass);

	/*override gobject base class method*/
	object_class->get_property = shared_memory_link_get_property;
	object_class->dispose = shared_memory_link_dispose;
	object_class->finalize = shared_memory_link_finalize;

	/*assign virtual class method by assigning to corresponding function*/
	klass->push_message = shared_memory_link_send_message;

	//register object properties
	properties[PROP_PEER_ID] =
		g_param_spec_int("peer-id",
			"PeerID",
			"peer id",
			0, G_MAXINT, 0, G_PARAM_READABLE);

	/*id of the hub that own this link*/
	properties[PROP_OWNED_HUB_ID] =
		g_param_spec_int("hub-id",
			"hubid",
			"hub id",
			0, G_MAXINT, 0, G_PARAM_READABLE);

	HANDLE handle;
}


/// <summary>
/// get property method of base class: get properties from object
/// </summary>
/// <param name="object"></param>
static void
shared_memory_link_get_property(GObject* object,
	guint prop_id,
	GValue* value,
	GParamSpec* pspec)
{
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private((SharedMemoryLink*)object);
	switch (prop_id)
	{
	case PROP_PEER_ID:
		g_value_set_int(value, priv->peer_id);
		break;
	case PROP_OWNED_HUB_ID:
		{
			gint id;
			g_object_get_property((GObject*)priv->owned_hub, "hub-id", &id);
			g_value_set_int(value, id);
			g_free(&id);
		}

	}
}

/// <summary>
/// dispose method of gobject base class: run before object termination process is done,
/// free all member of private struct
/// </summary>
/// <param name="object"></param>
static void
shared_memory_link_dispose(GObject* object)
{
	SharedMemoryLink* self = (SharedMemoryLink*)object;
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);

	for(gint i = 0; i< THREAD_PER_LINK; i++)
	{	
		g_thread_unref(priv->block->segment[i].send);
		g_thread_unref(priv->block->segment[i].receive);
	}	

	g_object_unref(priv->pool->send_queue);
	g_object_unref(priv->pool->receive_queue);

	g_thread_unref(priv->pool->input_handling_thread);

	

	CloseHandle(priv->block->handle);
}


/// <summary>
/// last process of object termination
/// </summary>
/// <param name="object"></param>
static void
shared_memory_link_finalize(GObject* object)
{
	g_free(object);
}

static void 
shared_memory_link_init(SharedMemoryLink* object)
{
	return;
}

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




/// <summary>
/// Create new link with a given hub and handle to the memoryblock.
/// link will be added to sharedmemory array after creation
/// </summary>
/// <param name="self">sharedmemory hub that own this link</param>
/// <param name="memory_handle">handle to the memory block</param>
/// <returns></returns>
SharedMemoryLink* 
shared_memory_link_new(SharedMemoryHub* self, HANDLE memory_handle)
{
	SharedMemoryLink* link = g_object_new(SHARED_MEMORY_TYPE_LINK,NULL);
	SharedMemoryLinkPrivate * priv = shared_memory_link_get_instance_private(link);

	MemoryBlock* block =  MapViewOfFile (memory_handle, 
        FILE_MAP_ALL_ACCESS,0,0,0);


	priv->block = block;
	priv->owned_hub = self;

	gint id;
	g_object_get_property((GObject*)priv->owned_hub,"hub-id",&id);
	
	if(block->peer_id1 == id)
		{priv->peer_id = block->peer_id2;}
	else
		{priv->peer_id = block->peer_id1;}

	priv->pool = thread_pool_new(self);

	shared_memory_hub_add_link(self, link);
	return link;	
}


/// <summary>
/// send message with opcode to peer hub, if size of the data package is larger than 
/// the size of memory segment, the package will be broke into smaller package
/// </summary>
/// <param name="self">the link that send the message</param>
/// <param name="opcode">opcode of the message</param>
/// <param name="data_pointer">pointer to data that should be send</param>
/// <param name="size">size of the data package</param>
void
shared_memory_link_send_message(SharedMemoryLink* self, 
	SharedMemoryOpcode opcode,
	gpointer data_pointer,
	gint size)   
{
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);


	if (size > priv->block->segment[0].size)
	{
		shared_memory_link_send_large_data(self, data_pointer, size);
	}

	thread_pool_push_message(priv->pool, opcode, data_pointer, size);
}





/// <summary>
/// if size of the message is too bug, shared_memory_link_send_large_data will be 
/// invoke to send smaller message that have the size of a memory segment
/// </summary>
/// <param name="self">shared memory link that own the link</param>
/// <param name="opcode"></param>
/// <param name="data_pointer">pointer to the data</param>
/// <param name="size">sizeof the data chunk</param>
void
shared_memory_link_send_large_data(SharedMemoryLink* self,
gpointer data_pointer,
gint size)   
{
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);

	gint unread_data = size;


	while(unread_data > priv->block->segment[0].size)
	{
		thread_pool_push_message(priv->pool,
		MESSAGE_HUGE,(gchar*) data_pointer + (size - unread_data),
		priv->block->segment[0].size);
	}

	thread_pool_push_message(priv->pool, MESSAGE_HUGE,
	(gchar*) data_pointer + (size - unread_data),
	unread_data);
}







/*
*getter function to acquires private member of object
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


/// <summary>
/// get thread pool from shared memory link
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
ThreadPool*
shared_memory_link_get_thread_pool(SharedMemoryLink* self)
{
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);

	return priv->pool;
}



/// <summary>
/// get the hub that own the link
/// </summary>
/// <param name="link"></param>
/// <returns></returns>
SharedMemoryHub*
shared_memory_link_get_owned_hub(SharedMemoryLink* link)
{
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(link);
	return priv->owned_hub;
}

void
shared_memory_link_set_owned_hub(SharedMemoryLink* link, SharedMemoryHub* hub)
{
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(link);
	priv->owned_hub = hub;
	return;
}


/// <summary>
/// get pointer to the memory block 
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
MemoryBlock*
shared_memory_link_get_memory_block(SharedMemoryLink * self)
{
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);
	return priv->block;
}
