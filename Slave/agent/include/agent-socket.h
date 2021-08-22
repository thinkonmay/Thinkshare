#pragma once
#ifndef __SOCKET_H__
#define __SOCKET_H__


#include <glib.h>
#include <libsoup/soup.h>
#include <json-glib-1.0/json-glib/json-glib.h>
#include <agent-object.h>










/// <summary>
/// (ATOMIC function)
/// handle message received from host over websocket, 
/// the received message should be parse using agent_on_message 
/// </summary>
/// <param name="conn"></param>
/// <param name="type"></param>
/// <param name="message">(TEXT only) received message</param>
/// <param name="user_data"></param>
void									on_server_message					(SoupWebsocketConnection* conn,
    																		 SoupWebsocketDataType type,
    																		 GBytes* message,
   																			 AgentObject* user_data);

/// <summary>
/// register slave device with host, 
/// provide slave information 
/// included cpu, gpu , ram capacity and os
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
gboolean                                register_with_host                  (AgentObject* self);


/// <summary>
/// (ATOMIC function)
/// send data to host in form of json object, should not use directly, 
/// using agent_send_message method instead
/// </summary>
/// <param name="self"></param>
/// <param name="message"></param>
void 									send_message_to_host				(AgentObject* self,
                                                                             gchar* data);

/// <summary>
/// connect to host asynchronously,
/// this function create new soup message and session,
/// wrap soup_websocket_connect_async function
/// </summary>
/// <param name="self"></param>
void									connect_to_host_async				(AgentObject* self);






/// <summary>
/// (THREAD FUNTION)
/// iretationally update the state of slave device to host,
/// thread stop when agent state is not 
/// </summary>
/// <param name="data"></param>
/// <returns></returns>
gpointer                                update_device_with_host             (AgentObject* data);


/// <summary>
/// Initialize socket, this include assign host url for socket 
/// </summary>
/// <param name="host_url"></param>
/// <returns>Socket correspond to host url</returns>
Socket*                                 initialize_socket                   (AgentObject* agent);

/// <summary>
/// 
/// </summary>
/// <param name="socket"></param>
void                                    socket_close                        (Socket* socket);




/*START get-set function for Socket*/

/// <summary>
/// get SoupWebsocketConnection for the socket
/// </summary>
/// <param name="socket"></param>
/// <returns></returns>
SoupWebsocketConnection*                socket_get_connection               (Socket* socket);

/// <summary>
/// set a host url for a specific socket
/// </summary>
/// <param name="socket"></param>
/// <param name="Host_Url"></param>
void                                    socket_set_host_url                 (Socket* socket,
                                                                             gchar* Host_Url);
/// <summary>
/// get host url from Socket
/// </summary>
/// <param name="socket"></param>
/// <returns></returns>
gchar*                                  socket_get_host_url                 (Socket* socket);

/*END get-set function for Socket*/

#endif