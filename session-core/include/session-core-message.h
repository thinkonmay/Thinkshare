#include <session-core-type.h>
#include "session-core.h"



/// <summary>
/// perform sending message to other module
/// </summary>
/// <param name="self"></param>
/// <param name="message"></param>
/// <returns></returns>
void				send_message			(SessionCore* self,
											 Message* message);
	
/// <summary>
/// GATEWAY function, handle all message
/// </summary>
/// <param name="core"></param>
/// <param name="data"></param>
void				session_core_on_message(SessionCore* core,
											gchar* data);
/// <summary>
/// initialize message to send to other module
/// </summary>
/// <param name="from"></param>
/// <param name="to"></param>
/// <param name="opcode"></param>
/// <param name="data"></param>
/// <returns></returns>
Message*			message_init			(Module from,
											 Module to,
											 Opcode opcode,
											 Message* data);
