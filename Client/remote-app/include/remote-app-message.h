#include <remote-app-type.h>
#include <remote-app.h>

#include <module.h>
#include <opcode.h>
#include <message-form.h>



/// <summary>
/// perform sending message to other module
/// </summary>
/// <param name="self"></param>
/// <param name="message"></param>
/// <returns></returns>
void				send_message			(RemoteApp* self,
											 Message* message);
	
/// <summary>
/// GATEWAY function, handle all message
/// </summary>
/// <param name="core"></param>
/// <param name="data"></param>
void				remote_app_on_message(RemoteApp* core,
											gchar* data);

