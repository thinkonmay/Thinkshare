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
	Session session;

	AgentState state;

	DeviceState* device_state;

	DeviceInformation* device_information;
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

	klass->

	
}

/// <summary>
/// agent object instance initialization
/// </summary>
/// <param name="object"></param>
static void
agent_object_init(AgentObject* object)
{

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
send_message_to(AgentObject* self,
	Message* message)
{

}
