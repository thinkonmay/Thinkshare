#include "shared-memory-thread-pool.h"


////establish new thread pool
ThreadPool* 
thread_pool_new (SharedMemoryLink* link)
{
    ThreadPool* pool = malloc(sizeof(ThreadPool));

    pool->send_queue =    g_async_queue_new();
    pool->receive_queue = g_async_queue_new();  

    pool->input_handling_thread = g_thread_new("input-thread",receive_queue_handle,link);

    MemoryBlock* block = shared_memory_link_get_memory_block(link);

    for (gint i = 0; i < THREAD_PER_LINK; i++)
    {
        shared_memory_create_thread(link, &block->segment[i], NULL);
    }
}


/// <summary>
/// ThreadFunc of "input-thread" that iretationally check 
/// if there any message left in the asynchronous queue and dispatch them
/// </summary>
/// <param name="data"> the ThreadData that passed from g_thread_new function</param>
/// <returns></returns>
static gpointer
receive_queue_handle(gpointer data)
{
    SharedMemoryLink* link = (SharedMemoryLink*) data;
    ThreadPool* pool = shared_memory_get_thread_pool(link);

    while (TRUE)
    {
        g_async_queue_lock(pool->receive_queue);

        if( g_async_queue_length_unlocked(pool->receive_queue) > 0)
        {
            SharedMemoryMessage* message = g_async_queue_pop_unlocked(pool->receive_queue);
            shared_memory_message_receive_handle_function(link, message);
        }
        g_async_queue_unlock(pool->receive_queue);
    }
}

/// <summary>
/// function executed in receiving thread
///  responsible for handle incoming message
/// </summary>
/// <param name="link">the shared memory link receive the package</param>
/// <param name="message">the message that should be parse</param>
void
shared_memory_message_receive_handle_function(SharedMemoryLink* link,
SharedMemoryMessage* message)
{
   SharedMemoryHub* hub = shared_memory_get_owned_hub(link);
   SharedMemoryHubMaster* hub_master = (SharedMemoryHubMaster*) hub;
   SharedMemoryHubMasterClass* klass = SHARED_MEMORY_HUB_MASTER_GET_CLASS(hub_master);

   gint peer_id;
   g_object_get_property(link, "peer-id",&peer_id);

   switch (message->opcode)
    {
        case MESSAGE:
            g_signal_emit_by_name(hub,"on-message", peer_id, message->data_pointer);
        case PEER_LINK_REQUEST:
            klass->establish_peer_link(hub_master, *(gint*)message->data_pointer, peer_id);
        case NEW_LINK_RESPOND:
            shared_memory_link_new(hub,*(HANDLE*)message->data_pointer);
        case UPDATE_HUB_ID_ARRAY:
            shared_memory_hub_update_id_array(hub, message->data_pointer);
        case MESSAGE_HUGE:
    }
}




/// <summary>
/// function run inside thread that check if any message available inside asynchronous queue
/// get a package from asyncqueue to perform memory copy 
/// </summary>
/// <param name="pool"></param>
/// <returns></returns>
static gpointer
shared_memory_thread_pool_wait_for_new_task (ThreadPool* pool)
{
    gpointer task = NULL;

    if(g_async_queue_length_unlocked(pool->send_queue) > 0)
    {
        task = g_async_queue_pop_unlocked(pool->send_queue);
    }
    return task;
}



/// <summary>
/// function run inside thread that check if any message available inside asynchronous queue
/// get a package from asyncqueue to perform memory copy 
/// </summary>
/// <param name="pool"></param>
/// <returns></returns>
static gpointer
shared_memory_thread_pool_wait_for_new_data (ThreadPool* pool)
{
    gpointer task = NULL;

    g_async_queue_unlock(pool->receive_queue);
    if(g_async_queue_length_unlocked(pool->receive_queue) > 0)
    {
        task = g_async_queue_pop_unlocked(pool->receive_queue);
    }
    g_async_queue_unlock(pool->receive_queue);
    return task;
}








/// <summary>
/// ThreadFunc that force the thread in thread pool to run in infinite loop
/// that iretationally check for available message in asynchronous queue
/// </summary>
/// <param name="input_data">ThreadData passed to thread contain thread pool
/// and corrsponding memory segment to this thread</param>
/// <returns></returns>
static gpointer
shared_memory_thread_pool_sender_thread_proxy(gpointer input_data)
{
    ThreadData* data = (ThreadData*) input_data;

    ThreadPool* pool = data->pool;
    MemorySegment*  segment = data->segment;

    g_async_queue_lock(pool->send_queue);

    while(TRUE)
    {
        SharedMemoryMessage* task = 
        shared_memory_thread_pool_wait_for_new_task(pool->send_queue);
        
        
        g_async_queue_unlock(pool->send_queue);

        /*switch thread action base on message context*/
        switch(task->opcode)
        {
            case MESSAGE:
            {
                if(WaitForSingleObject(segment->mutex,INFINITE) == WAIT_OBJECT_0)
                {
                    segment->opcode = MESSAGE;
                    segment->message_size = task->size;
                    memcpy(segment->buffer_pointer, task->data_pointer , task->size);
                }

                ReleaseMutex(segment->mutex);
            }
            case MESSAGE_HUGE:
            {
                if(WaitForSingleObject(segment->mutex,INFINITE) == WAIT_OBJECT_0)
                {
                    segment->opcode = MESSAGE;
                    segment->message_size = task->size;
                    memcpy(segment->buffer_pointer, task->data_pointer , task->size);
                }

                ReleaseMutex(segment->mutex);
            }


            case PEER_LINK_REQUEST:
            {
                if(WaitForSingleObject(segment->mutex,INFINITE) == WAIT_OBJECT_0)
                {
                    segment->opcode = MESSAGE;
                    segment->message_size = task->size;
                    memcpy(segment->buffer_pointer, task->data_pointer , task->size);
                }

                ReleaseMutex(segment->mutex);
            }
            case NEW_LINK_RESPOND:
            {
                if(WaitForSingleObject(segment->mutex,INFINITE) == WAIT_OBJECT_0)
                {
                    segment->opcode = NEW_LINK_RESPOND;
                    segment->message_size = task->size;
                    memcpy(segment->buffer_pointer, task->data_pointer , task->size);
                }

                ReleaseMutex(segment->mutex);

            }
            case UPDATE_HUB_ID_ARRAY:
            {
                if(WaitForSingleObject(segment->mutex,INFINITE) == WAIT_OBJECT_0)
                {
                    segment->opcode = UPDATE_HUB_ID_ARRAY;
                    segment->message_size = task->size;
                    memcpy(segment->buffer_pointer, task->data_pointer , task->size);
                }

                ReleaseMutex(segment->mutex);
            }
        }
    }
    return NULL;
}

/// <summary>
/// ThreadFunc that force the thread in thread pool to run in infinite loop
/// that iretationally check for available message in asynchronous queue
/// </summary>
/// <param name="input_data">ThreadData passed to thread contain thread pool
/// and corrsponding memory segment to this thread</param>
/// <returns></returns>
static gpointer
shared_memory_thread_pool_receiver_thread_proxy(gpointer input_data)
{
    ThreadData* data = (ThreadData*) input_data;
    MemorySegment* segment = data->segment;
    ThreadPool* pool = data->pool;

    while(TRUE)
    {    
        if(WaitForSingleObject(segment->mutex,INFINITE) == WAIT_OBJECT_0)
        {
            if(segment->opcode ==  OPCODE_UNKNOWN)
                goto end;



            SharedMemoryMessage* message = malloc(sizeof(SharedMemoryMessage));
                        
            switch(segment->opcode)
            {
                case MESSAGE:
                {
                    gpointer data = malloc(segment->message_size);
                    memcpy(data, segment->buffer_pointer, segment->size);

                    message->data_pointer = data;
                    message->opcode = MESSAGE;
                    message->size;                    

                }
                case PEER_LINK_REQUEST:
                {
                    message->opcode = PEER_LINK_REQUEST;
                    message->size   = 0;
                    message->data_pointer = NULL;
                }
                case NEW_LINK_RESPOND:
                {
                    message->opcode = NEW_LINK_RESPOND;
                    message->size   = segment->size;
                    
                    gpointer data = malloc(segment->message_size);
                    memcpy(data, segment->buffer_pointer, segment->message_size);

                    message->data_pointer = data;

                }
                case UPDATE_HUB_ID_ARRAY:
                {
                    message->opcode = UPDATE_HUB_ID_ARRAY;
                    message->size   = segment->size;
                    
                    gpointer data = malloc(segment->message_size);
                    memcpy(data, segment->buffer_pointer, segment->message_size);
                    message->data_pointer = data;
                    
                }

                 /*push message to receive queue*/
                g_async_queue_lock(pool->receive_queue);
                g_async_queue_push_unlocked(pool->receive_queue,&message);
                g_async_queue_unlock(pool->receive_queue);

                segment->opcode = OPCODE_UNKNOWN;


                end: ReleaseMutex(segment->mutex);
            }
        }
    }
}


/// <summary>
/// create new thread using g_thread_new function
/// </summary>
/// <param name="link"></param>
/// <param name="segment">memory segment corespond to this thread</param>
/// <param name="error">pointer to error when create new thread (if available)</param>
/// <param name="id"></param>
/// <returns></returns>
gboolean
shared_memory_create_thread(SharedMemoryLink* link , 
MemorySegment* segment,
GError** error)
{
    ThreadPool* pool = shared_memory_get_thread_pool(link);
    gboolean success = FALSE;
    GThread *thread;

    if(!success == FALSE)
    {
        const gchar *prgname = g_get_prgname ();
        gchar name[16] = "pool";
      

        if (prgname)
        {
            g_snprintf (name, sizeof (name), "pool-%s", prgname);
        }

        ThreadData* thread_data = malloc(sizeof(ThreadData));

        thread_data->pool = pool;
        thread_data->segment = segment;

        gint id;
        g_object_get_property(link,"hub-id",&id);
        
        if(segment->sender_id == id)
        {
            /*if sender id on segment is hub id of sharedmemorylink, thread perform sender thread proxy*/
            g_free(&id);

            segment->send = g_thread_try_new (name, 
            shared_memory_thread_pool_sender_thread_proxy, 
            thread_data, error);
        }
        else
        {   
            /*otherwise, thread perform receiver thread*/
            g_free(&id);

            segment->receive = g_thread_try_new (name, 
            shared_memory_thread_pool_receiver_thread_proxy, 
            thread_data, error);
        }
    }
    if (thread == NULL)
        return FALSE;
    return TRUE;    
}


/// <summary>
/// push a message into sending asynchronous queue
/// that will be handle by thread pool
/// </summary>
/// <param name="pool"></param>
/// <param name="opcode"></param>
/// <param name="data"></param>
/// <param name="data_size"></param>
void
thread_pool_push_message(ThreadPool* pool,
SharedMemoryOpcode opcode,
gpointer data,
gint data_size)
{
    g_async_queue_lock(pool->send_queue);

    SharedMemoryMessage* message = malloc(sizeof(SharedMemoryMessage));

    message->opcode = opcode;
    message->size=    data_size;
    message->data_pointer = data;

    g_async_queue_push_unlocked(pool->send_queue, message);

    g_async_queue_unlocked(pool->send_queue);
}


