#include <remote-app-data-channel.h>
#include <remote-app-message.h>
#include <remote-app-type.h>
#include <remote-app.h>

#include <module.h>
#include <opcode.h>

#include <glib.h>
#include <glib-object.h>
#include <gst/webrtc/webrtc.h>
#include <gst/webrtc/webrtc_fwd.h>



#include <stdio.h>
#include <general-constant.h>
#include <module.h>







/// <summary>
/// responsible for message from agent and session loader.
/// attached with "on-message" signal of SharedMemoryLink object,
/// refer to on_link_connected callback function
/// </summary>
/// <param name="self"></param>
/// <param name="msg"></param>
/// <param name="user_data"></param>
void
remote_app_on_message(RemoteApp* core,
						 gchar* data)
{
	QoE* qoe = remote_app_get_qoe(core);


    GError* error = NULL;
    JsonObject* object = get_json_object_from_string(data,&error);
	if(!error == NULL || object == NULL) {return;}

	gint		from =		json_object_get_int_member(object, "From");
	gint 	to =			json_object_get_int_member(object, "To");
	Opcode     opcode =		json_object_get_int_member(object, "Opcode");
	gchar* data_string =	json_object_get_string_member(object, "Data");
}




void
send_message(RemoteApp* self,
			 JsonObject* message)
{
	gint to = json_object_get_int_member(message, "To");

	gchar* string_data = get_string_from_json_object(message);

	switch(to)
	{
	}
}



