#include <session-core.h>
#include <session-core-type.h>

#include <gst\gst.h>
#include <glib-2.0\glib.h>
#define GST_USE_UNSTABLE_API



int
main(int argc, char* argv[])
{
    gst_init(&argc,&argv);
    SessionCore* core = session_core_initialize();
    return 0;
}

