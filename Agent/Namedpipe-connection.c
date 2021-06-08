#pragma once
#include "Namedpipe-connection.h"

G_DEFINE_TYPE_WITH_PRIVATE (NamedpipeConnection, namedpipe_connection, G_TYPE_OBJECT)


enum
{
	SIGNAL_RECEIVED_MESSAGE_STRING,
	SIGNAL_RECEIVED_MESSAGE_BYTES,
	SIGNAL_SEND_MESSAGE_STRING,
	SIGNAL_SEND_MESSAGE_BYTES,
	SIGNAL_READ_ERROR,
	NUM_SIGNALS
};
static guint signals[NUM_SIGNALS] = { 0, };
enum
{
	PROP_PIPE_NAME_STRING_IN,
	PROP_PIPE_NAME_STRING_OUT,
	PROP_PIPE_NAME_BYTE_IN,
	PROP_PIPE_NAME_BYTE_OUT,
	PROP_MAX_BUFFER_SIZE,
	LAST_PROP
};
static GParamSpec* namedpipe_connection_properties[LAST_PROP] = { NULL, };

typedef struct
{
	GSource* input_pipe;
	GSource* output_pipe;

	GBytes* buffer_bytes_in;
	gchar* buffer_string_in;
	GBytes* buffer_bytes_out;
	gchar* buffer_string_out;

	gchar* pipe_name_in_string;
	gchar* pipe_name_out_string;
	gchar* pipe_name_in_byte;
	gchar* pipe_name_out_byte;

	gint buffer_size;

	HANDLE file_handler_string_in;
	HANDLE file_handler_string_out;
	HANDLE file_handler_byte_in;
	HANDLE file_handler_byte_out;

} NamedpipeConnectionPrivate;

static void
namedpipe_connection_get_property(GObject *gobject,
	gint prop_id,
	GValue *value,
	GParamSpec *pspec)
{
	NamedpipeConnection* self = NAMEDPIPE_CONNECTION(gobject);
	NamedpipeConnectionPrivate* priv = namedpipe_connection_get_instance_private(gobject);
	switch (prop_id) {
	case PROP_PIPE_NAME_STRING_IN:
		g_value_set_string(value, priv-> pipe_name_in_string);
		break;
	case PROP_PIPE_NAME_STRING_OUT:
		g_value_set_string(value, priv->pipe_name_out_string);
		break;
	case PROP_PIPE_NAME_BYTE_IN:
		g_value_set_string(value, priv->pipe_name_in_byte);
		break;
	case PROP_PIPE_NAME_BYTE_OUT:
		g_value_set_string(value, priv->pipe_name_out_byte);
		break;
	case PROP_MAX_BUFFER_SIZE:
		g_value_set_int(value,priv->buffer_size);
		break;
	}
}
static void
namedpipe_connection_set_property(GObject* gobject,
	gint prop_id,
	GValue* value,
	GParamSpec* pspec)
{
	NamedpipeConnection* self = NAMEDPIPE_CONNECTION(gobject);
	NamedpipeConnectionPrivate* priv = namedpipe_connection_get_instance_private(gobject);
	switch (prop_id) {
	case PROP_PIPE_NAME_STRING_IN:
		g_return_if_fail(priv->pipe_name_in_string == NULL);
		priv->pipe_name_in_string = g_value_dup_string(value);
	case PROP_PIPE_NAME_STRING_OUT:
		g_return_if_fail(priv->pipe_name_out_string == NULL);
		priv->pipe_name_out_string = g_value_dup_string(value);
	case PROP_PIPE_NAME_BYTE_IN:
		g_return_if_fail(priv->pipe_name_in_byte == NULL);
		priv->pipe_name_in_string = g_value_dup_string(value);
	case PROP_PIPE_NAME_BYTE_OUT:
		g_return_if_fail(priv->pipe_name_out_byte == NULL);
		priv->pipe_name_out_string = g_value_dup_string(value);
	case PROP_MAX_BUFFER_SIZE:
		g_return_if_fail(priv->buffer_size == NULL);
		priv->pipe_name_out_string = g_value_get_int(value);
	}
}
static void
namedpipe_connection_setup_file_handler(NamedpipeConnection* self)
{
	NamedpipeConnectionPrivate* priv = namedpipe_connection_get_instance_private(self);

	priv->file_handler_byte_in = CreateFileW(priv->pipe_name_in_byte,
		GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_WRITE,
		NULL,
		OPEN_EXISTING, 0, NULL);

	priv->file_handler_byte_out = CreateFileW(priv->pipe_name_out_byte,
		GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_WRITE,
		NULL,
		OPEN_EXISTING, 0, NULL);

	priv->file_handler_string_in = CreateFileW(priv->pipe_name_in_string,
		GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_WRITE,
		NULL,
		OPEN_EXISTING, 0, NULL);

	priv->file_handler_string_out = CreateFileW(priv->pipe_name_out_string,
		GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_WRITE,
		NULL,
		OPEN_EXISTING, 0, NULL);
}

static void
namedpipe_connection_class_init(NamedpipeConnectionClass* klass)
{
	GObjectClass* object_class = G_OBJECT_CLASS(klass);

	/*connect named pipe function*/
	object_class->constructed = namedpipe_connection_constructed;
	object_class->set_property = namedpipe_connection_set_property;
	object_class->get_property = namedpipe_connection_get_property;
	object_class->dispose = namedpipe_connection_dispose;
	object_class->finalize = namedpipe_connection_finalize;

	/*create object properties*/
	namedpipe_connection_properties[PROP_MAX_BUFFER_SIZE] =
		g_param_spec_int("maxbuffersize",
			"Maxbuffersize",
			"Maximum buffer size of pipe.",
			0, 1000, 100, G_PARAM_READWRITE);

	namedpipe_connection_properties[PROP_PIPE_NAME_BYTE_IN] =
		g_param_spec_string("pipename-in",
			"Pipe name in",
			"Name of input pipe.",
			L"\\\\.\\pipe\\pipea", G_PARAM_READWRITE);

	namedpipe_connection_properties[PROP_PIPE_NAME_BYTE_OUT] =
		g_param_spec_string("pipename-out",
			"Pipe name out",
			"Name of output pipe.",
			L"\\\\.\\pipe\\pipeb", G_PARAM_READWRITE);
	namedpipe_connection_properties[PROP_PIPE_NAME_STRING_IN] =
		g_param_spec_string("pipename-in",
			"Pipe name in",
			"Name of input pipe.",
			L"\\\\.\\pipe\\pipec", G_PARAM_READWRITE);

	namedpipe_connection_properties[PROP_PIPE_NAME_STRING_OUT] =
		g_param_spec_string("pipename-out",
			"Pipe name out",
			"Name of output pipe.",
			L"\\\\.\\pipe\\piped", G_PARAM_READWRITE);

	g_object_class_install_properties(object_class, LAST_PROP, namedpipe_connection_properties);


	/*create signal for object*/
	signals[SIGNAL_RECEIVED_MESSAGE_BYTES] =
		g_signal_new("on-message-byte",
			G_SIGNAL_RUN_FIRST,
			0,
			NULL, NULL, NULL,
			G_TYPE_NONE, 1, G_TYPE_BYTES);


	signals[SIGNAL_RECEIVED_MESSAGE_STRING] =
		g_signal_new("on-message-byte",
			G_SIGNAL_RUN_FIRST,
			0,
			NULL, NULL, NULL,
			G_TYPE_NONE, 1, G_TYPE_STRING);


	signals[SIGNAL_SEND_MESSAGE_BYTES] =
		g_signal_new("send-message-byte",
			G_SIGNAL_RUN_FIRST,
			0,
			NULL, NULL, NULL,
			G_TYPE_NONE, 1, G_TYPE_BYTES);


	signals[SIGNAL_SEND_MESSAGE_STRING] =
		g_signal_new("send-message-byte",
			G_SIGNAL_RUN_FIRST,
			0,
			NULL, NULL, NULL,
			G_TYPE_NONE, 1, G_TYPE_STRING);

	signals[SIGNAL_READ_ERROR] =
		g_signal_new("read-error",
			G_SIGNAL_RUN_FIRST,
			0,
			NULL, NULL, NULL,
			G_TYPE_NONE, 0);


}

static void
namedpipe_connection_init(NamedpipeConnection* self)
{
	NamedpipeConnectionPrivate* priv = namedpipe_connection_get_instance_private(self);


}

static void 
namedpipe_connection_dispose(GObject* gobject)
{
	NamedpipeConnectionPrivate* priv = namedpipe_connection_get_instance_private(NAMEDPIPE_CONNECTION(gobject));
	g_free(&priv->pipe_name_in_string);
	g_free(&priv->pipe_name_out_string);

	G_OBJECT_CLASS(namedpipe_connection_parent_class)->dispose(gobject);

}

static void
namedpipe_connection_finalize(GObject* gobject)
{
	NamedpipeConnectionPrivate* priv = namedpipe_connection_get_instance_private(NAMEDPIPE_CONNECTION(gobject));

	g_free(priv->buffer_bytes_in);
	g_free(priv->buffer_bytes_out);
	g_free(priv->buffer_string_in);
	g_free(priv->buffer_string_out);
	g_free(priv->output_pipe);
	g_free(priv->input_pipe);
}

struct _Namedpipeconnection
{
	GObject parent_instance;



	/* Other members, including private data. */
};

NamedpipeConnection* 
namedpipe_connection_new(GType message_data_type)
{

	return g_object_new(NAMEDPIPE_CONNECTION,
		"message-data-type", message_data_type,
		NULL
	);
}
static void
namedpipe_connection_constructed(GObject* object)
{
	NamedpipeConnection* self = NAMEDPIPE_CONNECTION(object);
	NamedpipeConnectionPrivate* priv = namedpipe_connection_get_instace_private(self);
	G_OBJECT_CLASS(namedpipe_connection_parent_class)->constructed(object);



}


static void
process_incoming(NamedpipeConnection* self)
{
	while (process_message(self))
		;
}

static gboolean
process_message(NamedpipeConnection* self)
{
	NamedpipeConnectionPrivate* priv = namedpipe_connection_get_instace_private(self);

}

