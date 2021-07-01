#include <glib.h>
#include <agent-object.h>
#define HOST_URL abc;


GMainLoop* loop;

int main(int argc, char* argv[])
{
    AgentObject* object = agent_object_new(HOST_URL); 

    loop = g_main_loop_new(NULL, FALSE);


    connect_to_host_async();

    g_main_loop_run(loop);
    g_main_loop_unref(loop);
    return 0;
}
