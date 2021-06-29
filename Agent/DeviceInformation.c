#include "DeviceInformation.h"

gchar*
get_string_from_json_object(JsonObject* object)
{
	JsonNode* root;
	JsonGenerator* generator;
	gchar* text;

	/* Make it the root node */
	root = json_node_init_object(json_node_alloc(), object);
	generator = json_generator_new();
	json_generator_set_root(generator, root);
	text = json_generator_to_data(generator, NULL);

	/* Release everything */
	g_object_unref(generator);
	json_node_free(root);
	return text;
}
/// <summary>
/// TRUE if success
/// FALSE of fail
/// </summary>
/// <param name="self"></param>
/// <param name="command"></param>
/// <returns></returns>
gboolean
send_command_line_to_window(AgentObject* self,
	gchar* command)
{
	

}


/// <summary>
/// update device state function, run in infinite loop in a separate thread
/// </summary>
/// <param name="self"></param>
void
update_slave_state_with_host(AgentObject* self)
{


	while (TRUE)
	{
		JsonObject* json;
		DeviceState* state = update_slave_state();

		json_object_set_int_member(json, "gpu-usage", state->gpu_usage);
		json_object_set_int_member(json, "ram-usage", state->ram_usage);
		json_object_set_int_member(json, "cpu-usage", state->cpu_usage);

		Message message;
		message.from = AGENT;
		message.to = HOST;
		message.opcode = UPDATE_SLAVE_STATE;
		message.data = get_string_from_json_object(json);
		
		send_message(self, &message);
	}
}

DeviceState*
update_slave_state(void)
{

}
