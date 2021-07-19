#include <glib.h>
#include <agent-object.h>
#include <libsoup-2.4/libsoup/soup.h>
#include <agent-type.h>
#include <agent-socket.h>
#include <gst/gst.h>


GMainContext* context;


gpointer
timeout(gpointer data)
{
    AgentObject* agent = (AgentObject*)data;

    if (agent == NULL)
    {
        return NULL;
    }


    Socket* socket = agent_get_socket(agent);
    SoupWebsocketConnection* ws = socket_get_connection(socket);


    if (ws == NULL)
    {
        return NULL;
    }
}


int 
main(int argc, char** argv)
{
    AgentObject* agent;

    /*
    GOptionContext* context;
    GError* error = NULL;

    context = g_option_context_new("agent");
    g_option_context_add_group(context, gst_init_get_option_group());
    */
    agent = agent_new();
    //g_timeout_add_seconds(100, timeout, agent);




    return 0;
}
