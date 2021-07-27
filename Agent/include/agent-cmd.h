#pragma once

#ifndef __AGENT_CMD_H__
#define __AGENT_CMD_H__

#include <glib.h>
#include <agent-object.h>

/// <summary>
/// </summary>
/// <param name="self"></param>
/// <param name="command"></param>
/// <returns></returns>
gboolean			send_message_to_child_process				(ChildProcess* self,
																 gchar* buffer,
																 gint size);

/// <summary>
/// 
/// </summary>
/// <param name="cmd"></param>
void				close_command_line_process						(CommandLine* cmd);



/// <summary>
/// 
/// </summary>
/// <param name=""></param>
/// <returns></returns>
ChildProcess*		create_new_child_process					(gchar* process_name,
																 gint id,		
																 gchar* command_array,
																 ChildHandleFunc func,
																 AgentObject* agent);

/// <summary>
/// 
/// </summary>
/// <param name="buffer"></param>
/// <param name="agent"></param>
void				command_line_output_handle					(gchar* buffer,
																 AgentObject* agent);

/// <summary>
/// 
/// </summary>
/// <param name="position"></param>
/// <param name="agent"></param>
/// <param name="first_command"></param>
void				create_new_cmd_process						(gint position,
																AgentObject* agent,
																gchar* first_command);

#endif


