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


gchar*              get_string_from_json_object (JsonObject* object);


Message*            get_json_object_from_string (gchar* string,
                                                 GError* error);

Message*            empty_message_init          (Module from,
			                                    Module to,
			                                    Opcode opcode);

#endif