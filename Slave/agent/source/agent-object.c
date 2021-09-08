#include <agent-object.h>
#include <agent-type.h>
#include <agent-session-initializer.h>
#include <agent-socket.h>
#include <agent-device.h>
#include <agent-message.h>
#include <agent-cmd.h>
#include <agent-device.h>
#include <agent-state.h>
#include <agent-state-unregistered.h>



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
	ChildProcess* child_process[LAST_CHILD_PROCESS];
};














AgentObject*
agent_new(gchar* url)
{
	// allocate heap for agent object
	static AgentObject agent;
	ZeroMemory(&agent, sizeof(AgentObject));
	
	//set initial state of agent as unregistered	
	AgentState* unregistered = transition_to_unregistered_state();
	agent.state = unregistered;

	//g_thread_new("update device",(GThreadFunc)update_device, &agent);
	initialize_child_process_system(&agent);
	agent.socket=initialize_socket(&agent);
	
	// connect to host with given id
	agent_connect_to_host(&agent);

	// start gmainloop, 
	agent.loop = g_main_loop_new(NULL, FALSE);
	g_main_loop_run(agent.loop);
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
agent_on_cmd_process_terminate(AgentObject* self, gint ProcessID)
{
	self->state->on_commandline_exit(self, ProcessID);
}

void
agent_send_message(AgentObject* self,
	Message* message)
{
	write_to_log_file(AGENT_GENERAL_LOG, 
		get_string_from_json_object(message));
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

/*START get-set function*/



/*START get-set function*/

