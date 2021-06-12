#pragma once
/*
 * Copyright/Licensing information.
 */

 /* inclusion guard */
#ifndef __NAMEDPIPE_CONNECTION_H__
#define __NAMEDPIPE_CONNECTION_H__

#include <glib-object.h>
#include <Windows.h>
/*
 * Potentially, include other headers on which this header depends.
 */

G_BEGIN_DECLS

/*
 * Type declaration.
 */
#define NAMEDPIPE_TYPE_CONNECTION (namedpipe_connection_get_type ())

G_DECLARE_FINAL_TYPE (NamedpipeConnection, namedpipe_connection, NAMEDPIPE, CONNECTION, GObject)

NamedpipeConnection* 
namedpipe_connection_new(GType message_data_type);

void 
namedpipe_connection_send_message(NamedpipeConnection* self, NamedpipeDataType type, GBytes* message);




G_END_DECLS



#endif  __NAMEDPIPE_CONNECTION_H__ 