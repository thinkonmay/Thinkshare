#pragma once
#ifndef __SHAREDMEMORY_H__
#define __SHAREDMEMORY_H__ 

#include <glib.h>
#include <agent-object.h>
#include <agent-type.h>




gboolean 			send_message_through_shared_memory				(AgentObject* self,
																	Message* message);

gboolean			session_initialize   							(AgentObject* object);

gboolean 			session_terminate   							(AgentObject* object);

gboolean			remote_control_disconnect						(AgentObject* object);

gboolean			remote_control_reconnect						(AgentObject* object);

gboolean 			send_message_through_shared_memory				(AgentObject* object,
                                 									 gint destination,
	                            								     Message* message,
                                									 gint message_size);
																	 
#endif