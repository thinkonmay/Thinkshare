#ifndef __AGENT_CHILD_PROCESS_H__
#define __AGENT_CHILD_PROCESS_H__
#include "agent-type.h"
#include "glib.h"

#include <Windows.h>
/// <summary>
/// send message to child process
/// </summary>
/// <param name="self">child process to send message</param>
/// <param name="buffer">content send to child process through stdin</param>
/// <param name="size">size of content</param>
/// <returns></returns>
gboolean			send_message_to_child_process				(ChildProcess* self,
																 gchar* buffer,
																 gint size);

/// <summary>
/// close child process
/// </summary>
/// <param name="proc">child process which will be close</param>
void				close_child_process							(ChildProcess* proc);



/// <summary>
/// create new child process with given binary and handle function
/// </summary>
/// <param name="binary_name">name of binary</param>
/// <returns></returns>
ChildProcess*		create_new_child_process					(gchar* binary_name,
																 gint id,
																 gchar* parsed_command,
																 ChildStdHandle func,
																 ChildStateHandle handler,
																 AgentObject* agent);

/// <summary>
/// 
/// </summary>
/// <returns></returns>
ChildProcess* 		get_available_child_process					();

/// <summary>
/// 
/// </summary>
/// <returns></returns>
ChildProcess* 		get_available_file_transceiver				();


/// <summary>
/// 
/// </summary>
/// <param name="agent"></param>
void				initialize_child_process_system				(AgentObject* agent);


/// <summary>
/// get state of a specific child process, 
/// return true if it is running,
/// otherwise, return false
/// </summary>
/// <param name="buffer"></param>
/// <param name="agent"></param>
gboolean			get_current_child_process_state				(AgentObject* agent,
																 gint order);


/// <summary>
/// 
/// </summary>
/// <param name="process"></param>
/// <returns></returns>
gint 				get_child_process_id						(ChildProcess* process);
#endif