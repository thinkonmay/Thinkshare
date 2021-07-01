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
gboolean			send_command_line_to_window						(AgentObject* self,
																	gchar* command);



gboolean			add_nas											(AgentObject* self);

#endif


