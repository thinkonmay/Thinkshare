#include <glib.h>
#include <logging.h>
#include <gio/gio.h>
#include <string.h>

#include <Windows.h>



void
time_stamp(GFileOutputStream* stream,
            gchar* log)
{

    guint64 time = g_get_real_time();

    gchar timebuffer[1000];
    ZeroMemory(&timebuffer, 1000);

    strcat(timebuffer, "[");
    itoa(time,timebuffer+1,10);
    strcat(timebuffer, "]   :    ");

    strcat(timebuffer, log);  


    strcat(timebuffer, "\n");
    g_print(timebuffer);
    g_output_stream_write(stream, timebuffer, strlen(timebuffer),NULL,NULL);
}



void
write_to_log_file(gchar* file_name,
                  gchar* text)
{

    GFile* log = g_file_parse_name(file_name);

    GFileOutputStream* output_stream = 
        g_file_append_to(log,G_FILE_CREATE_NONE,NULL,NULL);

    time_stamp(output_stream,text);   
    return;
}                  