#pragma once
#include "Framework.h"
#include "Variable.h"

gchar*
get_string_from_json_object(JsonObject* object);

void
connect_to_host_async(void);

void
on_server_connected(SoupSession* session,
    GAsyncResult* res,
    SoupMessage* msg);

void
on_server_closed(SoupWebsocketConnection* conn G_GNUC_UNUSED,
    gpointer user_data G_GNUC_UNUSED);

void
on_server_message(SoupWebsocketConnection* conn,
    SoupWebsocketDataType type,
    GBytes* message,
    gpointer user_data);

gboolean
register_with_server(void)