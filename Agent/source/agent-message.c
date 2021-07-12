#include <agent-message.h>
#include <glib.h>
#include <agent-socket.h>
#include <agent-ipc.h>
#include <json-glib/json-glib.h>
#include <agent-object.h>
#include <glib.h>
#include <agent-type.h>
#include <agent-state-open.h>
#include <agent-state.h>

#include <string.h>


/// <summary>
/// SessionQoE (quality of experience) of the remote control session
/// (included QoE controller)
/// </summary>
struct _SessionQoE
{
	gint screen_height;
	gint screen_width;
	gint framerate;
	gint bitrate;
};

/// <summary>
/// session related information:
/// session slave id, signalling url, ...
/// </summary>
struct _Session
{
	gint SessionSlaveID;
	gchar* signalling_url;
	SessionQoE* qoe;
	gboolean client_offer;
	gchar* stun_server;
};







void
session_qoe_init(SessionQoE* qoe,
				 gint frame_rate,
				 gint screen_width,
				 gint screen_height,
				 gint bitrate)
{
	qoe = malloc(sizeof(SessionQoE));

	qoe->bitrate = bitrate;
	qoe->framerate = frame_rate;
	qoe->screen_height = screen_height;
	qoe->screen_width = screen_width;
}

void
session_information_init(Session* session,
			 gint session_slave_id,
			 gchar* signalling_url,
			 SessionQoE* qoe,
			 gboolean client_offer,
			 gchar* stun_server)
{


	session = malloc(sizeof(Session));

	session->SessionSlaveID = session_slave_id;
	session->client_offer = client_offer;
	session->stun_server = stun_server;
	session->qoe = qoe;
	session->signalling_url = signalling_url;
}

Message* 
get_json_from_session(Session* session)
{

	JsonObject* object = json_object_new();
	json_object_set_int_member(object, "session-slave-id", session->SessionSlaveID);
	json_object_set_string_member(object, "signalling-url", session->signalling_url);
	json_object_set_boolean_member(object, "client-offer", session->client_offer);
	json_object_set_string_member(object, "stun-server", session->stun_server);

	JsonObject* qoe;
	json_object_set_int_member(object, "framerate", session->qoe->framerate);
	json_object_set_int_member(object, "bitrate", session->qoe->bitrate);
	json_object_set_int_member(object, "screen_height", session->qoe->screen_height);
	json_object_set_int_member(object, "screen_width", session->qoe->screen_width);


	json_object_set_member(object, "session-qoe", qoe);

	return object;

}










Message*
message_init(Module from,
             Module to,
             Opcode opcode,
             Message* data)
{
	Message* object = json_object_new();
	gchar* data_string = get_string_from_json_object(data);
	gchar* data_string_ = g_strndup( data_string,sizeof(data_string) );

	json_object_set_int_member		(object, "from",	from);
	json_object_set_int_member		(object, "to",		to);
	json_object_set_int_member		(object, "opcode",	opcode);
	if(data == NULL)
		return object;
	else
	{
		json_object_set_string_member	(object, "data", data_string_);
		return object;
	}
}




void
send_message(AgentObject* self,
             Message* message)
{
	Module to = json_object_get_int_member(message, "to");

	switch (to)
	{
	case HOST_MODULE:
	    send_message_to_host(self, 
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

/// <summary>
/// (PRIVATE function)
/// get_session_information from host message, 
/// used to set session information
/// </summary>
/// <param name="object"></param>
/// <returns></returns>
Session*
get_session_information_from_message(Message* object)
{
	Session* session;
	SessionQoE* qoe;

	JsonObject* session_object = json_object_get_member(object, "data");
	gint SessionSlaveID = json_object_get_int_member(session_object, "SessionSlaveID");
	gchar* signalling_url = json_object_get_string_member(session_object, "SignallingURL");
	gchar* client_offer = json_object_get_boolean_member(session_object, "ClientOffer");
	gchar* stun_server = json_object_get_string_member(session_object, "StunServer");

	JsonObject* qoe_object = json_object_get_member(session_object, "SessionQoE");
	gint screen_width = json_object_get_int_member(qoe_object, "ScreenWidth");
	gint screen_height = json_object_get_int_member(qoe_object, "ScreenHeight");
	gint framerate = json_object_get_int_member(qoe_object, "FrameRate");
	gint bitrate = json_object_get_int_member(qoe_object, "Bitrate");

	session_qoe_init(qoe, framerate,
		screen_width, screen_height, bitrate);

	session_information_init(session, SessionSlaveID,
		signalling_url, qoe, client_offer, stun_server);
	return session;
}



void
on_agent_message(AgentObject* agent,
				 gchar* data)
{

	JsonNode* root;
	JsonObject* object, * json_data;

	JsonParser* parser = json_parser_new();
	json_parser_load_from_data(parser, data, -1, NULL);

	root = json_parser_get_root(parser);
	object = json_node_get_object(root);

	Module     from =	json_object_get_int_member(object, "from");
	Module     to =		json_object_get_int_member(object, "to");
	Opcode     opcode =	json_object_get_int_member(object, "opcode");
	gchar*	   data_string =   json_object_get_string_member(object, "data");

	if (data_string != NULL)
	{
		JsonParser* parser = json_parser_new();
		json_parser_load_from_data(parser, data_string, -1, NULL);

		JsonNode* root = json_parser_get_root(parser);
		json_data = json_node_get_object(root);
	}

	if (to == AGENT_MODULE)
	{
		if (from == HOST_MODULE)
		{
			if (opcode == SLAVE_ACCEPTED)
			{
				AgentState* open_state = transition_to_on_open_state();
				agent_set_state(agent, open_state);

				GError** err;
				g_thread_try_new("information update",
					(GThreadFunc*)update_device_with_host, agent, err);
				if (err != NULL)
				{
					g_printerr("failed to create thread");
				}
				return;
			}
			else if (opcode == REJECT_SLAVE)
			{
				agent_finalize(agent);
				return;
			}
			else if (opcode == DENY_SLAVE)
			{
				agent_finalize(agent);
			}
			else
			{
				switch (opcode)
				{
				case SESSION_INITIALIZE:
				{
					Session* session = 
						get_session_information_from_message(json_data);
					agent_set_session(agent, session);

					agent_session_initialize(agent);
				}
				case SESSION_TERMINATE:
					agent_session_terminate(agent);
				case RECONNECT_REMOTE_CONTROL:
					agent_remote_control_reconnect(agent);
				case DISCONNECT_REMOTE_CONTROL:
					agent_remote_control_disconnect(agent);
				}
			}

		}
		else if(from == CORE_MODULE)
		{
			Message* msg;
			switch (opcode)
			{
			case SESSION_INFORMATION_REQUEST:
			{
				Session* session = agent_get_session(agent);
				Message* message = get_json_from_session(session);
				msg = message_init(AGENT_MODULE, CORE_MODULE, SESSION_INFORMATION, message);
			}
			}
			agent_send_message(agent, msg);
		}
	}
	else
	{
		agent_send_message(agent, object);
	}
}





