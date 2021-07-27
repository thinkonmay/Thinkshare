#include <glib.h>
#include <agent-object.h>


const gchar* HOST_URL  = "http://google.com";

int 
main(void)
{
    AgentObject* agent;


    GError* error = NULL;
    GOptionContext* context;

    context = g_option_context_new("- test tree model performance");
    g_option_context_add_main_entries(context, entries, NULL);
    if (!g_option_context_parse(context, &argc, &argv, &error))
    {
        g_print("option parsing failed: %s\n", error->message);
        exit(1);
    }
    
    if (local_host)
    {
        if (!port)
        {
            g_printerr("port number is neccessary");
            return 0;
        }
        gchar* buffer = malloc(10);
        itoa(port,buffer,10);
        gchar* route = "/Agent";
        gchar prefix[100] = "wss://localhost:";
        strcat(&prefix, buffer);
        strcat(&prefix, route);
        memcpy(url , prefix, strlen(prefix));

        g_print("connecting to host with url %s ,\n",url);
    }
    else
    {
        memcpy(url, Host_url, strlen(Host_url));
    }



    
    agent_new(url);
    return 1;
}
