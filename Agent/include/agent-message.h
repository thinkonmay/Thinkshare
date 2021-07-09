
#ifndef __AGENT_MESSAGE_H__
#define __AGENT_MESSAGE_H__

#include <agent-type.h>



/// <summary>
/// initialize message (json_object) 
/// with given from, destination, opcode and data (Message datatype)
/// </summary>
/// <param name="from"></param>
/// <param name="to"></param>
/// <param name="opcode"></param>
/// <param name="data"></param>
/// <param name="data_size"></param>
/// <returns></returns>
Message*             message_init              ( Module from,
                                                 Module to,
                                                 Opcode opcode,
                                                 Message* data);

/// <summary>
/// initialize session qoe (quality of experience)
/// </summary>
/// <param name="qoe"></param>
/// <param name="frame_rate"></param>
/// <param name="screen_width"></param>
/// <param name="screen_height"></param>
/// <param name="bitrate"></param>
void                session_qoe_init            (SessionQoE* qoe,
				                                 gint frame_rate,
				                                 gint screen_width,
		                                		 gint screen_height,
		                                		 gint bitrate);

/// <summary>
/// initialize session information directly from member, 
/// should be used by agent_on_message while parsing json message from host
/// </summary>
/// <param name="session"></param>
/// <param name="session_slave_id"></param>
/// <param name="signalling_url"></param>
/// <param name="qoe"></param>
/// <param name="client_offer"></param>
/// <param name="stun_server"></param>
void                session_information_init	(Session* session,
	                                    		 gint session_slave_id,
	                                    		 gchar* signalling_url,
	                                    		 SessionQoE* qoe,
	                                    		 gboolean client_offer,
		                                         gchar* stun_server);
/// <summary>
/// all message from agent-ipc and agent-socket are handled here,
/// if agent is destination, parsing the opcode and data,
/// if not, send it to the correct destination
/// </summary>
/// <param name="conn"></param>
/// <param name="type"></param>
/// <param name="message"></param>
/// <param name="self"></param>
void				on_agent_message			(AgentObject* self,
												 gchar* data);
#endif