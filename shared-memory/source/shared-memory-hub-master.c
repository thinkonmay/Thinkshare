#include <shared-memory-hub-master.h>
#include <shared-memory-link.h>
#include <shared-memory-thread-pool.h>


/// <summary>
/// private struct 
/// </summary>
typedef struct
{
    Slave* slave_array[MAX_LINK_MASTER];

    gint segment_size;

    gint segment_number;
}SharedMemoryHubMasterPrivate;



static void
shared_memory_hub_master_class_init(SharedMemoryHubMasterClass* klass);
static void
shared_memory_hub_master_constructed(GObject* object);
static void
shared_memory_hub_master_dispose(GObject* object);
static void
shared_memory_hub_master_finalize(GObject* object);

void
shared_memory_hub_master_update_id(SharedMemoryHubMaster* self, gint id_array[MAX_LINK_MASTER]);


G_DEFINE_TYPE_WITH_PRIVATE(SharedMemoryHubMaster,shared_memory_hub_master,SHARED_MEMORY_TYPE_HUB)



/// <summary>
/// create new simple shared memory hub, remember that master block always has value equal zero
/// </summary>
/// <param name="hub_id"></param>
/// <param name="max_link"></param>
/// <param name="is_master"></param>
/// <returns></returns>
SharedMemoryHubMaster* 
shared_memory_hub_master_new(void)
{
    SharedMemoryHubMaster* ret = g_object_new(SHARED_MEMORY_TYPE_HUB_MASTER,NULL);

    g_object_set_property((SharedMemoryHub*)ret,"hub-id", MASTER_ID);
	return ret;
}


/// <summary>
/// class initialization:
/// override base gobject class method,
/// declare signal and properties
/// </summary>
/// <param name="klass"></param>
static void
shared_memory_hub_master_class_init(SharedMemoryHubMasterClass* klass)
{
	GObjectClass* object_class = G_OBJECT_GET_CLASS(klass);

	object_class->constructed =  shared_memory_hub_master_constructed;
	object_class->dispose =      shared_memory_hub_master_dispose;
	object_class->finalize =     shared_memory_hub_master_finalize;

    klass->establish_peer_link = shared_memory_hub_master_create_link;
    klass->call_slave_process  = shared_memory_hub_master_create_new_slave;
    klass->terminate_slave_process = shared_memory_hub_master_terminate_slave;
}

/// <summary>
/// constructed method used by base class,
///  process after initialization process is done
/// </summary>
/// <param name="object"></param>
static void
shared_memory_hub_master_constructed(GObject* object)
{
	SharedMemoryHubMaster* self = (SharedMemoryHubMaster*)object;
	SharedMemoryHubMasterPrivate* priv = shared_memory_hub_master_get_instance_private(self);

    priv->segment_number = THREAD_PER_LINK;

    /*initialize slave_array array with NULL pointer*/
	///memset(priv->slave_array, 0, MAX_LINK_MASTER*sizeof(gpointer));
}



static void
shared_memory_hub_master_dispose(GObject* object)
{

	SharedMemoryHubMaster* self = (SharedMemoryHubMaster*)object;
	SharedMemoryHubMasterPrivate* priv = shared_memory_hub_master_get_instance_private(self);

	for (int i = 0; i < MAX_LINK_MASTER; i++)
	{
		g_free(priv->slave_array[i]);
	}
}

static void
shared_memory_hub_master_finalize(GObject* object)
{

	SharedMemoryHubMaster* self = (SharedMemoryHubMaster*)object;
	SharedMemoryHubMasterPrivate* priv = shared_memory_hub_master_get_instance_private(self);

	g_free(object);
}


static void 
shared_memory_hub_master_init(SharedMemoryHubMaster *object)
{
    return;
}
/*
* done dealing with class declaration stuff
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*/


/// <summary>
/// calculate size of the memory block.
/// </summary>
/// <param name="master"></param>
/// <returns></returns>
gint
size_of_memory_block(SharedMemoryHubMaster* master)
{
    SharedMemoryHubMasterPrivate* priv = shared_memory_hub_master_get_instance_private(master);

    return sizeof(HANDLE) + 2* sizeof(gint) +
    sizeof(MemorySegment) * priv->segment_number +
    priv->segment_size * priv->segment_number;
}


/// <summary>
/// calculate start address of the buffer in Memory block
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
gint 
buffer_start_address(SharedMemoryHubMaster* self)
{
    SharedMemoryHubMasterPrivate* priv = shared_memory_hub_master_get_instance_private(self);

    return segment_buffer_start_address() +
    priv->segment_number*sizeof(MemorySegment) ;
}


/// <summary>
///calculate start address of buffer of a specific segment in memory block
/// </summary>
/// <param name=""></param>
/// <returns></returns>
gint 
segment_buffer_start_address(void)
{
    return sizeof(HANDLE)+2*sizeof(gint);
}

/// <summary>
/// create and initialize memory block for link establishment,
/// memory block handle will be send to slave hub through normal link send 
/// </summary>
/// <param name="self">Masterhub that create the memory block</param>
/// <param name="id1">id of the first shared memory hub that own the link</param>
/// <param name="id2">id of the second shared memory hub that own the link</param>
/// <returns></returns>
HANDLE
memory_block_new(SharedMemoryHubMaster* self, gint id1, gint id2)
{
    SharedMemoryHubMasterPrivate* priv = shared_memory_hub_master_get_instance_private(self);

    /*ADD secirity attribute for mapped file*/
   SECURITY_ATTRIBUTES attr;
    attr.bInheritHandle = TRUE;
    attr.lpSecurityDescriptor = NULL;
    attr.nLength = sizeof(SECURITY_ATTRIBUTES);



    /*create shared memory handle*/
    HANDLE memory_handle = 
    CreateFileMapping( INVALID_HANDLE_VALUE,
    &attr,PAGE_READWRITE,0, size_of_memory_block(self)
    ,NULL);

    gchar* ret = MapViewOfFile (memory_handle, 
        FILE_MAP_ALL_ACCESS,0,0,0);

    memset(ret,0,sizeof(ret));

    memcpy(ret,&memory_handle, sizeof(HANDLE));

    /*initialize id add of hub for */
    memcpy((ret + sizeof(HANDLE)),  &id1 , sizeof(gint));
    memcpy(ret+sizeof(HANDLE)+sizeof(gint), &id2, sizeof(gint));
    
    for (gint i = 0; i <  priv->segment_number; i++)
    {
        MemorySegment* segment = (MemorySegment*) ret + segment_buffer_start_address() + (i*sizeof(MemorySegment));

        
        /*simple algorithm to ensure number of send and receive segment are equal*/
        if (segment->sender_id % 2 == 0)
        {
            segment->sender_id = id1;
        }
        else
        {
            segment->sender_id = id2;
        }


        segment->size = priv->segment_size;
        SECURITY_ATTRIBUTES attr;
        attr.nLength = sizeof(SECURITY_ATTRIBUTES);
        attr.lpSecurityDescriptor = NULL;
        attr.bInheritHandle = TRUE;
        segment->mutex = CreateMutexA(&attr,FALSE,NULL);

        /*navigate segment pointer to corrresponding position on memory block*/
        segment->buffer_pointer = (gchar*) ret + 
        buffer_start_address(self) + i*segment->size;
    }

    return memory_handle;
}




/// <summary>
/// Master hub perform peer memory block transfer between two slave hub,
/// the process include create new memory block and send its handle to both shared memory hub
/// </summary>
/// <param name="self">masterhub</param>
/// <param name="id1">id of the first hub that should be linked</param>
/// <param name="id2">id of the second hub that should be linked</param>
/// <returns></returns>
gboolean
shared_memory_hub_master_create_link(SharedMemoryHubMaster* self,
    gint id1, 
    gint id2)
{
	SharedMemoryHubMasterPrivate* priv = shared_memory_hub_master_get_instance_private(self);
    SharedMemoryHub* hub = (SharedMemoryHub*) self;
    SharedMemoryHubClass* hub_class = SHARED_MEMORY_HUB_GET_CLASS(hub);

    MemoryBlock* block = memory_block_new(self, id1, id2);


    hub_class->send_data(hub,id1, block->handle,sizeof(HANDLE));
    hub_class->send_data(hub,id1, block->handle,sizeof(HANDLE));
    return TRUE;
}



/// <summary>
/// generate new slave process (child process in window language) 
/// lies in the same directory with the master process
/// given by an application name
/// </summary>
/// <param name="self">masterhub that create new slave</param>
/// <param name="app">name of the slave application (ex: 'slave.exe')</param>
/// <returns></returns>
gint
shared_memory_hub_master_create_new_slave(SharedMemoryHubMaster* self, 
gchar* app, gint id)
{   
    SharedMemoryHubMasterPrivate* priv = shared_memory_hub_master_get_instance_private(self);

    Slave* slave = malloc(sizeof(Slave));
    gint slave_id = id;

    HANDLE mem_handle = memory_block_new(self,MASTER_ID, slave_id);
    gchar* handle_string = (gchar*) mem_handle;
    

    STARTUPINFO si;
    PROCESS_INFORMATION pi;

    ZeroMemory(&si, sizeof(si));
    si.cb = sizeof(si);
    ZeroMemory(&pi,sizeof(pi));


    /*concantenate command line string by using memcpy*/
    gchar* cmd = malloc(strlen(app)+strlen(handle_string)+2);
    memcpy(cmd, &app, strlen(app));
    memcpy(cmd + strlen(app), &(" "), 1);
    memcpy(cmd + strlen(app) + 1, &handle_string, strlen(handle_string));


    CreateProcess(NULL, cmd ,NULL,NULL, TRUE, CREATE_NEW_CONSOLE,
    NULL, NULL,&si,&pi);
    
    slave->process_handle = &pi.hProcess;

    /*add new link to link array*/
    shared_memory_hub_add_link((SharedMemoryHub*)self,mem_handle);

    for(gint i = 0; i< MAX_LINK_MASTER; i++)
    {
        if (priv->slave_array[i] == NULL)
        {
            priv->slave_array[i] = slave;
        }
    }




    gint id_array[MAX_LINK_MASTER];
    for (gint i = 0; i < MAX_LINK_MASTER ; i++)
    {
        id_array[i] = priv->slave_array[i]->id;
    }

    /*every slave hub update their own hub_id_array*/
    shared_memory_hub_master_update_id(self,id_array);
    return slave->id;
}


/// <summary>
/// update hub_id_array with every slave hub. 
/// This process should be invoke after a new slave have been create or terminated.
/// </summary>
/// <param name="self">Masterhub</param>
/// <param name="id_array">array of updated slave id</param>
void
shared_memory_hub_master_update_id(SharedMemoryHubMaster*  self,gint id_array[MAX_LINK_MASTER])
{
    SharedMemoryHub* hub = (SharedMemoryHub*) self;
    SharedMemoryHubClass* klass = SHARED_MEMORY_HUB_GET_CLASS(hub);
    SharedMemoryHubMasterPrivate* priv = shared_memory_hub_master_get_instance_private(self);


    for (gint i = 0; i < MAX_LINK_MASTER; i++)
    {
        if (priv->slave_array[i] != NULL)
        {
            SharedMemoryLink* link = shared_memory_hub_get_link_by_id(hub,priv->slave_array[i]->id );
            ThreadPool* pool = shared_memory_link_get_thread_pool(link);
            thread_pool_push_message(pool, UPDATE_HUB_ID_ARRAY, &id_array, sizeof(gint)*MAX_LINK_MASTER);
        }    
    }
}



/// <summary>
/// slave hub termination process, this mean that slave process is ended by using TerminateProcess function
/// </summary>
/// <param name="self">MasterHub</param>
/// <param name="slave_id">id of the terminated process</param>
/// <returns></returns>
gboolean
shared_memory_hub_master_terminate_slave(SharedMemoryHubMaster* self,
gint slave_id)
{
    SharedMemoryHubMasterPrivate* priv = shared_memory_hub_master_get_instance_private(self);
    for (gint i = 0; i < MAX_LINK_MASTER; i++)
    {
        if (priv->slave_array[i]->id == slave_id)
        {
            TerminateProcess(*priv->slave_array[i]->process_handle,1);
            CloseHandle(*priv->slave_array[i]->process_handle);
            priv->slave_array[i] = NULL;
        }
    }
    return TRUE;
}


