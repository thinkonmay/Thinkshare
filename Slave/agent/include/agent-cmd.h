#pragma once

#ifndef __AGENT_CMD_H__
#define __AGENT_CMD_H__

#include <glib.h>
#include <agent-object.h>



/// <summary>
/// create new window process, new process created should be run under admin privillege,
/// server are able to send text to child process std input,
///	two common example of child process are cmd.exe and session-core.exe
/// </summary>
/// <param name="position"></param>
/// <param name="agent"></param>
/// <param name="first_command"></param>
void				create_new_cmd_process					(AgentObject* agent, 
															gint position);


/// <summary>
/// agent send message function, wrap around send_messsage function
/// </summary>
/// <param name="self"></param>
/// <param name="message"></param>
/// <returns></returns>
void				agent_send_command_line					(AgentObject* agent, 
															gchar* command, 
															gint order)



#endif


