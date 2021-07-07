#include <agent-object.h>

#include <Windows.h>
#include <stdio.h>

#include <agent-type.h>
#include <agent-ipc.h>
#include <agent-socket.h>
#include <agent-device.h>
#include <agent-message.h>
#include <agent-cmd.h>
#include <agent-device.h>



/// <summary>
/// agent instance base for instance
/// </summary>
struct _AgentObject
{
	HANDLE state_mutex;

	DeviceState* device_state;

	DeviceInformation* device_information;

	IPC* ipc;

	Socket* socket;

	Session* session;

	gint state;

	GMainLoop* loop;
};


gpointer
update_device(gpointer data)
{
	AgentObject* self = (AgentObject*)data;


	while (TRUE)
	{

		WaitForSingleObject(self->state_mutex, INFINITE);
		self->device_information =	get_device_information();
		self->device_state	 =		get_device_state();
		ReleaseMutex(self->state_mutex);
		Sleep(1000);


		if (self->state == AGENT_CLOSED)
			break;
	}
	return NULL;
}

void
agent_object_constructed(AgentObject* self)
{
	SECURITY_ATTRIBUTES attr;
	attr.nLength = sizeof(SECURITY_ATTRIBUTES);
	attr.bInheritHandle = TRUE;
	attr.lpSecurityDescriptor = NULL;

	HANDLE mutex = CreateMutex(&attr, FALSE, NULL);

	self->state_mutex = &mutex;
	g_thread_new("update device", (GThreadFunc)update_device, self);

	self->loop = g_main_loop_new(NULL, FALSE);

	g_main_loop_run(self->loop);
	g_main_loop_unref(self->loop);
}

AgentObject*
agent_object_new(gchar* Host_URL)
{
	AgentObject* agent = malloc(sizeof(AgentObject));
	agent->state = AGENT_NEW;


	agent->ipc=		initialize_ipc(agent);
	agent->socket = initialize_socket(agent,Host_URL);

	agent->device_information = get_device_information();
	agent->device_state =		get_device_state();
	agent->state = ATTEMP_TO_RECONNECT;


	agent_connect_to_host(agent);

	agent_object_constructed(agent);
	return agent;
}




void
agent_object_finalize(AgentObject* object)
{
	object->state = AGENT_CLOSED;

	SoupWebsocketConnection* connection = 
		socket_get_connection(object->socket);

	soup_websocket_connection_close(connection,0,"");

	if (object->loop)
	{
		g_main_loop_quit(object->loop);
		object->loop = NULL;
		g_free(object);
	}
}


void
agent_register_with_host(AgentObject* self)
{
	if (self->state == ATTEMP_TO_RECONNECT)
		register_with_host(self);
	else
	{
		g_printerr("register while not in reconnect state");
		return;
	}
	self->state = SLAVE_REGISTERING;
	return;
}









IPC*
agent_object_get_ipc(AgentObject* self)
{
	return self->ipc;
}

Socket*
agent_object_get_socket(AgentObject* self)
{
	return self->socket;
}

DeviceState*
agent_get_device_state(AgentObject* self)
{
	return self->device_state;
}

DeviceInformation*
agent_get_device_information(AgentObject* self)
{
	return self->device_information;
}

Session*
agent_object_get_session(AgentObject* self)
{
	return self->session;
}

void
agent_object_set_session(AgentObject* self, Session* session)
{
	return self->session = session;
}

void
agent_set_state(AgentObject* object, AgentState state)
{
	object->state = state;
}

AgentState
agent_get_state(AgentObject* self)
{
	return self->state;
}

HANDLE*
agent_get_mutex_handle_ptr(AgentObject* self)
{
	return self->state_mutex;
}























/// <summary>
/// attemp to connect to host until succes or close signal is received
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
gboolean
agent_connect_to_host(AgentObject* self)
{
	while (TRUE)
	{
		connect_to_host_async(self);
		Sleep(10000);

		if (self->state == AGENT_OPEN || self->state == AGENT_CLOSED || self->state == SLAVE_REGISTERING)
			break;
	}
}




gboolean
agent_session_initialize(AgentObject* self)
{
	gboolean ret = FALSE;
	if (self->state != AGENT_OPEN)
	{
		ret = FALSE;
		g_printerr("cannot create new session while on one");
	}
	ret = session_initialize(self);
	self->state = ON_SESSION;
	return ret;
}


gboolean										
agent_disconnect_host(AgentObject* self)
{
	if (self->state != AGENT_CLOSED)
	{
		self->state = ATTEMP_TO_RECONNECT;
		agent_connect_to_host(self);
	}
	else
	{
		agent_object_finalize(self);
	}
}

gboolean										
agent_session_terminate(AgentObject* self)
{
	if (self->state == ON_SESSION || self->state == ON_SESSION_OFF_REMOTE)
	{
		g_free(self->session);
		self->session = NULL;
		session_terminate_process(self);
		self->state = AGENT_OPEN;
	}
	else
	{
		g_printerr("not in sesssion");
		return FALSE;
	}
	return TRUE;
}

gboolean										
agent_remote_control_disconnect(AgentObject* self)
{
	if (self->state == ON_SESSION)
	{
		session_terminate_process(self);
		self->state = ON_SESSION_OFF_REMOTE;
	}
	else 
	{
		g_printerr("not in sesssion");
		return FALSE;
	}
	return TRUE;
}

gboolean										
agent_remote_control_reconnect(AgentObject* self)
{
	if (self->state == ON_SESSION_OFF_REMOTE)
	{
		if (session_initialize(self))
		{
			self->state = ON_SESSION;
			return TRUE;
		}
		else
			return FALSE;
	}
	else
	{
		g_printerr("not in session state");
		return;
	}
}

gboolean
agent_register_settled(AgentObject* self)
{
	if (self->state != SLAVE_REGISTERING)
	{
		g_printerr("not in registering state");
		return FALSE;
	}
	else
	{
		GError** err;
		do
		{
			
			self->state = AGENT_OPEN;
			g_thread_try_new("information update",
				(GThreadFunc*)update_device_with_host, self, err);
			if (err != NULL)
			{
				g_printerr("failed to create thread");
			}
		} while (err == NULL);
		return TRUE;
	}
}

gboolean										
agent_command_line_passing(AgentObject* self,
	gchar* command)
{
	
}

gboolean										
agent_add_local_nas_storage(AgentObject* self,
	LocalStorage* storage)
{

}

gboolean										
agent_send_message(AgentObject* self,
	Message* message)
{
	send_message(self, message);
}

