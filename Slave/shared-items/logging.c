#include <glib.h>
#include <logging.h>
#include <gio/gio.h>
#include <string.h>



void
time_stamp(GFileOutputStream* stream,
            gchar* log)
{

    guint64 time = g_get_real_time();

    gchar timebuffer[100] = { NULL };
    itoa(time,timebuffer,10);

    strcat(&timebuffer, &log);  

    
    g_output_stream_write(stream, timebuffer, strlen(timebuffer),NULL,NULL);
}



void
write_to_log_file(gchar* file_name,
                  gchar* text)
{
    g_print(text);
    g_print("\n");
    GFile* log = g_file_parse_name(file_name);

    GFileOutputStream* output_stream = 
        g_file_append_to(log,G_FILE_CREATE_NONE,NULL,NULL);

    strcat(text, "\n");
    time_stamp(output_stream,text);   
    return;
}                  