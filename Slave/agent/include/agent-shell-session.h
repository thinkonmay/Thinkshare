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
/// <param name="process_id"></param>
/// <returns></returns>
gchar*				shell_output_map							(gint process_id);

/// <summary>
/// 
/// </summary>
gchar*				shell_script_map							(gint process_id);


void				initialize_shell_session					(AgentObject* agent,
                    										     gchar* data_string);

#endif


