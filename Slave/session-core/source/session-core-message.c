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
#include <general-constant.h>








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
	QoE* qoe = session_core_get_qoe(core);

	write_to_log_file(SESSION_CORE_NETWORK_LOG,data);

    GError* error = malloc(sizeof(GError));
    Message* object = get_json_object_from_string(data,&error);
	if(error != NULL || object == NULL) {return;}

	Module     from =		json_object_get_int_member(object, "From");
	Module     to =			json_object_get_int_member(object, "To");
	Opcode     opcode =		json_object_get_int_member(object, "Opcode");
	gchar* data_string =	json_object_get_string_member(object, "Data");

	if (to == CORE_MODULE)
	{
		if (from == CLIENT_MODULE)
		{
			if(opcode == QOE_REPORT)
			{
			    Message* json_data = get_json_object_from_string(data_string,&error);
				if(error != NULL || object == NULL) {return;}
				
				qoe_update_quality(qoe, 
					g_get_real_time(),
					json_object_get_int_member(json_data, "FrameRate"),
					json_object_get_int_member(json_data, "AudioLatency"),
					json_object_get_int_member(json_data, "VideoLatency"),
					json_object_get_int_member(json_data, "AudioBitrate"), 
					json_object_get_int_member(json_data, "VideoBitrate"),
					json_object_get_int_member(json_data, "TotalBandwidth"),
					json_object_get_int_member(json_data, "PacketsLost"));
			}
		}
		else if (from == AGENT_MODULE)
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




void
send_message(SessionCore* self,
			 Message* message)
{
	Module to = json_object_get_int_member(message, "To");

	gchar* string_data = get_string_from_json_object(message);
	write_to_log_file(SESSION_CORE_NETWORK_LOG,string_data);

	switch(to)
	{
		case CLIENT_MODULE:
		{
			WebRTCHub* hub = session_core_get_rtc_hub(self);
			GstWebRTCDataChannel* control = webrtc_hub_get_control_data_channel(hub);

			GBytes* byte = g_bytes_new(&string_data, sizeof(string_data));
			g_signal_emit_by_name(control, "send-data", byte);
			break;
		}
		/*write to std out stream if destination is not client module*/
		case HOST_MODULE:
		{	
			send_message_to_agent(self, string_data, strlen(string_data));
			break;
		}
		case LOADER_MODULE:
		{
			send_message_to_agent(self, string_data, strlen(string_data));
			break;
		}
		case AGENT_MODULE:
		{
			send_message_to_agent(self, string_data, strlen(string_data));
			break;
		}
	}
}



