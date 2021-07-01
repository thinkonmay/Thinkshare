#include <shared-memory.h>

void
on_message(GObject* object,
    gint from,
    gpointer data,
    gpointer user_data)
{
    g_print("receive %s from %d", (gchar*)data, from);
}

int 
main(gint argc, gchar* argv[])
{
    SharedMemoryHub* hub = shared_memory_hub_initialize(argc,argv);

    g_signal_connect(hub, "on-message",G_CALLBACK(on_message),NULL);

    return 0;
}

