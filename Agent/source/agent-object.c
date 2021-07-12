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
#include <agent-state.h>

#define CMD_MAX 8

/// <summary>
/// agent object 
/// </summary>
struct _AgentObject
{
	CommandLine* cmd[CMD_MAX];

	HANDLE state_mutex;

	DeviceState* device_state;

	DeviceInformation* device_information;

	IPC* ipc;

	Socket* socket;

	Session* session;

	GMainLoop* loop;

	AgentState* state;
};



/// <summary>
/// update device thread function, invoke during agent object initialization
/// </summary>
/// <param name="data"></param>
/// <returns></returns>
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
	}
	return NULL;
}









/// <summary>
/// constructed function, run after agent object has been initialized sucessfully,
/// run related thread and main loop
/// </summary>
/// <param name="self"></param>
void
agent_constructed(AgentObject* self)
{
	SECURITY_ATTRIBUTES attr;
	attr.nLength = sizeof(SECURITY_ATTRIBUTES);
	attr.bInheritHandle = TRUE;
	attr.lpSecurityDescriptor = NULL;

	HANDLE mutex = CreateMutex(&attr, FALSE, NULL);

	self->state_mutex = &mutex;

	g_thread_new("update-device", (GThreadFunc)update_device, self);
	g_thread_new("handle-commandline", (GThreadFunc)handle_command_line_thread, self);

	self->loop = g_main_loop_new(NULL, FALSE);

	g_main_loop_run(self->loop);
	g_main_loop_unref(self->loop);
}

AgentObject*
agent_new(gchar* Host_URL)
{
	AgentObject* agent = malloc(sizeof(AgentObject));
	agent->state = AGENT_NEW;


	agent->ipc=		initialize_ipc(agent);
	agent->socket = initialize_socket(agent,Host_URL);

	agent->device_information = get_device_information();
	agent->device_state =		get_device_state();
	agent->state = ATTEMP_TO_RECONNECT;

	for (gint i = 0; i < CMD_MAX; i++)
	{
		agent->cmd[i] = initialize_command_line();
	}


	agent_connect_to_host(agent);

	agent_constructed(agent);
	return agent;
}




void
agent_finalize(AgentObject* object)
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













gboolean
agent_create_new_command_line_process(AgentObject* self, gint order)
{
	if (self->cmd[order] == NULL)
	{
		return FALSE;
	}
	else
	{
		self->cmd[order] = create_new_command_line_process();

		if (self->cmd[order] == NULL)
			return FALSE;
	}
}

gboolean
agent_close_command_line_process(AgentObject* self, gint order)
{
	close_command_line_process(self->cmd[order]);
	return TRUE;
}

void
agent_send_command_line(AgentObject* self, 
						gchar* command, 
						gint order)
{
	if (self->cmd[order] != NULL)
	{
		agent_create_new_command_line_process(self, order);
	}
	send_command_line(self->cmd[order], command);
}










void
agent_register_with_host(AgentObject* self)
{
	self->state->register_to_host(self);
}


void
agent_connect_to_host(AgentObject* self)
{
	self->state->connect_to_host(self);
}


void
agent_send_message(AgentObject* self,
	Message* message)
{
	send_message(self, message);
}


void
agent_send_message_to_host(AgentObject* self,
	gchar* message)
{
	self->state->send_message_to_host(self, message);
}

void
agent_send_message_to_session_core(AgentObject* self,
	gchar* message)
{
	self->state->send_message_to_session_core(self, message);
}

void
agent_send_message_to_session_loader(AgentObject* self,
	gchar* message)
{
	self->state->send_message_to_session_loader(self, message);
}


void
agent_session_initialize(AgentObject* self)
{
	self->state->session_initialize(self);
}

void										
agent_session_terminate(AgentObject* self)
{
	self->state->session_terminate(self);
}

void										
agent_remote_control_disconnect(AgentObject* self)
{
	self->state->remote_control_disconnect(self);
}

void										
agent_remote_control_reconnect(AgentObject* self)
{
	self->state->remote_control_reconnect(self);
}
























/*START get-set function*/

IPC*
agent_get_ipc(AgentObject* self)
{
	return self->ipc;
}

Socket*
agent_get_socket(AgentObject* self)
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
agent_get_session(AgentObject* self)
{
	return self->session;
}

void
agent_set_session(AgentObject* self, Session* session)
{
	return self->session = session;
}

void
agent_set_state(AgentObject* object, AgentState* state)
{
	object->state = state;
}

AgentState*
agent_get_state(AgentObject* self)
{
	return self->state;
}

HANDLE*
agent_get_mutex_handle_ptr(AgentObject* self)
{
	return self->state_mutex;
}

CommandLine**
agent_get_command_line_array(AgentObject* self)
{
	self->cmd;
}

/*START get-set function*/

