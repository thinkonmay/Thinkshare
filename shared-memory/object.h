#pragma once

#ifndef __TYPE_DEF_h__
#define __TYPE_DEF_H__

#include <glib-2.0/glib.h>
#include <windows.h>
#include <glib-2.0/glib-object.h>
typedef struct pipe
{
	gint id;

	HANDLE handle;
	LPCSTR name;

	gint size;

	GMainContext* context;
}NamedPipe;

typedef enum opcode
{
	MESSAGE,

	PRIMARY_MEM_BLOCK_INFORMATION,

	READ,
	WRITE,
	READ_DONE,
	WRITE_DONE,
	STATE_ERROR,

	PEER_TRANSFER_REQUEST,
}Opcode;

typedef struct message
{
	gint from;
	gint to;
	Opcode opcode;
	gpointer data;
}Message;


typedef struct memory_block
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


typedef struct init_data
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