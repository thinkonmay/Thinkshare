#include <session-core-type.h>
#include <session-core.h>

#include <module-code.h>
#include <opcode.h>
#include <message-form.h>



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

