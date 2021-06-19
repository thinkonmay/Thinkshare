#include "agent-object.h"


static GOptionEntry entries[] = {
  {"Host_URL",0,0, G_OPTION_ARG_STRING, &Host_URL,
    "personal cloud computer cluster host",NULL},
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
    if (!g_option_context_parse(context, &argc, &argv, &error)) {
        g_printerr("Error initializing: %s\n", error->message);
        return -1;
    }

    loop = g_main_loop_new(NULL, FALSE);


    connect_to_host_async();

    g_main_loop_run(loop);
    g_main_loop_unref(loop);
    return 0;
}
