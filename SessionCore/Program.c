#include "Framework.h"
#include "Session.h"
#include "session-core.h"
#include "Handle pipeline.h"
#include "Handle data channel.h"
#include "Signalling handling.h"
gint id;
SessionCore* core;
GMainLoop* loop;


static GOptionEntry entries[] = {
  {"hub-id",0,0, G_OPTION_ARG_INT, &id,
    "shared memory id used for shared memory handshake","ID"},
  {NULL},
};


int
main(int argc, char* argv[])
{
    GOptionContext* context;
    GError* error = NULL;

    /*context stuff*/
    context = g_option_context_new("personal cloud computing");
    g_option_context_add_main_entries(context, entries, NULL);
    g_option_context_add_group(context, gst_init_get_option_group());

    if (!g_option_context_parse(context, &argc, &argv, &error)) {
        g_printerr("Error initializing: %s\n", error->message);
        return -1;
    }
    loop = g_main_loop_new(NULL, FALSE);
   
    g_object_new(SESSION_TYPE_CORE, "hub-id",id);

    core = session_core_new(id);




    session_core_link_shared_memory_hub(core);
    session_core_setup_pipeline(core,NULL);
    connect_WebRTCHub_handler(core);



    if (!session_core_connect_data_channel_signals(core))
    {
        g_printerr("cannot start data channel signal");
    }
    
    if (!session_core_start_pipeline(core))
    {
        g_printerr("cannot start pipeline");
    }
    

    g_main_loop_run(loop);
    g_main_loop_unref(loop);


    return 0;
}
