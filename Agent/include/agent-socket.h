#pragma once
#ifndef __SOCKET_H__
#define __SOCKET_H__


#include <glib.h>
#include <libsoup/soup.h>
#include <json-glib-1.0/json-glib/json-glib.h>
#include <agent-object.h>







/// <summary>
/// contain information about websocket socket with host
/// </summary>
struct _Socket
{
    SoupWebsocketConnection* ws;
    gchar* host_url;
};








gchar*									get_string_from_json_object			(JsonObject* object);




void									on_server_message					(SoupWebsocketConnection* conn,
    																		 SoupWebsocketDataType type,
    																		 GBytes* message,
   																			 AgentObject* user_data);




gboolean								register_with_host					(AgentObject* self);



void 									send_message_to_host				(AgentObject* self,
   																			 gint from,
  																			 gint to,
   																			 gint opcode,
   																			 JsonObject* data);


void									connect_to_host_async				(AgentObject* self);

void 									send_message_to_host				(Socket* socket,
                   															 gint from,
                     														 gint to,
                    														 gint opcode,
                    														 GValue* data);

#endif