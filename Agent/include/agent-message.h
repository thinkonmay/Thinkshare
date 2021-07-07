
#ifndef __AGENT_MESSAGE_H__
#define __AGENT_MESSAGE_H__

#include <agent-type.h>


Message*             message_init              ( gint from,
                                                 gint to,
                                                 gint opcode,
                                                 gpointer data,
                                                 gint data_size);

void                session_qoe_init            (SessionQoE* qoe,
				                                 gint frame_rate,
				                                 gint screen_width,
		                                		 gint screen_height,
		                                		 gint bitrate);

void                session_information_init	(Session* session,
	                                    		 gint session_slave_id,
	                                    		 gchar* signalling_url,
	                                    		 SessionQoE* qoe,
	                                    		 gboolean client_offer,
		                                         gchar* stun_server);
/// <summary>
/// all message from agent-ipc and agent-socket are handled here
/// </summary>
/// <param name="conn"></param>
/// <param name="type"></param>
/// <param name="message"></param>
/// <param name="self"></param>
void				on_agent_message			(AgentObject* self,
												 gchar* data);
#endif