#pragma once
#include "Framework.h"
#include "Object.h"

gchar*
get_string_from_json_object(JsonObject* object);

void
<<<<<<< Updated upstream
connect_to_host_async(void);

void
on_server_connected(SoupSession* session,
    GAsyncResult* res,
    SoupMessage* msg);

void
on_server_closed(SoupWebsocketConnection* conn G_GNUC_UNUSED,
    gpointer user_data G_GNUC_UNUSED);

void
=======
>>>>>>> Stashed changes
on_server_message(SoupWebsocketConnection* conn,
    SoupWebsocketDataType type,
    GBytes* message,
    gpointer user_data);

gboolean
<<<<<<< Updated upstream
register_with_server(void)
=======
register_with_host(AgentObject* self);

void
send_message_to_host(AgentObject* self,
    Message* message);
>>>>>>> Stashed changes
