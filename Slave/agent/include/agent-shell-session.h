/// <summary>
/// @file agent-shell-sesion.h
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-23
/// 
/// @copyright Copyright (c) 2021
/// 
#ifndef __AGENT_SHELL_SESSION_H__
#define __AGENT_SHELL_SESSION_H__

#include <glib.h>
#include <agent-object.h>






/// <summary>
/// </summary>
/// 
/// <param name="session"></param>
/// <returns></returns>
gchar*				shell_session_get_output					(gint process_id);

/// <summary>
/// </summary>
/// 
/// <param name="session"></param>
/// <returns></returns>
gchar*				shell_session_get_script					(gint process_id);


/// <summary>
/// </summary>
/// 
/// <param name="agent"></param>
/// <param name="data_string"></param>
void				initialize_shell_session					(AgentObject* agent,
                    										     gchar* data_string);

/// <summary>
/// </summary>
/// 
/// <param name="process_id"></param>
/// <returns></returns>
gint				shell_session_get_id						(gint process_id);

/// <summary>
/// </summary>
/// 
/// <param name="process_id"></param>
/// <returns></returns>
gint				shell_session_get_model						(gint process_id);

/// <summary>
/// 
/// </summary>
/// <param name="agent"></param>
/// <param name="process_id"></param>
void				report_shell_session						(AgentObject* agent,
                    											 gint process_id);

#endif


