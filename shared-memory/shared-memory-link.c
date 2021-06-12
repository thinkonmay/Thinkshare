#include "frame-work.h"
#include "shared-memory-hub.h"
#include "shared-memory-link.h"


typedef struct
{
	MemoryBlock** block_array;

	gint max_block;

	SharedMemoryHub* owned_hub;
	gint owned_hub_id;
	gint peer_id;

	Pipe* send_pipe;
	Pipe* receive_pipe;

	gint link_id;
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

	PROP_LAST
};

G_DEFINE_TYPE_WITH_PRIVATE(SharedMemoryLink, shared_memory_link, G_TYPE_OBJECT)

static guint signals[SIGNAL_LAST] = { 0, };
static guint properties[PROP_LAST] = {0, };

static void
shared_memory_link_class_init(SharedMemoryLinkClass* klass)
{
	GObjectClass* object_class = G_OBJECT_CLASS(klass);

	object_class->constructed = shared_memory_link_constructed;
	object_class->get_property = shared_memory_get_property;
	object_class->dispose = shared_memory_link_dispose;
	object_class->finalize = shared_memory_link_finalize;

	klass->atomic_mem_read =  atomic_mem_read;
	klass->atomic_mem_write = atomic_mem_write;
	klass->atomic_pipe_send = atomic_pipe_send;
	klass->establish_link =   establish_link;
	klass->send_link_signal = send_link_signal;

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
			G_TYPE_NONE, 3,
			G_TYPE_INT, G_TYPE_INT, G_TYPE_POINTER);


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

}

static void
shared_memory_link_constructed(GObject* object)
{
	SharedMemoryLink* self = (SharedMemoryLink*)object;
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);

	priv->block_array = malloc(priv->max_block * sizeof(gpointer));
	memset(priv->block_array, NULL, priv->max_block);
}

static void
shared_memory_get_property(GObject* object) 
{

}

static void
shared_memory_link_dispose(GObject* object)
{

}

static void
shared_memory_link_finalize(GObject* object)
{

}

gpointer
atomic_mem_read(SharedMemoryLink* self,
	MemoryBlock* block)
{
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);
	gpointer ret;

	g_main_context_push_thread_default(block->context);
	while (TRUE)
	{
		if (block->state==priv->hub_is_master)
		{
			break;
		}
	}
	memcpy(ret, block->pointer, block->size);
	send_link_signal(self, READ_DONE);
	g_main_context_pop_thread_default(block->context);
	return ret;
}

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
	memcpy(block->pointer, data, data_size);
	send_link_signal(self, WRITE_DONE);
	g_main_context_pop_thread_default(block->context);
}

gboolean 
shared_memory_link_send_message(SharedMemoryLink* self, 
	gint destination,
	gpointer data,
	gsize data_size)
{
	SharedMemoryLinkClass* klass = SHARED_MEMORY_LINK_GET_CLASS(self);
	SharedMemoryLinkPrivate* priv = shared_memory_get_instance_private(self);


	if (destination == priv->peer_id)
	{
		if (!klass->atomic_mem_read(self, priv->block_array[0]))
		{
			return FALSE
		}
		klass->atomic_mem_write(self, priv->block_array[0],data,data_size);
		send_link_signal(self, MESSAGE);
	}
	else
	{
		MemoryBlock* block = new_empty_block(data_size);

		block->handle =	CreateFileMapping(
				INVALID_HANDLE_VALUE,    // use paging file
				NULL,                    // default security
				PAGE_READWRITE,          // read/write access
				0,                       // maximum object size (high-order DWORD)
				data_size,                // maximum object size (low-order DWORD)
				block->name);                 // name of mapping object
		
		
		block->pointer = 
			MapViewOfFile(block->handle,   // handle to map object
				FILE_MAP_ALL_ACCESS, // read/write permission
				0,
				0,
				data_size);
		memcpy(block->pointer, data, data_size);

		block->pointer = NULL;
		block->handle =  NULL;

		shared_memory_link_send_message_lite(self, PEER_TRANSFER_REQUEST, priv->owned_hub_id, destination, block);
	}
}





/// <summary>
/// 
/// </summary>
/// <param name="self"></param>
/// <param name="data"></param>
/// <param name="data_size"></param>
/// <returns></returns>
gboolean
atomic_pipe_send(SharedMemoryLink* self,
	gpointer* data,
	gsize data_size) 
{
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);
	BYTE* buffer = (BYTE*)data;
	BYTE* read_buffer;

	gboolean ret = WriteFile(
		priv->send_pipe->handle,              // pipe handle 
		buffer,                                // message 
		sizeof(buffer),                        // message length 
		read_buffer,                           // bytes written 
		NULL);
	 return ret;
}

gboolean
send_link_signal(SharedMemoryLink* self,
	gint opcode)
{
	SharedMemoryLinkClass* klass = SHARED_MEMORY_LINK_GET_CLASS(self);
	PacketLite* pkg;
	pkg->opcode = opcode;
	pkg->data = NULL;
	pkg->to = NULL;
	/*no destination needed for signal link*/

	gboolean ret = klass->atomic_pipe_send(self, &opcode, sizeof(gint));
	g_free(pkg);
	return ret;
}

gboolean
shared_memory_link_send_message_lite(SharedMemoryLink* self,
	gint opcode,
	gint destination,
	gpointer data)
{
	SharedMemoryLinkClass* klass = SHARED_MEMORY_LINK_GET_CLASS(self);
	SharedMemoryLinkPrivate* priv = shared_memory_get_instance_private(self);
	PacketLite* pkg;
	pkg->opcode = opcode;

	int from;
	g_object_get_property(priv->owned_hub_id, "hub-id", &from);
	pkg->from = from;
	pkg->to = destination;
	pkg->size = sizeof(data);
	/*copy from data to pkg buffer*/
	memcpy(pkg->data, data, sizeof(data));


	return klass->atomic_pipe_send(self, pkg, sizeof(PacketLite));
}









gboolean
establish_link(GTask* task,
	gpointer source_object,
	gpointer data,
	GCancellable *cancellable)
{
	SharedMemoryLink* self = source_object;
	InitData* init_data = data;
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);
	SharedMemoryLinkClass* klass = SHARED_MEMORY_LINK_GET_CLASS(self);

	priv->receive_pipe =   init_data->receive_pipe;
	priv->send_pipe =      init_data->send_pipe;
	priv->block_array[0] = init_data->mem_block;
	priv->link_id =        init_data->link_id;
	priv->hub_is_master =  init_data->is_master;
	priv->link_id =        init_data->link_id;
	priv->owned_hub =      init_data->owned_hub;
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
				_tprintf(TEXT("CreateNamedPipe failed, GLE=%d.\n"), GetLastError());
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
				_tprintf(TEXT("CreateNamedPipe failed, GLE=%d.\n"), GetLastError());
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


	g_main_context_push_default_thread(priv->receive_pipe->context);

	handle_receive_pipe(self,priv->hub_is_master);

	g_main_context_pop_default_thread(priv->receive_pipe->context);






	shared_memory_link_send_message_lite(self, 
		PRIMARY_MEM_BLOCK_INFORMATION, 
		init_data->destination_id, 
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

void
handle_receive_pipe(SharedMemoryLink* self)
{
	SharedMemoryLinkPrivate* priv = shared_memory_get_instance_private(self);
	SharedMemoryLinkClass* klass = shared_memory_get_instance_private(self);
	BYTE* buffer = malloc(priv->receive_pipe->size * sizeof(BYTE));
	BYTE* read_buffer;

	do
	{
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

	PacketLite* pkg = (PacketLite*)buffer;

	switch (pkg->opcode)
	{/*
	case MEM_BLOCK_INFORMATION:
		if (priv->hub_is_master == TRUE)
		{
			g_printerr("MASTER do not receive memory_block");
			return;
		}
		else
		{
			link_add_mem_block(self, (MemoryBlock*)pkg->data, FALSE);
		}
	*/
	case PRIMARY_MEM_BLOCK_INFORMATION:
		if (priv->hub_is_master == TRUE)
		{
			g_printerr("MASTER do not receive memory_block");
			return;
		}
		else
		{
			link_add_mem_block(self, (MemoryBlock*)pkg->data, TRUE);
		}
	case READ:
		if (priv->block_array[0]->state != UN_OWN)
		{
			klass->send_link_signal(self, STATE_ERROR);
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
			klass->send_link_signal(self, STATE_ERROR);
		}
		else
		{
			priv->block_array[0]->state = UN_OWN;
		}
	case WRITE:
		if (priv->block_array[0]->state != UN_OWN)
		{
			klass->send_link_signal(self, STATE_ERROR);
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
			klass->send_link_signal(self, STATE_ERROR);
		}
		else
		{
			priv->block_array[0]->state = UN_OWN;
		}
	case MESSAGE_LITE:
		if (priv->hub_is_master)
		{
			g_signal_emit(self, signals[SIGNAL_ON_DATA_MESSAGE], pkg->from, pkg->to, pkg->data);
		}
	case MESSAGE:
		if (priv->block_array[0]->state != UN_OWN)
		{
			klass->send_link_signal(self, STATE_ERROR);
		}
		else
		{

			priv->block_array[0]->state = !priv->hub_is_master;

			gpointer receive_data = klass->atomic_mem_read(self, priv->block_array[0]);

			g_signal_emit(self, signals[SIGNAL_ON_DATA_MESSAGE], pkg->from, pkg->to, receive_data);
		}

	case PEER_TRANSFER_REQUEST:
		if (!priv->hub_is_master)
		{
			MemoryBlock* block = (MemoryBlock*)pkg->data;
			link_add_mem_block(self, block, FALSE);

			gpointer receive_data = klass->atomic_mem_read(self, priv->block_array[block->position]);

			g_signal_emit(self, signals[SIGNAL_ON_DATA_MESSAGE], pkg->from, pkg->to, receive_data);

			link_del_mem_block(self, block);
		}
		else
		{
			MemoryBlock* block = (MemoryBlock*)pkg->data;


			shared_memory_hub_perform_peer_transfer_request(priv->owned_hub_id, self, pkg, pkg->to);
		}
	}

	g_free(pkg);
}


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

void
link_del_mem_block(SharedMemoryLink* self,
	MemoryBlock* block)
{
	SharedMemoryLinkPrivate* priv = shared_memory_link_get_instance_private(self);
	
	priv->block_array[block->position] = NULL;

	g_free(block->handle);
	g_free(block);
}



