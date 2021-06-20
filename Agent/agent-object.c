﻿#pragma once
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

	klass->add_local_nas_storage;       ///phần này có thể hoàn thiện sau
	klass->session_initialization;      ///session_  
	klass->session_termination;
	klass->command_line_passing;		////tôi cần bảo đầu tiên là hoàn thiện method commandline parsing này
	klass->remote_control_disconnect;
	klass->remote_control_reconnect;
	klass->connect_to_host;             ////phần này sẽ là phần cần phối hợp với Trường Giang do đó sẽ gác lại sau
	klass->query_device_information;    ////tiếp theo bảo có thể làm phần này,
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

gboolean
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