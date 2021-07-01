#include <session-core.h>



SessionCore* core;
GMainLoop* loop;









int
main(int argc, char* argv[])
{
    GOptionContext* context;
    GError* error = NULL;

    /*context stuff*/
    context = g_option_context_new("personal cloud computing");

    g_option_context_add_group(context, gst_init_get_option_group());
    loop = g_main_loop_new(NULL, FALSE);
   
    SharedMemoryHub* hub = shared_memory_hub_initialize(argc, argv);
    g_object_new(SESSION_TYPE_CORE,NULL);


    core = session_core_initialize(argc,argv);
    SessionCoreClass* klass = SESSION_CORE_GET_CLASS(core);


    klass->setup_pipeline(core);
    klass->setup_data_channel(core);
    klass->setup_webrtc_signalling(core);
    klass->connect_signalling_server(core);
    klass->start_pipeline(core);
    

    g_main_loop_run(loop);
    g_main_loop_unref(loop);


    return 0;
}
