#include <glib.h>
#include <logging.h>
#include <gio/gio.h>
#include <string.h>

#include <message-form.h>



void
time_stamp(GFileOutputStream* stream,
            gchar* log)
{
    GTimeVal current_time;
	g_get_current_time(&current_time);
	gchar* iso_time = g_time_val_to_iso8601(&current_time);

    Message* json_output = json_object_new();

    json_object_set_string_member(json_output,"EmitTime",iso_time);
    json_object_set_string_member(json_output,"LogContent",log);

    gchar* timebuffer = get_string_from_json_object(json_output);
    g_printerr(timebuffer);
    g_free(timebuffer);
    // g_output_stream_write(stream, timebuffer, strlen(timebuffer),NULL,NULL);
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