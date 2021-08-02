#include <session-core-logging.h>
#include <glib.h>
#include <session-core.h>



void
write_to_log_file(SessionCore* core,
                  gchar* text)
{
    GFile* log = session_core_get_log_file(core);

    GFileOutputStream* output_stream = 
        g_file_append_to(log,G_FILE_CREATE_NONE,NULL,NULL);

    strcat(text, "\n");

    g_output_stream_write(output_stream, text, strlen(text),
    NULL,NULL);
    
    return;
}                  