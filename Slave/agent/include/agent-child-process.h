#ifndef __AGENT_CHILD_PROCESS_H__
#define __AGENT_CHILD_PROCESS_H__
#include "agent-type.h"
#include "glib.h"

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
void				close_child_process							(ChildProcess* proc);



/// <summary>
/// 
/// </summary>
/// <param name=""></param>
/// <returns></returns>
ChildProcess*		create_new_child_process					(gchar* process_name,
																 gint id,
																 gchar* parsed_command,
																 ChildStdHandle func,
																 ChildStateHandle handler,
																 AgentObject* agent);

/// <summary>
/// 
/// </summary>
/// <param name="buffer"></param>
/// <param name="agent"></param>
void				command_line_output_handle					(gchar* buffer,
																 AgentObject* agent);


void				initialize_child_process_system				(AgentObject* agent);


/// <summary>
/// get state of a specific child process, return true if it is running,
/// otherwise, return false
/// </summary>
/// <param name="buffer"></param>
/// <param name="agent"></param>
gboolean			get_current_child_process_state				(AgentObject* agent,
																 gint order);



gint 				get_child_process_id						(ChildProcess* process);
#endif