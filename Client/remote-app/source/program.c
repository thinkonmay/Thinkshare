#include <remote-app.h>
#include <remote-app-type.h>

#include <gst\gst.h>
#include <glib-2.0\glib.h>

#ifndef GST_USE_UNSTABLE_API
#define GST_USE_UNSTABLE_API
#endif

static gchar signalling_url[50] = "wss://signalling.thinkmay.net/Session";
static gchar turn[100] = "turn://thinkmaycoturn:thinkmaycoturn_password@turn:stun.thinkmay.net:3478";
static gint session_id = 0;
static gchar video_codec[50] = {0};
static gchar audio_codec[50] = {0}; 



static GOptionEntry entries[] = {
  {"sessionid", 0, 0, G_OPTION_ARG_INT, &session_id,
      "String ID of the peer to connect to", "ID"},
  {"signalling", 0, 0, G_OPTION_ARG_STRING, &signalling_url,
      "Signalling server to connect to", "URL"},
  {"turn", 0, 0, G_OPTION_ARG_STRING, &turn,
      "Request that the peer generate the offer and we'll answer", "URL"},
  {"audiocodec", 0, 0, G_OPTION_ARG_STRING, &audio_codec,
      "audio codec use for decode bin", "codec"},
  {"videocodec", 0, 0, G_OPTION_ARG_STRING, &video_codec,
      "video codec use for decode bin", "codec"},
  {NULL},
};

int
main (int argc, char *argv[])
{
    GOptionContext *context;
    GError *error = NULL;

    context = g_option_context_new ("- thinkmay gstreamer client");
    g_option_context_add_main_entries (context, entries, NULL);
    g_option_context_add_group (context, gst_init_get_option_group ());
    if (!g_option_context_parse (context, &argc, &argv, &error)) {
        g_printerr ("Error initializing: %s\n", error->message);
        return -1;
    }

    if(!session_id || !video_codec || !audio_codec)
    {
        g_printerr(argv);
        g_printerr("missing argument");
        return -1;
    }



    remote_app_initialize(session_id, signalling_url, turn, audio_codec, video_codec);
    return 0;
}
