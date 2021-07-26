#include <Windows.h>
#include "session-core-type.h"
#include "session-core.h"



/// <summary>
/// initialize ipc include std pipe and input reading thread
/// </summary>
/// <param name="core"></param>
/// <returns></returns>
IPC*					ipc_initialize					(SessionCore* core);


/// <summary>
/// send message to agent over standard input and output
/// </summary>
/// <param name="self"></param>
/// <param name="buffer"></param>
void					send_message_to_agent			(SessionCore* self,
														 gchar** buffer,
														 gint size);

