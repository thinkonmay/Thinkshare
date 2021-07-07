#pragma once
#ifndef __SOCKET_H__
#define __SOCKET_H__


#include <glib.h>
#include <libsoup/soup.h>
#include <json-glib-1.0/json-glib/json-glib.h>
#include <agent-object.h>








gchar*									get_string_from_json_object			(JsonObject* object);




void									on_server_message					(SoupWebsocketConnection* conn,
    																		 SoupWebsocketDataType type,
    																		 GBytes* message,
   																			 AgentObject* user_data);

/*register slave device with host, provide slave information*/
gboolean                                register_with_host                  (AgentObject* self);


/// <summary>
/// send data to host in form of json object, should not use directly, 
/// using agent_send_message method instead
/// </summary>
/// <param name="self"></param>
/// <param name="message"></param>
void 									send_message_to_host				(AgentObject* self,
                                                                             gchar* data);


void									connect_to_host_async				(AgentObject* self);


gchar*                                  socket_get_host_url                 (Socket* socket);


SoupWebsocketConnection*                socket_get_connection               (Socket* socket);

/// <summary>
/// (THREAD FUNTION)
/// iretationally update the state of slave device to host,
/// thread stop when agent state is not 
/// </summary>
/// <param name="data"></param>
/// <returns></returns>
gpointer                                update_device_with_host             (AgentObject* data);

void                                    socket_set_host_url                 (Socket* socket,
                                                                             gchar* Host_Url);

Socket*                                 initialize_socket                   (gchar* host_url);

#endif