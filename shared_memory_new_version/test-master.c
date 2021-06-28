#include "shared-memory.h"

int main(void)
{
	SharedMemoryHubMaster* master = shared_memory_hub_master_new();

	SharedMemoryHubMasterClass* klass = SHARED_MEMORY_HUB_MASTER_GET_CLASS(master);

	gint slave_id = klass->call_slave_process(master,"test-slave.exe");

	SharedMemoryHubClass* hub_klass = SHARED_MEMORY_HUB_GET_CLASS((SharedMemoryHub*) master);

	gchar* data = "hello";
	hub_klass->send_data(slave_id,&data,sizeof(data));
}