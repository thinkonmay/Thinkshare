#pragma once
#ifndef __OBJECT_h__
#define __OBJECT_H__
#include "frame-work.h"



#define MAX_LINK_HUB 64
#define MAX_LINK_MASTER 64
#define MASTER_ID 0
#define THREAD_PER_LINK 4

typedef enum
{
	OPCODE_UNKNOWN,
	MESSAGE,
	MESSAGE_HUGE,

	PEER_LINK_REQUEST,
	NEW_LINK_RESPOND,
	UPDATE_HUB_ID_ARRAY,
}SharedMemoryOpcode;

typedef struct
{
	SharedMemoryOpcode opcode;
	gint size;
	gpointer data_pointer;
}SharedMemoryMessage;


typedef struct
{
	gpointer buffer_pointer;
	gint size;

#ifdef G_OS_WIN32
	HANDLE mutex;
#endif
	GThread* send;
	GThread* receive;
	gint sender_id;

	SharedMemoryOpcode opcode;
	gint message_size;
}MemorySegment;



typedef struct
{
	GAsyncQueue* send_queue;

	GAsyncQueue* receive_queue;

	GThread* input_handling_thread;
}ThreadPool;

typedef struct
{
	MemorySegment* segment;
	ThreadPool* pool;
}ThreadData;

typedef struct
{
#ifdef G_OS_WIN32
	HANDLE handle;
#endif

	gint peer_id1;
	gint peer_id2;


	MemorySegment segment[THREAD_PER_LINK];

	gchar* buffer;
}MemoryBlock;

typedef struct
{	
	HANDLE* process_handle;
	gint id;
}Slave;












#endif