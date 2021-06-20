#include "frame-work.h"
#include "shared-memory-hub.h"
#include "shared-memory-link.h"
#include <Windows.h>
#include <glib.h>
#include <glib-2.0/glib-object.h>
#include <glib-2.0/gio/gio.h>
/*Test file for shared-memory project, 
Hiep and Tuan should perform bandwidth and latency test here 
*/






#define MAIN_HUB_ID 123456
#define PEER_HUB_ID 654321

#define BLOCK_SIZE 1000000
#define PIPE_SIZE 1000

SharedMemoryHub* peer_hub;
SharedMemoryHub* main_hub;
SharedMemoryLink* main_link;
SharedMemoryLink* peer_link;
GMainContext* context;


void 
callback_main( GAsyncResult* result, gpointer data)
{
    GError *error;
    main_link = shared_memory_hub_link_finish(main_hub, result, &error);
}

void
callback_peer( GAsyncResult* result, gpointer data)
{
    GError *error;
    peer_link = shared_memory_hub_link_finish(peer_hub, result, &error);
}

void
on_message_string(SharedMemoryLink* self,
    gint from,
    gint to,
    SharedMemoryOpcode opcode,
    gpointer data,
    gpointer user_data)
{
    g_print("got message string from %d", from);
    g_print((gchar*)data);
}



int main (int argc, char argv[])
{
    main_hub = shared_memory_hub_new(MAIN_HUB_ID,8,TRUE);
    peer_hub = shared_memory_hub_new_default(PEER_HUB_ID);


    g_main_context_push_thread_default(context);

    shared_memory_hub_link_default_async(peer_hub, MAIN_HUB_ID, BLOCK_SIZE, PIPE_SIZE,NULL, G_CALLBACK(callback_peer),NULL);

    gchar* data = "Hello";

    Message* msg = malloc(sizeof(msg));
    msg->from = PEER_HUB_ID;
    msg->to   = MAIN_HUB_ID;
    msg->opcode = 0;
    msg->data = data;

    if(!shared_memory_link_send_message(peer_link, msg))
    {
        g_printerr("fail to send message");
    }
    g_main_context_pop_thread_default(context);

    shared_memory_hub_link_default_async(main_hub,PEER_HUB_ID,BLOCK_SIZE,PIPE_SIZE,NULL,G_CALLBACK(callback_main), NULL);

    g_signal_connect(main_hub, "on-message",on_message_string,NULL );
}