#include <agent-message.h>
#include <agent-socket.h>
#include <agent-session-initializer.h>
#include <agent-object.h>
#include <agent-type.h>
#include <agent-state-open.h>
#include <agent-state.h>
#include <agent-shell-session.h>
#include <agent-child-process.h>

#include <general-constant.h>
#include <logging.h>
#include <error-code.h>
#include <message-form.h>


#include <glib.h>
#include <json-glib/json-glib.h>



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
	GError* error = NULL;
	json_parser_load_from_file(parser,SESSION_SLAVE_FILE,&error);
	if(!error == NULL)
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
	json_object_set_string_member	(new_session_config,"TurnConnection",		
		json_object_get_string_member(obj,"TurnConnection"));

	json_object_set_object_member(new_session_config,"QoE",qoe);
	gchar* buffer = get_string_from_json_object(new_session_config);

	GFile* config = g_file_parse_name(SESSION_SLAVE_FILE);

	if(!g_file_replace_contents(config,buffer,sizeof(buffer),NULL,
		TRUE,G_FILE_CREATE_NONE,NULL,NULL,&error))
	{
		agent_report_error(agent,ERROR_FILE_OPERATION);
	}	

	g_free(buffer);
	if(!error == NULL)
	{
		agent_report_error(agent,error->message);
	}

} 


void
on_agent_message(AgentObject* agent,
				 gchar* data)
{
	GError* error = NULL;
	Message* object = 		get_json_object_from_string(data,&error);
	if(!error == NULL || object == NULL || ! json_object_has_member(object,"Opcode")) {return;}

	gint	from = 			json_object_get_int_member(object, "From");
	gint	to = 			json_object_get_int_member(object, "To");
	gint	opcode = 		json_object_get_int_member(object, "Opcode");
	gchar*	data_string = 	json_object_get_string_member(object, "Data");
	

	if (to == AGENT_MODULE)
	{
		if (from == HOST_MODULE)
		{
			if (opcode == SLAVE_ACCEPTED){
				setup_slave_device(agent, data_string);	return;}
			else if (opcode == DENY_SLAVE){
				agent_finalize(agent); return;}
			else if (opcode == REJECT_SLAVE) {
				agent_finalize(agent); return;}
			else if (opcode == SESSION_TERMINATE){
				agent_session_terminate(agent); return;}
			else if (opcode == RECONNECT_REMOTE_CONTROL){
				agent_remote_control_reconnect(agent); return;}
			else if (opcode == DISCONNECT_REMOTE_CONTROL) {
				agent_remote_control_disconnect(agent); return;}
			else if (opcode == NEW_SHELL_SESSION){
				initialize_shell_session(agent,data_string); return;}
			else if (opcode == SESSION_INITIALIZE){
				setup_session(agent,data_string); return;}
			else if (opcode == FILE_TRANSFER_SERVICE){
				agent_start_file_transfer(data_string);
			}
		}
		else if(from == CORE_MODULE)
		{		
			Message* json_data = get_json_object_from_string(data_string,&error);
			if(!error == NULL || json_data == NULL) {return;}

			else if(opcode == CLIPBOARD_SERVICE){

			}else if(opcode == RESET_QOE){
				agent_reset_qoe(agent,json_data);
			}
			
		}
	}
	else
	{
		agent_send_message(agent, object);
	}
}





