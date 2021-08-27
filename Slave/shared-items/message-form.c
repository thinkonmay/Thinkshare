#include "message-form.h"



Message*
message_init(Module from,
			Module to,
			Opcode opcode,
			Message* data)
{
	Message* object = json_object_new();

	json_object_set_int_member(object, "From", from);
	json_object_set_int_member(object, "To", to);
	json_object_set_int_member(object, "Opcode", opcode);


    gchar* data_string = get_string_from_json_object(data);

    json_object_set_string_member(object, "Data", data_string);
    return object;
}



gchar*
get_string_from_json_object(JsonObject* object)
{
    JsonNode* root;
    JsonGenerator* generator;
    gchar* text;

    /* Make it the root node */
    root =      json_node_init_object(json_node_alloc(), object);
    generator = json_generator_new();
    json_generator_set_root(generator, root);
    text =      json_generator_to_data(generator, NULL);

    /* Release everything */
    g_object_unref(generator);
    json_node_free(root);
    return text;
}


Message*
get_json_object_from_string(gchar* string, 
                            GError** error)
{
    JsonNode* root;
	JsonObject* json_data;

	JsonParser* parser = json_parser_new();
	json_parser_load_from_data(parser, string, -1, error);
	root = json_parser_get_root(parser);  
    
    return json_node_get_object(root);
}