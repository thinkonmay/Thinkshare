#pragma once

#ifndef __OBJECT_h__
#define __OBJECT_H__
#include "frame-work.h"


typedef struct
{
	gint id;

	HANDLE handle;
	LPCSTR name;

	gint size;

	GMainContext* context;
}NamedPipe;

typedef enum
{
	MESSAGE,

	PRIMARY_MEM_BLOCK_INFORMATION,

	READ,
	WRITE,
	READ_DONE,
	WRITE_DONE,
	STATE_ERROR,

	PEER_TRANSFER_REQUEST,
}SharedMemoryOpcode;

typedef struct
{
	gint from;
	gint to;
	SharedMemoryOpcode opcode;
	gpointer data;
}SharedMemoryMessage;


typedef struct
{
	HANDLE handle;
	gpointer pointer;

	gint state;

	gint id;
	LPCSTR name;

	gint size;
	gint position;

	GMainContext* context;
}MemoryBlock;


typedef struct
{
	MemoryBlock*		mem_block;
	NamedPipe*			send_pipe;
	NamedPipe*			receive_pipe;
	gboolean			is_master;
	GObject*	        owned_hub;
	gint				owned_hub_id;
	gint				peer_hub_id;
}InitData;


#endif