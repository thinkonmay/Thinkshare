#include <file-transfer.h>
#include <file-transfer-type.h>

#include <gst\gst.h>
#include <glib-2.0\glib.h>

#ifndef GST_USE_UNSTABLE_API
#define GST_USE_UNSTABLE_API
#endif


int
main(int argc, char* argv[])
{
    gst_init(&argc,&argv);
    FileTransferSvc* core = file_transfer_initialize();
    return 0;
}
