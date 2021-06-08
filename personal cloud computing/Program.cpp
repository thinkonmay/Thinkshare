#include "Framework.h"
#include "Variable.h"
#include "Handle data channel.h"
#include "Handle pipeline.h"
#include "RC config.h"
#include "CorePipeSink.h"
#include "Signalling handling.h"

static GOptionEntry entries[] = {
  {"screen-width",0,0, G_OPTION_ARG_INT, &screen_width,
    "width of the video stream (pixels)","screen width"},
  {"screen-height",0,0, G_OPTION_ARG_INT, &screen_height,
    "height of the video stream (pixels)","screen width"},
  {"framerate",0,0, G_OPTION_ARG_INT, &framerate,
    "frame rate of the video stream (frame per second)","frame rate"},
  {"SessionSlaveID", 0, 0, G_OPTION_ARG_STRING, &peer_id,
      "String ID of the peer to connect to", "ID"},
  {"SignallingServer", 0, 0, G_OPTION_ARG_STRING, &server_url,
      "Signalling server to connect to", "URL"},
  {"disable-ssl", 0, 0, G_OPTION_ARG_NONE, &disable_ssl, "Disable ssl", NULL},
  {"Client-send-offer", 0, 0, G_OPTION_ARG_NONE, &Client_send_offer_first,
      "Request that the peer generate the offer and we'll answer", NULL},
  {NULL},
};



int
main(int argc, char* argv[])
{
    GOptionContext* context;
    GError* error = NULL;

    /*context stuff*/
    context = g_option_context_new("- personal cloud computing");
    g_option_context_add_main_entries(context, entries, NULL);
    g_option_context_add_group(context, gst_init_get_option_group());
    if (!g_option_context_parse(context, &argc, &argv, &error)) {
        g_printerr("Error initializing: %s\n", error->message);
        return -1;
    }
    core_pipe_sink_handle();

    loop = g_main_loop_new(NULL, FALSE);
   

    connect_to_websocket_server_async();

    g_main_loop_run(loop);
    g_main_loop_unref(loop);

    if (pipeline) {
        gst_element_set_state(GST_ELEMENT(pipeline), GST_STATE_NULL);
        g_print("Pipeline stopped\n");
        gst_object_unref(pipeline);
    }

    return 0;
}
