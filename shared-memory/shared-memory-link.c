#pragma once
#include "frame-work.h"
#include "shared-memory-hub.h"
#include "shared-memory-link.h"
#include "object.h"



/// <summary>
/// private struct
/// </summary>
typedef struct
{
	MemoryBlock** block_array;
	gint max_block;

	SharedMemoryHub* owned_hub;
	gint owned_hub_id;
	gint peer_id;

	NamedPipe* send_pipe;
	NamedPipe* receive_pipe;

	gboolean hub_is_master;
}SharedMemoryLinkPrivate;

enum
{
	SIGNAL_MEM_READ,
	SIGNAL_MEM_WRITE,
	SIGNAL_MEM_READ_DONE,
	SIGNAL_MEM_WRITE_DONE,
	SIGNAL_MEM_STATE_ERROR,
	SIGNAL_PIPE_CONNECTED,
	SIGNAL_ON_DATA_MESSAGE,

	SIGNAL_LAST
};



enum
{
	PROP_PEER_ID,
	PROP_OWNED_HUB_ID,
	PROP_HUB_IS_MASTER,

	PROP_LAST
};



/*define gtype (glib standard)*/
G_DEFINE_TYPE_WITH_PRIVATE(SharedMemoryLink, shared_memory_link, G_TYPE_OBJECT)



/*declaration stuff*/
static void
shared_memory_link_constructed(GObject* object);

static void
shared_memory_link_get_property(GObject* object);

static void
shared_memory_get_property(GObject* object);

static void
shared_memory_link_dispose(GObject * object);

static void
shared_memory_link_finalize(GObject * object);

gpointer
atomic_mem_read(SharedMemoryLink* self,
	MemoryBlock* block);

gboolean
atomic_mem_write(SharedMemoryLink* self,
	MemoryBlock* block,
	gpointer data,
	gsize data_size);

gboolean
atomic_pipe_send(SharedMemoryLink* self,
	gpointer data,
	gsize data_size);
gboolean
establish_link(GTask* task,
	gpointer source_object,
	gpointer data,
	GCancellable *cancellable);
void
link_add_mem_block(SharedMemoryLink* self,
	MemoryBlock* block,
	gboolean is_primary);
void
link_del_mem_block(SharedMemoryLink* self,
	MemoryBlock* block);

static guint signals[SIGNAL_LAST] = { 0, };
static guint properties[PROP_LAST] = {0, };


/// <summary>
/// class initialization: override gobject base class method
/// </summary>
/// <param name="klass"></param>
static void
shared_memory_link_class_init(SharedMemoryLinkClass* klass)
{
	/*get object class from shared memory class*/
	GObjectClass* object_class = G_OBJECT_CLASS(klass);

	object_class->constructed = shared_memory_link_constructed;
	object_class->get_property = shared_memory_get_property;
	object_class->dispose = shared_memory_link_dispose;
	object_class->finalize = shared_memory_link_finalize;


	///register object signal
	signals[SIGNAL_PIPE_CONNECTED] =
		g_signal_new("pipe-connected",
			SHARED_MEMORY_TYPE_LINK,
			G_SIGNAL_RUN_LAST,
			0, NULL, NULL, NULL,
			G_TYPE_NONE, 0);
	signals[SIGNAL_ON_DATA_MESSAGE] =
		g_signal_new("on-message",
			SHARED_MEMORY_TYPE_LINK,
			G_SIGNAL_RUN_LAST,
			0, NULL, NULL, NULL,
			G_TYPE_NONE, 4,
			G_TYPE_INT,G_TYPE_INT, G_TYPE_INT, G_TYPE_POINTER);

	//register object properties
	properties[PROP_PEER_ID] =
		g_param_spec_int("peer-id",
			"PeerID",
			"peer id",
			0, G_MAXINT, 0, G_PARAM_READABLE);
	properties[PROP_OWNED_HUB_ID] =
		g_param_spec_int("hub-id",
			"hubid",
			"hub id",
			0, G_MAXINT, 0, G_PARAM_READABLE);
	properties[PROP_OWNED_HUB_ID] =
		g_param_spec_boolean("hub-is-master",
			"hub-is-master",
			"hub id",
			FALSE, G_PARAM_READABLE);

}

/// <summary>
/// object instance initialization: declare link class method
/// </summary>
/// <param name="self"></param>
static void
shared_memory_link_init(SharedMemoryLink* self)
{
	/*get class from instance*/
	SharedMemoryLinkClass* klass = SHARED_MEMORY_LINK_GET_CLASS(self);

	klass->atomic_mem_read = atomic_mem_read;
	klass->atomic_mem_write = atomic_mem_write;
	klass->atomic_pipe_send = atomic_pipe_send;
	klass->establish_link = establish_link;
}

/// <summary>
/// constructed method of g object base class: run after initialize process is done
/// </summary>
/// <param name="object"></param>
static void
shared_memory_link_constructed(GObject* object)
{
	SharedMemoryLink* self = (SharedMemoryLink*)object;
	/*get private struct from shared memory instance*/
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);

	/*Allocate and setup block array, prepare for memory add  and remove*/
	priv->block_array = malloc(priv->max_block * sizeof(gpointer));
	
	for(int i = 0; i < priv->max_block; i++)
	{
		priv->block_array[i] = NULL;
	}
}


/// <summary>
/// get property method of base class: get properties from object
/// </summary>
/// <param name="object"></param>
static void
shared_memory_get_property(GObject* object) 
{

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

	for (int i =0; i< priv->max_block;i++)
	{
		g_free(priv->block_array[i]);
	}
	g_free(priv->receive_pipe);
	g_free(priv->send_pipe);
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


/// <summary>
/// atomic memory read function, cannot use directly by user but used by other method
/// </summary>
/// <param name="self"></param>
/// <param name="block"></param>
/// <returns></returns>
gpointer
atomic_mem_read(SharedMemoryLink* self,
	MemoryBlock* block)
{
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);
	gpointer ret = malloc(block->size);

	g_main_context_push_thread_default(block->context);
	while (TRUE)
	{
		if (block->state==priv->hub_is_master)
		{
			break;
		}
	}
	/*copy memory from memory block to ret memory, then return ret pointer*/
	memcpy(ret, block->pointer, block->size);
	shared_memory_link_send_message_lite(self, READ_DONE,NULL);
	g_main_context_pop_thread_default(block->context);
	return ret;
}


/// <summary>
/// atomic memory write, similiar to atomic memread
/// </summary>
/// <param name="self"></param>
/// <param name="block"></param>
/// <param name="data"></param>
/// <para           m name="data_size"></param>
/// <returns></returns>
gboolean
atomic_mem_write(SharedMemoryLink* self,
	MemoryBlock* block,
	gpointer data,
	gsize data_size)
{
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);
	gpointer ret;

	g_main_context_push_thread_default(block->context);
	while (TRUE)
	{
		if (block->state == priv->hub_is_master)
		{
			break;
		}
	}
	/* copy data from data that (data) pointer pointed to, to memory block*/
	memcpy(block->pointer, data, data_size);
	shared_memory_link_send_message_lite(self, WRITE_DONE, NULL);
	g_main_context_pop_thread_default(block->context);
}


/// <summary>
/// public method used by user to send message to other hub.
/// </summary>
/// <param name="self"></param>
/// <param name="msg"></param>
/// <returns></returns>
gboolean 
shared_memory_link_send_message(SharedMemoryLink* self,
	gint to,
	gpointer data)
{
	SharedMemoryLinkClass* klass = SHARED_MEMORY_LINK_GET_CLASS(self);
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);


	if (to == priv->peer_id)
	{
		if (shared_memory_link_send_message_lite(self, READ, NULL))
		{
			return FALSE;
		}
		klass->atomic_mem_write(self, priv->block_array[0],data,sizeof(data));
		shared_memory_link_send_message_lite(self, MESSAGE, NULL);
	}
	else
	{
		/*process peer transfer process*/
		MemoryBlock* block = new_empty_block(g_random_int(),sizeof(data));

		/*get handle (search for window handle concept) of memory block*/
		block->handle =	
			CreateFileMapping(
				INVALID_HANDLE_VALUE,    // use paging file
				NULL,                    // default security
				PAGE_READWRITE,          // read/write access
				0,                       // maximum object size (high-order DWORD)
				sizeof(data),                // maximum object size (low-order DWORD)
				block->name);                 // name of mapping object
		
		/*get memory block pointer from memory block handle*/
		block->pointer = 
			MapViewOfFile(block->handle,   // handle to map object
				FILE_MAP_ALL_ACCESS, // read/write permission
				0,
				0,
				sizeof(data));

		/*copy data from message to block data*/
		memcpy(block->pointer, data, sizeof(data));

		block->pointer = NULL;
		block->handle =  NULL;

		/*initialize message */
		SharedMemoryMessage* msg = malloc(sizeof(SharedMemoryMessage));
		msg->from = priv->owned_hub_id;
		msg->to = to;
		msg->opcode = PEER_TRANSFER_REQUEST;
		msg->data = block;

		/*send peer transfer request to master hub*/
		shared_memory_link_send_message_lite(self, PEER_TRANSFER_REQUEST, msg);
	}
}





/// <summary>
/// atomic send message over named pipe function, used by other method like 
/// atomic_mem_read to signal peer about memory block state 
/// </summary>
/// <param name="self"></param>
/// <param name="data"></param>
/// <param name="data_size"></param>
/// <returns></returns>
gboolean
atomic_pipe_send(SharedMemoryLink* self,
	gpointer data,
	gsize data_size) 
{
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);
	BYTE* buffer = (BYTE*)data;
	BYTE* read_buffer  = malloc(data_size);

	gboolean ret =
	WriteFile(
		priv->send_pipe->handle,              // pipe handle 
		buffer,                                // message 
		sizeof(buffer),                        // message length 
		read_buffer,                           // bytes written 
		NULL);
	 return ret;
}

/// <summary>
/// send lightweight message to peer hub over named pipe, base on atomic_pipe_send function
/// 
/// </summary>
/// <param name="self"></param>
/// <param name="opcode"></param>
/// <param name="data"></param>
/// <returns></returns>
gboolean
shared_memory_link_send_message_lite(SharedMemoryLink* self,
	SharedMemoryOpcode opcode,
	gpointer data)
{
	SharedMemoryLinkClass* klass = SHARED_MEMORY_LINK_GET_CLASS(self);
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);

	SharedMemoryMessage* msg = malloc(sizeof(SharedMemoryMessage));
	msg->opcode = opcode;
	/*copy from data to pkg buffer*/
	if(data != NULL)
	{
		memcpy(msg->data, data, sizeof(data));
	}
	return klass->atomic_pipe_send(self, msg, sizeof(SharedMemoryMessage));
}





/// <summary>
/// handle named pipe message to:
/// 1. Determine memory block state.
/// 2. Master and slave process peer transfer
/// </summary>
/// <param name="self"></param>
void
handle_receive_pipe(SharedMemoryLink* self)
{
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);
	SharedMemoryLinkClass* klass = shared_memory_link_get_instance_private(self);

	BYTE* buffer = malloc(priv->receive_pipe->size * sizeof(BYTE));
	BYTE* read_buffer = malloc(priv->receive_pipe->size);

	do
	{
		/*Read file function from window.h to read from named pipe*/
		if (ReadFile(
			priv->receive_pipe->handle, // pipe handle 
			buffer,    // buffer to receive reply 
			sizeof(buffer),  // size of buffer 
			read_buffer,  // number of bytes read 
			NULL));    // not overlapped )
		{
			break;
		}
	} while (TRUE);
	g_free(read_buffer);

	SharedMemoryMessage* msg = (SharedMemoryMessage*)buffer;

	switch (msg->opcode)
	{
	case PRIMARY_MEM_BLOCK_INFORMATION:
		if (priv->hub_is_master == TRUE)
		{
			g_printerr("MASTER do not receive memory_block");
			return;
		}
		else
		{
			link_add_mem_block(self, (MemoryBlock*)msg->data, TRUE);
		}
	case READ:
		if (priv->block_array[0]->state != UN_OWN)
		{
			shared_memory_link_send_message_lite(self, STATE_ERROR, NULL);
		}
		else
		{
			if (priv->hub_is_master)
			{
				priv->block_array[0]->state = SLAVE_OWN;
			}
			else
			{
				priv->block_array[0]->state = MASTER_OWN;
			}
		}
	case READ_DONE:
		if (priv->block_array[0]->state == UN_OWN)
		{
			shared_memory_link_send_message_lite(self, STATE_ERROR, NULL);
		}
		else
		{
			priv->block_array[0]->state = UN_OWN;
		}
	case WRITE:
		if (priv->block_array[0]->state != UN_OWN)
		{
			shared_memory_link_send_message_lite(self, STATE_ERROR, NULL);
		}
		else
		{
			if (priv->hub_is_master)
			{
				priv->block_array[0]->state = SLAVE_OWN;
			}
			else
			{
				priv->block_array[0]->state = MASTER_OWN;
			}
		}
	case WRITE_DONE:
		if (priv->block_array[0]->state == UN_OWN)
		{
			shared_memory_link_send_message_lite(self, STATE_ERROR, NULL);
		}
		else
		{
			priv->block_array[0]->state = UN_OWN;
		}
	case MESSAGE:
		if (priv->block_array[0]->state != UN_OWN)
		{
			shared_memory_link_send_message_lite(self, STATE_ERROR, NULL);
		}
		else
		{

			priv->block_array[0]->state = !priv->hub_is_master;

			SharedMemoryMessage* message = klass->atomic_mem_read(self, priv->block_array[0]);

			g_signal_emit(self, signals[SIGNAL_ON_DATA_MESSAGE],
				message->from, message->to, message->opcode, message->data);
		}

	case PEER_TRANSFER_REQUEST:
		if (!priv->hub_is_master)
		{
			/*Algorithm summation: if hub is not an master, its mean that hub is the destination of the message,
			* thus link will add the memblock which information lies inside message
			*/
			MemoryBlock* block = (MemoryBlock*)msg->data;
			link_add_mem_block(self, block, FALSE);

			gpointer data = klass->atomic_mem_read(self, priv->block_array[block->position]);

			g_signal_emit(self, signals[SIGNAL_ON_DATA_MESSAGE],
				msg->from, msg->to, msg->opcode, data);

			link_del_mem_block(self, block);
		}
		else
		{
			SharedMemoryMessage* message = (SharedMemoryMessage*)msg->data;
			shared_memory_hub_perform_peer_transfer_request(priv->owned_hub, message, msg->to);
		}
	}

	g_free(msg);
}



/// <summary>
/// GThreadFunction used to establish SharedMemoryLink between two hub,
/// this function is used by shared_memory_hub_link_default_async(),
/// do not use this method directly
/// </summary>
/// <param name="task"></param>
/// <param name="source_object"></param>
/// <param name="data"></param>
/// <param name="cancellable"></param>
/// <returns></returns>
gboolean
establish_link(GTask* task,
	gpointer source_object,
	gpointer data,
	GCancellable *cancellable)
{
	SharedMemoryLink* self = source_object;
	InitData* init_data;
	init_data = (InitData*)data;

	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);
	SharedMemoryLinkClass* klass = SHARED_MEMORY_LINK_GET_CLASS(self);

	priv->receive_pipe =   init_data->receive_pipe;
	priv->send_pipe =      init_data->send_pipe;
	priv->block_array[0] = init_data->mem_block;
	priv->hub_is_master =  init_data->is_master;
	priv->owned_hub =      init_data->owned_hub;
	priv->hub_is_master =  init_data->is_master;
	priv->owned_hub_id =   init_data->owned_hub_id;
	priv->peer_id =        init_data->peer_hub_id;

	if (priv->hub_is_master)
	{
		while(TRUE)
		{
			priv->send_pipe->handle =
				CreateNamedPipe(
					priv->send_pipe->name,    // pipe name 
					PIPE_ACCESS_OUTBOUND,     // read/write access 
					PIPE_TYPE_BYTE |          // message type pipe 
					PIPE_READMODE_BYTE |      // message-read mode 
					PIPE_WAIT,                // blocking mode 
					2, // max. instances  
					priv->send_pipe->size,    // output buffer size 
					priv->send_pipe->size,    // input buffer size 
					0,                        // client time-out 
					NULL);                    // default security attribute 

			if (priv->send_pipe->handle == INVALID_HANDLE_VALUE)
			{
				g_printerr(TEXT("CreateNamedPipe failed, GLE=%d.\n"), GetLastError());
				return FALSE;
			}

			if (ConnectNamedPipe(priv->send_pipe->handle, NULL)!=0)
			{
				g_print("Send-pipe connected");
				break;
			}
		}
		while(TRUE)
		{
			priv->receive_pipe->handle =
				CreateFile(
					priv->send_pipe->name,   // pipe name 
					GENERIC_READ,
					0,              // no sharing 
					NULL,           // default security attributes
					OPEN_EXISTING,  // opens existing pipe 
					0,              // default attributes 
					NULL);          // no template file 

			  // Break if the pipe handle is valid. 
			if (priv->receive_pipe->handle != INVALID_HANDLE_VALUE)
			{
				break;
			}
		}
	}
	else 
	{
		while (TRUE)
		{
			priv->receive_pipe->handle =
				CreateFile(
					priv->send_pipe->name,   // pipe name 
					GENERIC_READ,
					0,              // no sharing 
					NULL,           // default security attributes
					OPEN_EXISTING,  // opens existing pipe 
					0,              // default attributes 
					NULL);          // no template file 

			  // Break if the pipe handle is valid. 
			if (priv->receive_pipe->handle != INVALID_HANDLE_VALUE)
			{
				break;
			}
		}
		while (TRUE)
		{
			priv->send_pipe->handle =
				CreateNamedPipe(
					priv->send_pipe->name,    // pipe name 
					PIPE_ACCESS_OUTBOUND,     // read/write access 
					PIPE_TYPE_BYTE |          // message type pipe 
					PIPE_READMODE_BYTE |      // message-read mode 
					PIPE_WAIT,                // blocking mode 
					2, // max. instances  
					priv->send_pipe->size,    // output buffer size 
					priv->send_pipe->size,    // input buffer size 
					0,                        // client time-out 
					NULL);                    // default security attribute 

			if (priv->send_pipe->handle == INVALID_HANDLE_VALUE)
			{
				g_printerr(TEXT("CreateNamedPipe failed, GLE=%d.\n"), GetLastError());
				return FALSE;
			}

			if (ConnectNamedPipe(priv->send_pipe->handle, NULL) != 0)
			{
				g_print("Send-pipe connected");
				break;
			}
		}
	}

	g_signal_emit(self, signals[SIGNAL_PIPE_CONNECTED],0);

	priv->send_pipe->context =    g_main_context_new();
	priv->receive_pipe->context = g_main_context_new();

	/*push thread function, force handle receive pipe to run in receive pipe context 
	instead of blocking other process*/
	g_main_context_push_thread_default(priv->receive_pipe->context);

	handle_receive_pipe(self);

	g_main_context_pop_thread_default(priv->receive_pipe->context);

	shared_memory_link_send_message_lite(self,
		PRIMARY_MEM_BLOCK_INFORMATION,
		init_data->mem_block);

	/*wait for primary block to be added*/
	while (TRUE)
	{
		if (!priv->block_array[0]==NULL)
		{
			break;
		}
	}

	g_task_return_pointer(task, self, g_object_unref);
}


/// <summary>
/// add an memory block to memory_block_array
/// </summary>
/// <param name="self"></param>
/// <param name="block"></param>
/// <param name="is_primary"></param>
void
link_add_mem_block(SharedMemoryLink* self,
	MemoryBlock* block,
	gboolean is_primary)
{
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);

	/*at this time, block pointer is NULL until we create block handler*/
	block->handle =
		CreateFileMapping(
			INVALID_HANDLE_VALUE,    // use paging file
			NULL,                    // default security
			PAGE_READWRITE,          // read/write access
			0,                       // maximum object size (high-order DWORD)
			block->size,             // maximum object size (low-order DWORD)
			block->name);

	if (is_primary == FALSE)
	{
		for (int i = 0; i < priv->max_block; i++)
		{
			if (priv->block_array[i] == NULL)
			{
				block->position = i;
				priv->block_array[i] = block;
			}
		}
	}
	else
	{
		block->position = 0;
		priv->block_array[0] = block;
	}
}



/// <summary>
/// delete an memory block from mem_block_array
/// </summary>
/// <param name="self"></param>
/// <param name="block"></param>
void
link_del_mem_block(SharedMemoryLink* self,
	MemoryBlock* block)
{
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);
	
	priv->block_array[block->position] = NULL;

	g_free(block->handle);
	g_free(block);
}



