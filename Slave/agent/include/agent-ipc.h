#pragma once
#ifndef __SHAREDMEMORY_H__
#define __SHAREDMEMORY_H__ 

#include <glib.h>
#include <agent-object.h>
#include <agent-type.h>



/// <summary>
/// (ATOMIC function)
/// send message to session core using standard input output (stdio)
/// should only be used by send_message function
/// </summary>
/// <param name="self"></param>
/// <param name="buffer"></param>
/// <returns></returns>
gboolean			send_message_to_core							(AgentObject* self,
																	 gchar* buffer);
/// <summary>
/// (ATOMIC function)
/// send message to session loader using standard input output (stdio)
/// should only be used by send_message function
/// </summary>
/// <param name="self"></param>
/// <param name="buffer"></param>
/// <returns></returns>
gboolean			send_message_to_loader							(AgentObject* self,
																	 gchar* buffer);

/// <summary>
/// initialize session, include create std pipe and session core and session loader child process
/// should be invoked when there is a request from host
/// </summary>
/// <param name="object"></param>
/// <returns></returns>
gboolean			session_initialize   							(AgentObject* object);

/// <summary>
/// terminate session, included termination of two child process and cleaning some garbagge
/// and assign NULL value to session 
/// </summary>
/// <param name="object"></param>
/// <returns></returns>
gboolean 			session_terminate   							(AgentObject* object);

/// <summary>
/// disconnect remote control with user, unlike session terminate,
/// disconnect process do not assign NULL to agent->session
/// </summary>
/// <param name="object"></param>
/// <returns></returns>
gboolean			remote_control_disconnect						(AgentObject* object);

/// <summary>
/// initialize remote control based on session structure in agent_object
/// </summary>
/// <param name="object"></param>
/// <returns></returns>
gboolean			remote_control_reconnect						(AgentObject* object);


																	 
#endif