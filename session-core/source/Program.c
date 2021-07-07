#include <session-core.h>
#include <session-core-type.h>

#include <gst\gst.h>
#include <glib-2.0\glib.h>
#define GST_USE_UNSTABLE_API
#define SIGNALLING_URL


gboolean
check_plugins()
{
    gboolean ret;
    GstPlugin* plugin;
    GstRegistry* registry;
    const gchar* needed[] = { "dx9screencapsrc",
    "nvh264enc","rtph264pay","pulsesrc",
    "opusencode","rtpopuspay","rtprtxqueue",
    "webrtcbin", NULL };

    registry = gst_registry_get();
    ret = TRUE;


    for (int i = 0; i < g_strv_length((gchar**)needed); i++) {
        plugin = gst_registry_find_plugin(registry, needed[i]);

        if (!plugin) 
{
            g_printerr("Required gstreamer plugin '%s' not found\n", needed[i]);
            ret = FALSE;
            continue;
        }
        gst_object_unref(plugin);
    }
    return ret;
}

int
main(int argc, char* argv[])
{

    
    if (!check_plugins())
    {
    }   


    gst_init(&argc,&argv);
    
    SessionCore* core = session_core_initialize(SIGNALLING_URL);
    return 0;
}
