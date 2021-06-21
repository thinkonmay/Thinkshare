#pragma once
#include "Framework.h"
#include "Object.h"
#include "agent-object.h"

gchar*
get_string_from_json_object(JsonObject* object);

void
connect_to_host_async(AgentObject* self);

void
on_server_connected(SoupSession* session,
    GAsyncResult* res,
    AgentObject* self);

void
on_server_closed(SoupWebsocketConnection* conn G_GNUC_UNUSED,
    AgentObject* user_data G_GNUC_UNUSED);

void
on_server_message(SoupWebsocketConnection* conn,
    SoupWebsocketDataType type,
    GBytes* message,
    AgentObject* user_data);

gboolean
register_with_server(AgentObject* self);

void
send_message_to_host(AgentObject* self,
    Location from,
    Location to,
    Opcode opcode,
    gpointer data);