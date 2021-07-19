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
gboolean			send_command_line								(CommandLine* self,			
																	 gchar* buffer);

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
CommandLine*		create_new_command_line_process					(void);


/// <summary>
/// 
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
gboolean			add_nas											(AgentObject* self);

/// <summary>
/// 
/// </summary>
/// <param name="data"></param>
/// <returns></returns>
void			handle_command_line_thread						(GTask* task,
																 gpointer source_object,
																 gpointer agent,
																 GCancellable* cancellable);

#endif


