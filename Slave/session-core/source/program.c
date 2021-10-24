#include <session-core.h>
#include <session-core-type.h>
#include <general-constant.h>

#include <gst/gst.h>
#include <glib-2.0/glib.h>

#define GST_USE_UNSTABLE_API

#define GST_DEBUG       4

int
main(int argc, char* argv[])
{
    thinkmay_init(argv[0],21);

    gst_init(&argc,&argv);
    SessionCore* core = session_core_initialize();
    return 0;
}
