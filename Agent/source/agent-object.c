#pragma once
#include <agent-object.h>
#include <Object.h>



#include <Cmd.h>
#include <SharedMemory.h>
#include <Socket.h>
#include <DeviceInformation.h>




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

	klass->add_local_nas_storage;      
	klass->session_initialization;
	klass->session_termination;
	klass->command_line_passing;		
	klass->remote_control_disconnect;
	klass->remote_control_reconnect;
	klass->connect_to_host =			connect_to_host_async;            
	klass->query_device_information  =	get_device_state;    
	klass->send_message =				send_message;

}

static void
agent_object_constructed(GObject* object)
{
	AgentObject* self = (AgentObject*) object;
	AgentObjectPrivate* priv =  agent_object_get_instance_private(self);

	priv->ipc->hub = shared_memory_hub_master_new();
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
	AgentObject* agent =g_object_new(AGENT_TYPE_OBJECT,NULL);
	AgentObjectPrivate* priv = agent_object_get_instance_private(agent);
	priv->socket->host_url = Host_URL;



	AgentObjectClass* klass = AGENT_OBJECT_GET_CLASS(agent);
	klass->connect_to_host(agent);
}





gboolean
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

DeviceInformation*
agent_object_get_information(AgentObject* self)
{
	AgentObjectPrivate* priv = agent_object_get_instance_private(self);
	return priv->device_information;
}