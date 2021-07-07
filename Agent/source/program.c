#include <glib.h>
#include <agent-object.h>


const gchar* HOST_URL  = "http://google.com";

int 
main(void)
{
    AgentObject* object = agent_object_new(HOST_URL); 
    return 0;
}
