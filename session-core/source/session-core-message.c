#include <session-core-message.h>
#include <stdio.h>
#include <session-core-type.h>
#include <session-core.h>
#include <session-core-ipc.h>

#include <glib.h>
#include <glib-object.h>
#include <gst/webrtc/webrtc.h>
#include <gst/webrtc/webrtc_fwd.h>

#include <session-core-data-channel.h>









/// <summary>
/// responsible for message from agent and session loader.
/// attached with "on-message" signal of SharedMemoryLink object,
/// refer to on_link_connected callback function
/// </summary>
/// <param name="self"></param>
/// <param name="msg"></param>
/// <param name="user_data"></param>
void
session_core_on_message(SessionCore* core,
						 gchar* data)
{

	JsonNode* root;
	JsonObject* object, * json_data;

	JsonParser* parser = json_parser_new();
	json_parser_load_from_data(parser, data, -1, NULL);

	root = json_parser_get_root(parser);
	object = json_node_get_object(root);

	Module     from = json_object_get_int_member(object, "from");
	Module     to = json_object_get_int_member(object, "to");
	Opcode     opcode = json_object_get_int_member(object, "opcode");
	gchar* data_string = json_object_get_string_member(object, "data");

	if (data_string != NULL)
	{
		JsonParser* parser = json_parser_new();
		json_parser_load_from_data(parser, data_string, -1, NULL);

		JsonNode* root = json_parser_get_root(parser);
		json_data = json_node_get_object(root);
	}

	if (to = CORE_MODULE)
	{
		if (from == AGENT_MODULE)
		{
			switch (opcode)
			{
			case SESSION_INFORMATION:
			{
				if (session_core_get_state(core) != SESSION_CORE_INITIALIZING)
					session_core_finalize(core, CORE_STATE_CONFLICT);


				JsonNode* session_root;
				JsonParser* session_parser = json_parser_new();
				json_parser_load_from_data(parser, data_string, -1, NULL);
				session_root = json_parser_get_root(parser);
				Message* session_object = json_node_get_object(root);

				Session* session = get_session_information_from_message(session_object);

				if (session == NULL)
					session_core_finalize(core, CORRUPTED_SESSION_INFORMATION);

				session_core_setup_session(core, session);
			}
			}
		}
		else if (from == LOADER_MODULE)
		{
			switch (opcode)
			{
			default:
				break;
			}
		}
		else if (from == HOST_MODULE)
		{

		}
		else 
		{
			report_session_core_error(core, UNKNOWN_MESSAGE);
		}
	}
	else 
	{
		session_core_send_message(core, object);
	}

}

gchar*
get_string_from_json_object(JsonObject* object)
{
	JsonNode* root;
	JsonGenerator* generator;
	gchar* text;

	/* Make it the root node */
	root = json_node_init_object(json_node_alloc(), object);
	generator = json_generator_new();
	json_generator_set_root(generator, root);
	text = json_generator_to_data(generator, NULL);

	/* Release everything */
	g_object_unref(generator);
	json_node_free(root);
	return text;
}




void
send_message(SessionCore* self,
			 Message* message)
{
	gint to = json_object_get_int_member(message, "to");

	gchar* string_data = get_string_from_json_object(message);
	switch(to)
	{
	case CLIENT_MODULE:
	{
		WebRTCHub* hub = session_core_get_rtc_hub(self);
		GstWebRTCDataChannel* control = webrtc_hub_get_control_data_channel(hub);

		GBytes* byte = g_bytes_new(&string_data, sizeof(string_data));

		g_signal_emit_by_name(control, "send-data", byte);
	}
	/*write to std out stream if destination is not client module*/
	case HOST_MODULE:
		send_message_to_agent(self,string_data);
	case LOADER_MODULE:
		send_message_to_agent(self, string_data);
	case AGENT_MODULE:
		send_message_to_agent(self, string_data);
	}
}



Message*
message_init(Module from,
			Module to,
			Opcode opcode,
			Message* data)
{
	Message* object = json_object_new();

	json_object_set_int_member(object, "from", from);
	json_object_set_int_member(object, "to", to);
	json_object_set_int_member(object, "opcode", opcode);
	if (data == NULL)
		return object;
	else
	{
		gchar* data_string = get_string_from_json_object(data);
		gchar* data_string_ = g_strndup(data_string, sizeof(data_string));
		g_free(data_string);

		json_object_set_string_member(object, "data", data_string_);
		return object;
	}
}
