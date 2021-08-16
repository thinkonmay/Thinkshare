#include <agent-message.h>
#include <agent-socket.h>
#include <agent-session-initializer.h>
#include <agent-object.h>
#include <agent-type.h>
#include <agent-state-open.h>
#include <agent-state.h>
#include <agent-cmd.h>
#include <agent-child-process.h>

#include <general-constant.h>
#include <logging.h>
#include <error-code.h>


#include <glib.h>
#include <json-glib/json-glib.h>






Message*
message_init(Module from,
	Module to,
	Opcode opcode,
	Message* data)
{
	Message* object = json_object_new();

	json_object_set_int_member(object, "From", from);
	json_object_set_int_member(object, "To", to);
	json_object_set_int_member(object, "Opcode", opcode);
	if (data == NULL)
	{
		return object;
	}
	else
	{
		gchar* data_string = get_string_from_json_object(data);
		json_object_set_string_member	(object, "Data", data_string);
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
		break;
	case CORE_MODULE:
		agent_send_message_to_session_core(self,
			get_string_from_json_object(message));
		break;
	case LOADER_MODULE:
		agent_send_message_to_session_loader(self,
			get_string_from_json_object(message));
		break;
	case CLIENT_MODULE:
		agent_send_message_to_session_core(self,
			get_string_from_json_object(message));
		break;
	}
}

static void
setup_slave_device(AgentObject* agent, gchar* slave)
{

	AgentState* open_state = transition_to_on_open_state();
	agent_set_state(agent, open_state);	
}

void
agent_reset_qoe(AgentObject* agent, JsonObject* qoe)
{
	JsonParser* parser;
	GError* error;
	json_parser_load_from_file(parser,SESSION_SLAVE_FILE,&error);
	if(error != NULL)
	{
		agent_report_error(agent, error->message);
	}

	JsonNode* root = json_parser_get_root(parser);
	JsonObject* obj = json_node_get_object(root);

	JsonObject* new_session_config = json_object_new();

	json_object_set_string_member	(new_session_config,"SignallingUrl",
		json_object_get_string_member(obj,"SignallingUrl"));
	json_object_set_int_member		(new_session_config,"SessionSlaveID",
		json_object_get_string_member(obj,"SessionSlaveID"));
	json_object_set_boolean_member	(new_session_config,"ClientOffer",	
		json_object_get_string_member(obj,"ClientOffer"));
	json_object_set_string_member	(new_session_config,"StunServer",		
		json_object_get_string_member(obj,"StunServer"));

	json_object_set_object_member(new_session_config,"QoE",qoe);
	gchar* buffer = get_string_from_json_object(new_session_config);

	GFile* config = g_file_parse_name(SESSION_SLAVE_FILE);

	if(!g_file_replace_contents(config,buffer,sizeof(buffer),NULL,
		TRUE,G_FILE_CREATE_NONE,NULL,NULL,&error))
	{
		agent_report_error(agent,ERROR_FILE_OPERATION);
	}	

	g_free(buffer);
	if(error != NULL)
	{
		agent_report_error(agent,error->message);
	}

} 


void
on_agent_message(AgentObject* agent,
				 gchar* data)
{
	write_to_log_file(AGENT_MESSAGE_LOG,data);

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
				GFile* file = g_file_parse_name(SESSION_SLAVE_FILE);

				if(!g_file_replace_contents(file, data_string,strlen(data_string),
					NULL,FALSE,G_FILE_CREATE_NONE,NULL,NULL, NULL,NULL))
				{
					agent_report_error(agent,ERROR_FILE_OPERATION);					
				}

				agent_session_initialize(agent);
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
			


			///message data with actual json object
			JsonParser* parser_ = json_parser_new();
			json_parser_load_from_data(parser_, data_string, -1, NULL);
			JsonNode* root_ = json_parser_get_root(parser_);
			json_data = json_node_get_object(root_);
					
			if (opcode == NEW_COMMAND_LINE_SESSION)
			{
				create_new_cmd_process(agent,
					json_object_get_int_member(json_data, "ProcessID"), 
						json_object_get_string_member(json_data, "CommandLine"));
			}
			else if (opcode == END_COMMAND_LINE_SESSION)
			{
				close_child_process(
					agent_get_child_process(agent,
						json_object_get_string_member(json_data, "ProcessID")));
			}
			else if (opcode == COMMAND_LINE_FORWARD)
			{
				agent_send_command_line(agent,
					json_object_get_string_member(json_data, "CommandLine"),
						json_object_get_int_member(json_data, "ProcessID"));
			}
		}
		else if(from == CORE_MODULE)
		{		
			JsonParser* parser_ = json_parser_new();
			json_parser_load_from_data(parser_, data_string, -1, NULL);
			JsonNode* root_ = json_parser_get_root(parser_);
			json_data = json_node_get_object(root_);

			if(FILE_TRANSFER_SERVICE){

			}else if(CLIPBOARD_SERVICE){

			}else if(RESET_QOE){
				agent_reset_qoe(agent,json_data);
			}
			
		}
	}
	else
	{
		agent_send_message(agent, object);
	}
}





