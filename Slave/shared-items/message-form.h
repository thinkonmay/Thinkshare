#ifndef __MESSAGE_FORM_H__
#define __MESSAGE_FORM_H__

#include <glib.h>
#include <json-glib/json-glib.h>

#include <opcode.h>
#include <module.h>

typedef 				JsonObject				Message;

/// <summary>
/// initialize message (json_object) 
/// with given from, destination, opcode and data (Message datatype)
/// </summary>
/// <param name="from"></param>
/// <param name="to"></param>
/// <param name="opcode"></param>
/// <param name="data"></param>
/// <param name="data_size"></param>
/// <returns></returns>
Message*            message_init                (Module from,
                                                 Module to,
                                                 Opcode opcode,
                                                 Message* data);

/// <summary>
/// convert json object to string to save to file or send to other module
/// </summary>
/// <param name="object"></param>
/// <returns></returns>
gchar*              get_string_from_json_object (JsonObject* object);

/// <summary>
/// convert string to json object
/// </summary>
/// <param name="string">string to convert to json</param>
/// <param name="error">error pointer to receive any error might occour, should be compare to null after call this function</param>
/// <returns></returns>
Message*            get_json_object_from_string (gchar* string,
                                                 GError** error);

/// <summary>
/// create empty messsage to send to host (message with " " data)
/// </summary>
/// <param name="from"></param>
/// <param name="to"></param>
/// <param name="opcode"></param>
/// <returns></returns>
Message*            empty_message_init          (Module from,
			                                    Module to,
			                                    Opcode opcode);


Message*            get_json_object_from_file   (gchar* file_name,
                                                 GError** error);

#endif