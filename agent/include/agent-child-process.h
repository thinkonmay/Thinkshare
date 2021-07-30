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
																 gchar** command_array,
																 ChildHandleFunc func,
																 AgentObject* agent);

/// <summary>
/// 
/// </summary>
/// <param name="buffer"></param>
/// <param name="agent"></param>
void				command_line_output_handle					(gchar* buffer,
																 AgentObject* agent);


#endif