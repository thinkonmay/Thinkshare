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
#include <agent-state-unregistered.h>

#define CMD_MAX 8

/// <summary>
/// agent object 
/// </summary>
struct _AgentObject
{
	CommandLine* cmd[CMD_MAX];

	GFile* device;

	GFile* session;

	GFile* SlaveID;

	DeviceState* device_state;

	DeviceInformation* device_information;

	IPC* ipc;

};














AgentObject*
agent_new(gchar* Host_URL)
{
	AgentObject* agent = malloc(sizeof(AgentObject));

	AgentState* unregistered = transition_to_unregistered_state();
	agent->state = unregistered;



	agent.device = g_file_parse_name("C:\\ThinkMay\\DeviceLog.txt");

	agent.session = g_file_parse_name("C:\\ThinkMay\\Session.txt");

	agent.SlaveID = g_file_parse_name("C:\\ThinkMay\\SlaveID.txt");

	SECURITY_ATTRIBUTES attr;
	attr.nLength = sizeof(SECURITY_ATTRIBUTES);
	attr.bInheritHandle = TRUE;
	attr.lpSecurityDescriptor = NULL;


	g_thread_new("update device", (GThreadFunc)update_device, &agent);

	initialize_socket(&agent,url);
	agent.loop = g_main_loop_new(NULL, FALSE);
	g_main_loop_run(agent.loop);

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
	send_message_to_child_process(self->child_process[order], command,strlen(command)*sizeof(gchar));
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

GFile*
agent_get_device_log(AgentObject* self)
{
	return self->device_information;
}


GFile*
agent_get_session(AgentObject* self)
{
	return self->session;
}

GFile*
agent_get_slave_id(AgentObject* self)
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


ChildProcess*
agent_get_child_process(AgentObject* self, gint position)
{
	return self->child_process[position];
}

void
agent_set_child_process(AgentObject* self,
	gint postion,
	ChildProcess* process)
{
	self->child_process[postion] = process;
}


void
agent_object_set_main_loop(AgentObject* self,
						   GMainLoop* loop)
{
	self->loop = loop;
}

GMainLoop*
agent_get_main_loop(AgentObject* self)
{
	self->cmd;
}

/*START get-set function*/

