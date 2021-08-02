#include <agent-message.h>
#include <glib.h>
#include <agent-socket.h>
#include <agent-session-initializer.h>
#include <json-glib/json-glib.h>
#include <agent-object.h>
#include <glib.h>
#include <agent-type.h>
#include <agent-state-open.h>
#include <agent-state.h>
#include <agent-cmd.h>
#include <agent-child-process.h>

#include <string.h>






Message*
message_init(Module from,
             Module to,
             Opcode opcode,
             Message* data)
{
	Message* object = json_object_new();
	gchar* data_string = get_string_from_json_object(data);
	gchar* data_string_ = g_strndup( data_string,sizeof(data_string) );

	json_object_set_int_member		(object, "From",	from);
	json_object_set_int_member		(object, "To",		to);
	json_object_set_int_member		(object, "Opcode",	opcode);
	if(data == NULL)
		return object;
	else
	{
		json_object_set_string_member	(object, "Data", data_string_);
		return object;
	}
}




void
send_message(AgentObject* self,
             Message* message)
{
	Module to = json_object_get_int_member(message, "To");

	switch (to)
	{
	case HOST_MODULE:
	    agent_send_message_to_host(self, 
			get_string_from_json_object(message));
	case CORE_MODULE:
		agent_send_message_to_session_core(self,
			get_string_from_json_object(message));
	case LOADER_MODULE:
		agent_send_message_to_session_loader(self,
			get_string_from_json_object(message));
	case CLIENT_MODULE:
		agent_send_message_to_session_core(self,
			get_string_from_json_object(message));
	}
}

static void
setup_slave_device(AgentObject* agent, gchar* slave)
{

	AgentState* open_state = transition_to_on_open_state();
	agent_set_state(agent, open_state);
	GFile* file = agent_get_slave_id(agent);
	
	if(g_file_replace_contents(file, 
		slave, sizeof(slave),NULL,TRUE, G_FILE_COPY_NONE,NULL, NULL, NULL))
		g_print("set %d as SlaveID\n", atoi(slave));

//	g_thread_new("information update",
//		(GThreadFunc*)update_device_with_host, agent);
}

void
on_agent_message(AgentObject* agent,
				 gchar* data)
{
	static gboolean initialized = FALSE;
	static GFileOutputStream* stream;
	if (!initialized)
	{
		GFile* log_file = 
			g_file_parse_name("C:\\ThinkMay\\AgentMessageLog.txt");


		stream = 
			g_file_append_to(log_file, G_FILE_CREATE_NONE, NULL, NULL);
	}
	g_output_stream_write(stream, data, sizeof(data), NULL, NULL);


	JsonNode* root;
	JsonObject* object, * json_data;

	JsonParser* parser = json_parser_new();
	json_parser_load_from_data(parser, data, -1, NULL);
	root = json_parser_get_root(parser);

	Module from, to;
	Opcode opcode;
	gchar* data_string;

	if(JSON_NODE_TYPE(root) == JSON_NODE_OBJECT)
	{
		object = json_node_get_object(root);
		from = json_object_get_int_member(object, "From");
		to = json_object_get_int_member(object, "To");
		opcode = json_object_get_int_member(object, "Opcode");
		data_string = json_object_get_string_member(object, "Data");
	}
	else
	{
		return;
	}




	JsonParser* parser_ = json_parser_new();
	json_parser_load_from_data(parser_, data_string, -1, NULL);
	JsonNode* root_ = json_parser_get_root(parser);

	if (JSON_NODE_TYPE(root_) == JSON_NODE_OBJECT)
	{
		json_data = json_node_get_object(root_);
	}

	if (to == AGENT_MODULE)
	{
		if (from == HOST_MODULE)
		{
			if (opcode == SLAVE_ACCEPTED)
			{
				setup_slave_device(agent, data_string);
			}
			else if (opcode == SESSION_INITIALIZE)
			{
				GFile* handle = agent_get_session(agent);

				g_file_replace_contents(handle, data_string,sizeof(data_string),
					NULL,FALSE,G_FILE_CREATE_NONE,NULL,NULL, NULL,NULL);

				agent_session_initialize(agent);
			}
			else if (opcode == NEW_COMMAND_LINE_SESSION)
			{
				create_new_cmd_process(
					json_object_get_int_member(json_data, "Order"), agent,
						json_object_get_string_member(json_data, "CommandLine"));
			}
			else if (opcode == END_COMMAND_LINE_SESSION)
			{
				close_child_process(
					agent_get_child_process(agent,
						json_object_get_int_member(json_data, "Order")));
			}
			else if (opcode == COMMAND_LINE_FORWARD)
			{
				agent_send_command_line(agent,
					json_object_get_int_member(json_data, "Order"),
						json_object_get_int_member(json_data, "Command"));
			}
			else if (opcode == DENY_SLAVE){
				agent_finalize(agent);}
			else if (opcode == REJECT_SLAVE) {
				agent_finalize(agent);}
			else if (opcode == SESSION_TERMINATE) {
				agent_session_terminate(agent);}
			else if (opcode == RECONNECT_REMOTE_CONTROL) {
				agent_remote_control_reconnect(agent);}
			else if (opcode == DISCONNECT_REMOTE_CONTROL) {
				agent_remote_control_disconnect(agent);}
		}
		else if(from == CORE_MODULE)
		{
			switch (opcode)
			{
				case EXIT_CODE_REPORT:
					agent_session_terminate(agent);
			}
		}
	}
	else
	{
		agent_send_message(agent, object);
	}
}





