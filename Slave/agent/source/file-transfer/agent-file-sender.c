#include <agent-file-sender.h>
#include <glib-2.0/glib.h>


#include <child-process-constant.h>



struct _FileTransceiver
{
    gchar* input_file;

    ChildProcess* file_transfer_service;

    gchar* signalling_url;

    gint session_slave_id;

    gchar* turn_connection;
};

static FileTransceiver transceiver[MAX_FILE_TRANSFER_INSTANCE] = {0};



