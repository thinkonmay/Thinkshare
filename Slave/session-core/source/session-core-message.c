#include <session-core-data-channel.h>
#include <session-core-message.h>
#include <session-core-type.h>
#include <session-core.h>
#include <session-core-ipc.h>
#include <module.h>
#include <opcode.h>

#include <glib.h>
#include <glib-object.h>
#include <gst/webrtc/webrtc.h>
#include <gst/webrtc/webrtc_fwd.h>



#include <stdio.h>








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

	Module     from =		json_object_get_int_member(object, "From");
	Module     to =			json_object_get_int_member(object, "To");
	Opcode     opcode =		json_object_get_int_member(object, "Opcode");
	gchar* data_string =	json_object_get_string_member(object, "Data");

	if (data_string != NULL)
	{
		JsonParser* parser = json_parser_new();
		json_parser_load_from_data(parser, data_string, -1, NULL);

		JsonNode* root = json_parser_get_root(parser);
		json_data = json_node_get_object(root);
	}

	if (to == CORE_MODULE)
	{
		if (from == AGENT_MODULE)
		{
		}
		else if (from == LOADER_MODULE)
		{
			switch (opcode)
			{

			}
		}
		else if (from == HOST_MODULE)
		{

		}
		else if (from == CLIENT_MODULE)
		{
			QoE* qoe = session_core_get_qoe(core);
			switch (opcode)
			{
			case QOE_REPORT:
				qoe_update_quality(qoe, 1,
					json_object_get_int_member(json_data, "FrameRate"),
					json_object_get_int_member(json_data, "AudioLatency"),
					json_object_get_int_member(json_data, "VideoLatency"),
					json_object_get_int_member(json_data, "AudioBitrate"), 
					json_object_get_int_member(json_data, "VideoBitrate"),
					json_object_get_int_member(json_data, "TotalBandwidth"),
					json_object_get_int_member(json_data, "PacketsLost"));
				break;
			default:
				break;
			}
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
	gint to = json_object_get_int_member(message, "To");

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
		send_message_to_agent(self, &string_data, strlen(string_data));
	case LOADER_MODULE:
		send_message_to_agent(self, &string_data, strlen(string_data));
	case AGENT_MODULE:
		send_message_to_agent(self, &string_data, strlen(string_data));
	}
}



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
		return object;
	else
	{
		gchar* data_string = get_string_from_json_object(data);
		gchar* data_string_ = g_strndup(data_string, strlen(data_string));
		g_free(data_string);

		json_object_set_string_member(object, "Data", data_string_);
		return object;
	}
}
