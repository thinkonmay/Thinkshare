#include "frame-work.h"
#include "shared-memory-hub.h"
#include "shared-memory-link.h"


typedef struct
{
	SharedMemoryLink** link_array;
	gint max_link;

	gint hub_id;

	gboolean is_master;
}SharedMemoryHubPrivate;

enum
{
	SIGNAL_LINKED,
	SIGNAL_PEER_TRANSFER,

	SIGNAL_LAST
};

enum
{
	PROP_ID,
	PROP_MAX_LINK,
	PROP_HUB_ID,

	PROP_LAST
};

static guint signals[SIGNAL_LAST] = { 0, };
static guint properties[PROP_LAST] = { 0, };

G_DEFINE_TYPE_WITH_PRIVATE(SharedMemoryHub,shared_memory_hub,G_TYPE_OBJECT)

SharedMemoryHub* 
shared_memory_hub_new(gint hub_id,
	gint max_link,
	gboolean is_master)
{
	SharedMemoryHub* self = g_object_new(SHARED_MEMORY_TYPE_HUB, 
		"hub-id", hub_id,
		"max-link", max_link,
		"is-master", is_master);

	return self;
}

SharedMemoryHub* 
shared_memory_hub_new_default(gint hub_id)
{
	SharedMemoryHub* self = g_object_new(SHARED_MEMORY_TYPE_HUB, NULL);
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);
	
	priv->hub_id = hub_id;
	priv->is_master = FALSE;
	return self;
}

void 
shared_memory_hub_link_with_option_async(SharedMemoryHub* self, 
	MemoryBlock* mem_block, 
	Pipe* send_pipe, 
	Pipe* receive_pipe, 
	gboolean is_master,
	gint destination_id,
	GCancellable* cancellable, 
	GAsyncReadyCallback callback, 
	gpointer user_data)
{
	g_object_new(SHARED_MEMORY_TYPE_HUB, "link-id");
	SharedMemoryHubClass* klass = SHARED_MEMORY_HUB_GET_CLASS(self);
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);

	SharedMemoryLink* link = g_object_new(SHARED_MEMORY_TYPE_LINK, NULL);

	InitData *init_data;
	init_data->mem_block = mem_block;
	init_data->send_pipe = send_pipe;
	init_data->receive_pipe = receive_pipe;
	init_data->is_master = is_master;
	init_data->destination_id = destination_id;


	GTask *task= g_task_new(self,cancellable,callback,user_data);
	g_task_set_task_data(task, init_data, (GDestroyNotify)init_data_free);

	g_task_run_in_thread(task, establish_link);
	g_object_unref(task);
}

static void
shared_memory_hub_class_init(SharedMemoryHubClass* klass)
{
	GObjectClass* object_class = G_OBJECT_GET_CLASS(klass);

	object_class->constructed =  shared_memory_hub_constructed;
	object_class->get_property = shared_memory_hub_get_property;
	object_class->dispose =      shared_memory_hub_dispose;
	object_class->finalize =     shared_memory_hub_finalize;

	klass->atomic_create_block = new_empty_block;
	klass->atomic_create_pipe = new_empty_pipe;

	signals[SIGNAL_LINKED] =
		g_signal_new("connected",
			SHARED_MEMORY_TYPE_HUB,
			G_SIGNAL_RUN_FIRST,
			0, NULL, NULL, NULL,
			G_TYPE_NONE, 0);
	signals[SIGNAL_PEER_TRANSFER] =
		g_signal_new("on-peer-transfer",
			SHARED_MEMORY_TYPE_HUB,
			G_SIGNAL_RUN_FIRST,
			0, NULL, NULL, NULL,
			G_TYPE_NONE, 0);


	properties[PROP_MAX_LINK] =
		g_param_spec_int("max-link",
			"MaxLink",
			"mximum number of link",
			0, G_MAXINT, 2, G_PARAM_READWRITE);
	properties[PROP_HUB_ID] =
		g_param_spec_int("hub-id",
			"hubid",
			"hub id",
			0, G_MAXINT, 0, G_PARAM_READWRITE);

}

static void
shared_memory_hub_constructed(GObject* object)
{
	SharedMemoryHub* self = (SharedMemoryHub*)object;
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);

	priv->link_array = malloc(priv->max_link * sizeof(gpointer));
	memset(priv->link_array, NULL, priv->max_link);
}

static void
shared_memory_hub_get_property(GObject* object)
{
	SharedMemoryHub* self = (SharedMemoryHub*)object;
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);
}

static void
shared_memory_hub_dispose(GObject* object)
{

	SharedMemoryHub* self = (SharedMemoryHub*)object;
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);

	for (int i; i < priv->max_link; i++)
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

void
shared_memory_hub_link_default_async(SharedMemoryHub* self,
	gint peer_hub_id,
	GCancellable* cancellable,
	GAsyncReadyCallback callback,
	gpointer user_data)
{
	g_object_new(SHARED_MEMORY_TYPE_HUB, NULL);
	SharedMemoryHubClass* klass = SHARED_MEMORY_HUB_GET_CLASS(self);
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);

	SharedMemoryLink* link = g_object_new(SHARED_MEMORY_TYPE_LINK, NULL);

	InitData* init_data;
	init_data->mem_block = klass->atomic_create_block(self);
	init_data->send_pipe = klass->atomic_create_pipe(self);
	init_data->receive_pipe = klass->atomic_create_pipe(self);
	init_data->is_master = priv->is_master;
	init_data->link_id = g_random_int();
	init_data->owned_hub = self;
	init_data->owned_hub_id = priv->hub_id;
	init_data->peer_hub_id = peer_hub_id;


	GTask* task = g_task_new(self, cancellable, callback, user_data);
	g_task_set_task_data(task, init_data, (GDestroyNotify)init_data_free);

	g_task_run_in_thread(task, establish_link);
	g_object_unref(task);
}


static void
init_data_free(InitData* data)
{
	g_free(data->mem_block);
	g_free(data->receive_pipe);
	g_free(data->send_pipe);
	g_free(data);
}

SharedMemoryLink* 
shared_memory_hub_link_finish(SharedMemoryHub* self, 
	GAsyncResult* result, 
	GError **error)
{

	g_return_val_if_fail(g_task_is_valid(result, self), NULL);

	g_signal_emit(self, signals[SIGNAL_LINKED], 0);

	return g_task_propagate_pointer(G_TASK(result),error);
}

MemoryBlock* 
new_empty_block(gsize block_size)
{
	MemoryBlock* ret = malloc(sizeof(MemoryBlock));
	ret->state = UN_OWN;
	ret->size = block_size;
	ret->id = g_random_int();
	ret->name = g_random_int();
	ret->handle = NULL;
	ret->pointer = NULL;
	ret->context = NULL;
	ret->position = 0;
	return ret;
}

Pipe* 
new_empty_pipe(gsize buffer_size)
{
	Pipe* ret = malloc(sizeof(Pipe));
	ret->size = buffer_size;
	ret->id = g_random_int();
	ret->name = g_random_int();
	ret->handle = NULL;
	ret->context = NULL;
	return ret;
}


gboolean
shared_memory_hub_perform_peer_transfer_request(SharedMemoryHub* hub,
	SharedMemoryLink* link,
	PacketLite* pkg,
	gint destination)
{
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(hub);
	SharedMemoryLinkClass* klass = SHARED_MEMORY_LINK_GET_CLASS(link);
	MemoryBlock* block = new_empty_block(pkg->data);

	g_signal_emit(hub, signals[],0)

	for (gint i = 0; i < priv->max_link; i++)
	{
		int z;
		g_object_get_property(priv->link_array[i], "peer-id", &z);
		if (z == destination)
		{
			g_free(z);
			if (!shared_memory_link_send_message_lite(link,
				PEER_TRANSFER_REQUEST,
				pkg->from,
				pkg->to,block))
			{
				return FALSE;
			}
			return TRUE;
		}
	}

	g_free(block);


}

