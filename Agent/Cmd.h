#pragma once
#include "Framework.h"
#include "Object.h"
#include "agent-object.h"

/*
Bảo sẽ hoàn thiện commmand line parsing tại đây	
*/


/// <summary>
/// </summary>
/// <param name="self"></param>
/// <param name="command"></param>
/// <returns></returns>
gboolean
send_command_line_to_window(AgentObject* self,
	gchar* command);



gboolean
add_nas(AgentObject* self);


