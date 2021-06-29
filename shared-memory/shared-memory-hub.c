#pragma once
#include "frame-work.h"
#include "shared-memory-hub.h"
#include "shared-memory-link.h"
#include "Object.h"

/// <summary>
/// private struct 
/// </summary>
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
	PROP_IS_MASTER,

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
static void
init_data_free(InitData* data);
static void
shared_memory_hub_init(GObject* object) {}


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
shared_memory_hub_new(gint hub_id,
	gint max_link,
	gboolean is_master)
{
	return g_object_new(SHARED_MEMORY_TYPE_HUB,
		"max-link", max_link,
		"hub-id", hub_id,
		"is-master", is_master, NULL);
}


/// <summary>
/// create default slave hub with hub id and default 8 maximum link
/// </summary>
/// <param name="hub_id"></param>
/// <returns></returns>
SharedMemoryHub* 
shared_memory_hub_new_default(gint hub_id)
{
	SharedMemoryHub* self = g_object_new(SHARED_MEMORY_TYPE_HUB, NULL);
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);
	
	priv->hub_id = hub_id;
	priv->is_master = FALSE;
	return self;
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
	object_class->set_property = shared_memory_hub_set_property;
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
			2, G_MAXINT, 2, G_PARAM_READWRITE);
	properties[PROP_HUB_ID] =
		g_param_spec_int("hub-id",
			"hubid",
			"hub id",
			0, G_MAXINT, 0, G_PARAM_READWRITE);
	properties[PROP_IS_MASTER] =
		g_param_spec_boolean("is-master",
			"is master",
			"ismaster", FALSE, G_PARAM_READWRITE);

	g_object_class_install_properties(object_class, PROP_LAST, properties);

}

/// <summary>
/// constructed method used by base class, process after initialization process is done
/// </summary>
/// <param name="object"></param>
static void
shared_memory_hub_constructed(GObject* object)
{
	SharedMemoryHub* self = (SharedMemoryHub*)object;
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);

	priv->link_array = malloc(priv->max_link * sizeof(gpointer));
	memset(priv->link_array, 123456, priv->max_link);
}

/// <summary>
/// set properties of object
/// </summary>
/// <param name="object"></param>
/// <param name="prop_id"></param>
/// <param name="value"></param>
/// <param name="pspec"></param>
static void
shared_memory_hub_set_property(GObject* object, guint prop_id,
 GValue *value, GParamSpec* pspec)
{
	SharedMemoryHub* self = (SharedMemoryHub*)object;
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);

	switch (prop_id)
	{
	case PROP_HUB_ID:
		priv->hub_id = g_value_get_int(value);
		break;
	case PROP_IS_MASTER:
		priv->is_master = g_value_get_boolean(value);
		break;
	case PROP_MAX_LINK:
		priv->max_link = g_value_get_int(value);
		break;
	}
}


static void
shared_memory_hub_dispose(GObject* object)
{

	SharedMemoryHub* self = (SharedMemoryHub*)object;
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);

	for (int i = 0; i < priv->max_link; i++)
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



static void
shared_memory_hub_get_property(GObject* object,
	guint prop_id,
	GValue* value,
	GParamSpec* pspec)
{
	SharedMemoryHub* self = SHARED_MEMORY_HUB(object);
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);

	switch (prop_id)
	{
	case PROP_HUB_ID:
		g_value_set_int(value, priv->hub_id);
		break;
	case PROP_IS_MASTER:
		g_value_set_boolean(value, priv->is_master);
		break;
	case PROP_MAX_LINK:
		g_value_set_int(value, priv->max_link);
		break;
	}
}


/// <summary>
/// asynchronous method used to create link between two shared memory hub,
/// used g_task to run establish link function in separate thread.
/// </summary>
/// <param name="self"></param>
/// <param name="peer_hub_id"></param>
/// <param name="block_size"></param>
/// <param name="pipe_size"></param>
/// <param name="cancellable"></param>
/// <param name="callback"></param>
/// <param name="user_data"></param>
void
shared_memory_hub_link_default_async(SharedMemoryHub* self,
	gint peer_hub_id,
	gint block_size,
	gint pipe_size,
	GCancellable* cancellable,
	GAsyncReadyCallback callback,
	gpointer user_data)
{
	SharedMemoryHubClass* klass = SHARED_MEMORY_HUB_GET_CLASS(self);
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(self);

	SharedMemoryLink* link = g_object_new(SHARED_MEMORY_TYPE_LINK, NULL);

	InitData* init_data = malloc(sizeof(InitData));

	/*algorithm summation:
	*atomic_create_block and atomic_create_pipe function is a function 
	*which take the first parameter as a seed for an name and id generate algorithm
	* 
	* in hand shaking process, if hub is master, algorithm will take peer hub id as a seed
	* when slave take its own id as a seed , it will got the same id and name  
	*/
	if (priv->is_master == TRUE)
	{
		init_data->mem_block = klass->atomic_create_block(peer_hub_id, block_size);
		init_data->send_pipe = klass->atomic_create_pipe(peer_hub_id, pipe_size);
		init_data->receive_pipe = klass->atomic_create_pipe(peer_hub_id, pipe_size);
	}
	else
	{

		init_data->mem_block = klass->atomic_create_block(priv->hub_id, block_size);
		init_data->send_pipe = klass->atomic_create_pipe(priv->hub_id, pipe_size);
		init_data->receive_pipe = klass->atomic_create_pipe(priv->hub_id, pipe_size);
	}
	init_data->is_master =		priv->is_master;
	init_data->owned_hub =		self; 
	init_data->owned_hub_id =	priv->hub_id;
	init_data->peer_hub_id =	peer_hub_id;


	GTask* task = g_task_new(self, cancellable, callback, user_data);
	g_task_set_task_data(task, init_data, (GDestroyNotify)init_data_free);

	g_task_run_in_thread(task, establish_link);
	g_object_unref(task);
}

/// <summary>
/// free link initialization data used by asynchronous function
/// </summary>
/// <param name="data"></param>
static void
init_data_free(InitData* data)
{
	g_free(data->mem_block);
	g_free(data->receive_pipe);
	g_free(data->send_pipe);
	g_free(data);
}

/// <summary>
/// link_finish function used to catch data from asynchronous function
/// </summary>
/// <param name="self"></param>
/// <param name="result"></param>
/// <param name="error"></param>
/// <returns></returns>
SharedMemoryLink* 
shared_memory_hub_link_finish(SharedMemoryHub* self, 
	GAsyncResult* result, 
	GError **error)
{

	g_return_val_if_fail(g_task_is_valid(result, self), NULL);

	g_signal_emit(self, signals[SIGNAL_LINKED], 0);

	return g_task_propagate_pointer(G_TASK(result),error);
}


/// <summary>
/// new empty block used as shared memory between two shared memory hub
/// </summary>
/// <param name="id"></param>
/// <param name="block_size"></param>
/// <returns></returns>
MemoryBlock* 
new_empty_block(gint id, gsize block_size)
{
	MemoryBlock* ret = malloc(sizeof(MemoryBlock));

	ret->state = UN_OWN;
	ret->size = block_size;
	ret->id = g_random_int_range(100,1000000);
	char prefix[256] = "\\\\.\\pipe\\";
	char ending[7];

	_itoa_s(ret->id, ending,7, 7);
	ret->name = strcat_s(prefix, 256, ending);
	ret->handle = NULL;
	ret->pointer = NULL;
	ret->context = NULL;
	ret->position = 0;
	return ret;
}


NamedPipe* 
new_empty_pipe(gint id,gsize buffer_size)
{
	NamedPipe* ret = sizeof(NamedPipe);

	ret->size = buffer_size;
	ret->id = g_random_int();
	ret->name = strcat_s("Global\\",8, (gchar*)ret->id);
	ret->handle = NULL;
	ret->context = NULL;
	return ret;
}




/// <summary>
/// Master hub process peer memory block transfer between two slave hub
/// </summary>
/// <param name="hub"></param>
/// <param name="msg"></param>
/// <param name="destination"></param>
/// <returns></returns>
gboolean
shared_memory_hub_perform_peer_transfer_request(SharedMemoryHub* hub,
	MemoryBlock* block,
	gint destination)
{
	SharedMemoryHubPrivate* priv = shared_memory_hub_get_instance_private(hub);

	

	for (gint i = 0; i < priv->max_link; i++)
	{
		int z;
		g_object_get_property(priv->link_array[i], "peer-id", &z);
		if (z == destination)
		{
			g_free(z);
			if (!shared_memory_link_send_message_lite(priv->link_array[i],
				PEER_TRANSFER_REQUEST,block))
			{
				return FALSE;
			}
		}
	}

	g_signal_emit(hub, signals[SIGNAL_PEER_TRANSFER], 0);
	return TRUE;
}

