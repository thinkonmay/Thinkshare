﻿#include <agent-object.h>
#include <agent-type.h>
#include <agent-session-initializer.h>
#include <agent-socket.h>
#include <agent-device.h>
#include <agent-message.h>
#include <agent-shell-session.h>
#include <agent-device.h>
#include <agent-state.h>
#include <agent-child-process.h>
#include <agent-state-unregistered.h>
#include <agent-file-transfer-service.h>



#include <Windows.h>
#include <stdio.h>


#include <child-process-constant.h>
#include <general-constant.h>
#include <logging.h>
#include <error-code.h>
#include <message-form.h>

/// <summary>
/// agent object 
/// </summary>
struct _AgentObject
{
	/// <summary>
	/// socket object
	/// </summary>
	Socket* socket;

	/// <summary>
	/// gmainloop will be run in agent process
	/// </summary>
	GMainLoop* loop;

	/// <summary>
	/// state of the slave device
	/// </summary>
	AgentState* state;

	/// <summary>
	/// Child process array, agent module are capable to create 
	/// new child process in admintrator privillege 
	/// and has full control of child process
	/// (include handle stdout, child process state)
	/// </summary>
	ChildProcess* child_process[MAX_CHILD_PROCESS];

	/// <summary>
	/// 
	/// </summary>
	FileTransferSession* file_transfer_session[MAX_FILE_TRANSFER_INSTANCE];

	/// <summary>
	/// 
	/// </summary>
	ShellSession* shell_session[MAX_POWERSHELL_INSTANCE];

	/// <summary>
	/// 
	/// </summary>
	RemoteSession* remote_session;
};











static AgentObject agent_declare = {0};


AgentObject*
agent_new(gchar* url)
{	
	//set initial state of agent as unregistered	
	AgentState* unregistered = transition_to_unregistered_state();
	agent_declare.state = unregistered;

	//g_thread_new("update device",(GThreadFunc)update_device, &agent);
	initialize_child_process_system(&agent_declare);
	initialize_file_transfer_service(&agent_declare);
	intialize_remote_session_service(&agent_declare);
	agent_declare.socket=initialize_socket(&agent_declare);
	
	// connect to host with given id
	agent_connect_to_host(&agent_declare);

	// start gmainloop, 
	agent_declare.loop = g_main_loop_new(NULL, FALSE);
	g_main_loop_run(agent_declare.loop);
	return NULL;
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
agent_report_error(AgentObject* self,
				   gchar* message)
{
	JsonParser* parser = json_parser_new();
	json_parser_load_from_file(parser, HOST_CONFIG_FILE,NULL);
	JsonNode* root = json_parser_get_root(parser);
	JsonObject* json = json_node_get_object(root);
	gint SlaveID = json_object_get_int_member(json,DEVICE_ID);


	JsonObject* obj = json_object_new();
	json_object_set_int_member(obj,
		"SlaveID",SlaveID);
	json_object_set_int_member(obj,
		"Module",AGENT_MODULE);	
	json_object_set_string_member(obj,
		"ErrorMessage",message);
		

	Message* msg = message_init(AGENT_MODULE,
		HOST_MODULE, ERROR_REPORT, obj);
	agent_send_message(self,msg);
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
agent_on_shell_process_terminate(AgentObject* self, gint process_id)
{
	self->state->on_shell_process_exit(self, process_id);
}

void
agent_send_message(AgentObject* self,
	Message* message)
{
	// write_to_log_file(AGENT_GENERAL_LOG, 
	// 	get_string_from_json_object(message));
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

void
agent_on_session_core_exit(AgentObject* self)
{
	self->state->on_session_core_exit(self);
}

gchar*
agent_get_current_state_string(AgentObject* self)
{
	return self->state->get_current_state();
}

void
agent_start_file_transfer(gchar* server_commmand)
{
	FileTransferSession* session = setup_file_transfer_session(server_commmand);
	start_file_transfer(session);
}





















/*START get-set function*/



/*START get-set function*/
Socket*
agent_get_socket(AgentObject* self)
{
	return self->socket;
}

void
agent_set_socket(AgentObject* self, Socket* socket)
{
	self->socket = socket;
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
agent_set_file_transfer_service(AgentObject* self,
	gint position,
	FileTransferSession* session)
{
	self->file_transfer_session[position] = session;
}


void
agent_set_main_loop(AgentObject* self,
	GMainLoop* loop)
{
	self->loop = loop;
}

GMainLoop*
agent_get_main_loop(AgentObject* self)
{
	return self->loop;
}


RemoteSession*
agent_get_remote_session(AgentObject* self)
{
	return self->remote_session;
}

void
agent_set_remote_session(AgentObject* self, 
						 RemoteSession* session)
{
	self->remote_session = session;
}
