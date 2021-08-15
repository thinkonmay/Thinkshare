#pragma once

#ifndef __AGENT_CMD_H__
#define __AGENT_CMD_H__

#include <glib.h>
#include <agent-object.h>



/// <summary>
/// 
/// </summary>
/// <param name="position"></param>
/// <param name="agent"></param>
/// <param name="first_command"></param>
void				create_new_cmd_process						(AgentObject* agent, 
																gint position,
																gchar* first_command);

#endif


