#pragma once
#include "Framework.h"
#include "Object.h"
#include "agent-object.h"

#include "Cmd.h"
#include "SharedMemory.h"
#include "Socket.h"
#include "DeviceInformation.h"




/// <summary>
/// agent instance base for instance
/// </summary>
typedef struct 
{
	AgentState state;

	DeviceState* device_state;

	DeviceInformation* device_information;

	IPC* ipc;

	Socket* socket;
}AgentObjectPrivate;


///define agent object data type (follow gobject standard)
G_DEFINE_TYPE_WITH_PRIVATE(AgentObject, agent_object, G_TYPE_OBJECT)


static void
agent_object_class_init(AgentObjectClass* klass)
{
	GObjectClass* object_class = G_OBJECT_CLASS(klass);

	object_class->constructed = agent_object_constructed;
	object_class->dispose = agent_object_dispose;
	object_class->finalize = agent_object_finalize;
	klass->connect_to_host = connect_to_host_async;             ////phần này sẽ là phần cần phối hợp với Trường Giang do đó sẽ gác lại sau
	klass->query_device_information  = get_device_state;    ////tiếp theo bảo có thể làm phần này,
	klass->send_message = send_messsage;

}

static void
agent_object_constructed(GObject* object)
{

}

static void
agent_object_dispose(GObject* object)
{

}

static void
agent_object_finalize(GObject* object)
{

}


AgentObject*
agent_object_new(gchar* Host_URL,
	gint Host_ID)
{

}

void
handle_host_connection(AgentObject* self)
{

}

void
handle_shared_memory_hub(AgentObject* self)
{

}







send_message_to(AgentObject* self,
	Message* message)
{
send_message(AgentObject* self,
	Message* message)
{
	switch (message->to)
	{
	case HOST:
	{
		send_message_to_host(self, message);
	}
	case CORE:
		send_message_through_shared_memory(self, message);
	case LOADER:
		send_message_through_shared_memory(self, message);
	case CLIENT:
		send_message_through_shared_memory(self,message);
	}
}

}
Socket*
agent_object_get_socket(AgentObject* self)
{
	AgentObjectPrivate* priv = agent_object_get_instance_private(self);
	return priv->socket;
}

DeviceInformation*
agent_object_get_information(AgentObject* self)
{
	AgentObjectPrivate* priv = agent_object_get_instance_private(self);
	return priv->device_information;
}
send_message(AgentObject* self,
	Location from,
	Location to,
	Opcode opcode,
	gpointer data)
{
	switch (to)
	{
	case HOST:
		send_message_to_host(self, from, to, opcode, data);
	case CORE:
		send_message_through_shared_memory(self, from, to, opcode, data);
	case LOADER:
		send_message_through_shared_memory(self, from, to, opcode, data);
	case CLIENT:
		send_message_through_shared_memory(self, from, to, opcode, data);
	}
}



IPC*
agent_object_get_ipc(AgentObject* self)
{
	AgentObjectPrivate* priv = agent_object_get_instance_private(self);
	return priv->ipc;
}

Socket*
agent_object_get_socket(AgentObject* self)
{
	AgentObjectPrivate* priv = agent_object_get_instance_private(self);
	return priv->socket;
}
