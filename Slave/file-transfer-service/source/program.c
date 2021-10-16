#include <file-transfer.h>
#include <file-transfer-type.h>

#include <gst/gst.h>
#include <glib-2.0/glib.h>

#ifndef GST_USE_UNSTABLE_API
#define GST_USE_UNSTABLE_API
#endif

static gboolean stats = FALSE;
static gchar signalling_url[50] = {0};
static gchar turn[50] = {0};
static gint session_id = 0;
static gchar file[50] = {0};

static GOptionEntry entries[] = {
  {"sessionid", 0, 0, G_OPTION_ARG_INT, &session_id,
      "String ID of the peer to connect to", "ID"},
  {"signalling", 0, 0, G_OPTION_ARG_STRING, &signalling_url,
      "Signalling server to connect to", "URL"},
  {"path", 0, 0, G_OPTION_ARG_STRING, &file,
      "Request that the peer generate the offer and we'll answer", "Path"},
  {"turn", 0, 0, G_OPTION_ARG_STRING, &turn,
      "Request that the peer generate the offer and we'll answer", "URL"},
  {NULL},
};

int
main (int argc, char *argv[])
{
  GOptionContext *context;
  GError *error = NULL;

  context = g_option_context_new ("- gstreamer webrtc sendrecv demo");
  g_option_context_add_main_entries (context, entries, NULL);
  g_option_context_add_group (context, gst_init_get_option_group ());
  if (!g_option_context_parse (context, &argc, &argv, &error)) {
    g_printerr ("Error initializing: %s\n", error->message);
    return -1;
  }

  FileTransferService* core = file_transfer_initialize(signalling_url,session_id,file,turn);
  return 0;
}
