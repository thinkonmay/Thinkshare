#include <Windows.h>
#include <glib.h>
#include <glib-2.0/glib-object.h>
#include <glib-2.0/gio/gio.h>
#include "Framework.h"



///do not change this definition
#define MASTER_OWN 1
#define SLAVE_OWN  0
#define UN_OWN     2


enum
{
	MESSAGE_LITE,
	MESSAGE,

	PRIMARY_MEM_BLOCK_INFORMATION,
	MEM_BLOCK_INFORMATION,

	READ,
	WRITE,
	READ_DONE,
	WRITE_DONE,
	STATE_ERROR,

	PEER_TRANSFER_REQUEST,
};

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
	BYTE opcode;

	gint from;
	gint to;

	gsize size;
	gpointer data;
}PacketLite;

typedef struct
{
	gint			link_id;
	MemoryBlock*	mem_block;
	Pipe*			send_pipe;
	Pipe*			receive_pipe;
	gint			link_id;
	gboolean		is_master;
	gint			destination_id;
	SharedMemoryHub*owned_hub;
	gint            owned_hub_id;
	gint            peer_hub_id;
}InitData;




typedef struct
{
	gint id;

	HANDLE handle;
	LPCSTR name;

	gint size;

	GMainContext* context;
}Pipe;