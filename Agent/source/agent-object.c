﻿#include <agent-object.h>

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
#include <agent-state-unregistered.h>

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
/// update device thread function,
/// invoke during agent object initialization
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










AgentObject*
agent_new(gchar* Host_URL)
{
	AgentObject* agent = malloc(sizeof(AgentObject));

	AgentState* unregistered = transition_to_unregistered_state();
	agent->state = unregistered;


	agent->ipc =				initialize_ipc(agent);
	agent->socket =				initialize_socket(agent,Host_URL);
	agent->device_information = get_device_information();
	agent->device_state =		get_device_state();

	for (gint i = 0; i < CMD_MAX; i++)
	{
		agent->cmd[i] = create_new_command_line_process();
	}

	agent_connect_to_host(agent);

	SECURITY_ATTRIBUTES attr;
	attr.nLength = sizeof(SECURITY_ATTRIBUTES);
	attr.bInheritHandle = TRUE;
	attr.lpSecurityDescriptor = NULL;

	HANDLE mutex = CreateMutex(&attr, FALSE, NULL);
	agent->state_mutex = &mutex;

	g_thread_new("update-device", 
		(GThreadFunc)update_device, agent);
	g_thread_new("handle-commandline", 
		(GThreadFunc)handle_command_line_thread, agent);



	agent->loop = g_main_loop_new(NULL, FALSE);
	g_main_loop_run(agent->loop);
	g_main_loop_unref(agent->loop);
	
	return agent;
}




void
agent_finalize(AgentObject* self)
{
	socket_close(self->socket);

	agent_session_terminate(self);

	if (self->loop)
	{
		g_main_loop_quit(self->loop);
	}
}



void
agent_send_command_line(AgentObject* self, 
						gchar* command, 
						gint order)
{
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

