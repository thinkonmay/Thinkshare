#include <agent-device.h>
#include <tchar.h>
#include <json-glib/json-glib.h>
#include <agent-object.h>

#include <sysinfoapi.h>
#include <agent-message.h>
#include <Windows.h>
#include <stdio.h>

#define DIV 1048576
#define ID  0

#include <intrin.h>
static unsigned long long
FileTimeToInt64(const FILETIME ft);

//Function to Get CPU Load
float
GetCPULoad()
{
	FILETIME idleTime, kernelTime, userTime;
	GetSystemTimes(&idleTime, &kernelTime, &userTime);
	return	CalculateCPULoad(FileTimeToInt64(idleTime),
		FileTimeToInt64(kernelTime) + FileTimeToInt64(userTime));
}


/// <summary>
/// Information about slave hardware configuration
/// </summary>
struct _DeviceInformation
{
	gint id;

	gchar cpu[64];
	gchar gpu[64];
	gint ram_capacity;
	gchar OS[64];
};

struct _DeviceState
{
	gint* cpu_usage;
	gint* gpu_usage;
	gint* ram_usage;
};

DeviceInformation*
get_device_information() 
{
	DeviceInformation* device_info = malloc(sizeof(DeviceInformation));

	device_info->id = ID;


	memcpy(&device_info->cpu, "ryzen 7 2700",sizeof("ryzen 7 2700"));
	memcpy(&device_info->gpu, "gtx 1060 6GB",sizeof("gtx 1060 6GB"));
	memcpy(&device_info->OS, "Windows 10", sizeof("Windows 10"));
	device_info->ram_capacity = 16000000;


	return device_info;

	/*

	int CPUInfo[4] = { -1 };
	unsigned nExIds, i = 0;
	char CPUBrandString[0x40];
	__cpuid(CPUInfo, 0x80000000);
	nExIds = CPUInfo[0];
	

	
	for (i = 0x80000000; i <= nExIds; i++) {
		__cpuid(CPUInfo, i);
		if (i == 0x80000002)
			memcpy(CPUBrandString, CPUInfo, sizeof(CPUInfo));
		else if (i == 0x80000003)
			memcpy(CPUBrandString + 16, CPUInfo, sizeof(CPUInfo));
		else if (i == 0x80000004)
			memcpy(CPUBrandString + 32, CPUInfo, sizeof(CPUInfo));
	}
	

	memcpy(&device_info->cpu ,"ryzen 7 2700" (&CPUBrandString), 64);


	MEMORYSTATUSEX statex;
	statex.dwLength = sizeof(statex);
	GlobalMemoryStatusEx(&statex);
	guint64 ram_cap = (statex.ullTotalPhys / 1024) / 1024;


	memcpy(&device_info->ram_capacity, &ram_cap,sizeof(guint64));


	IDirect3D9* d3Object = Direct3DCreate9(D3D_SDK_VERSION);
	UINT adaptercount = d3Object->lpVtbl->GetAdapterCount(d3Object);
	D3DADAPTER_IDENTIFIER9* adapters = (D3DADAPTER_IDENTIFIER9*)malloc(sizeof(D3DADAPTER_IDENTIFIER9) * adaptercount);

	for (int i = 0; i < adaptercount; i++)
	{
		d3Object->lpVtbl->GetAdapterIdentifier(d3Object, i, 0, &(adapters[i]));
	}

	memcpy(&device_info->gpu , "gtx 1060", 512);

	memcpy(&device_info->OS , "Window10",sizeof("Window10"));

	*/
};

//Function to Update get device state;
DeviceState* 
get_device_state() 
{
	DeviceState* device_state = malloc(sizeof(DeviceState));

	MEMORYSTATUSEX statex;
	statex.dwLength = sizeof(MEMORYSTATUSEX);

	GlobalMemoryStatusEx(&statex);

	gulong ram_use = (gulong)statex.dwMemoryLoad;

	device_state->ram_usage = &ram_use;

	gint load = (gint)(GetCPULoad() * 100);
	 
	device_state->cpu_usage = &load;

	return device_state;
};

Message*
get_json_message_from_device(AgentObject* object)
{
	HANDLE* handle = agent_get_mutex_handle_ptr(object);

	WaitForSingleObject(*handle,INFINITE);
	DeviceInformation* infor =	agent_get_device_information(object);
	DeviceState* state =		agent_get_device_state(object);
	ReleaseMutex(*handle);

	JsonObject* information = json_object_new();
	JsonObject* device_state = json_object_new();

	json_object_set_string_member(information, "CPU", infor->cpu);
	json_object_set_string_member(information, "GPU", infor->gpu);
	json_object_set_string_member(information, "OS", infor->OS);
	json_object_set_int_member(information, "RAMcapacity", infor->ram_capacity);
	json_object_set_int_member(information, "ID", infor->id);

	json_object_set_int_member(device_state, "CPUusage", *state->cpu_usage);
	json_object_set_int_member(device_state, "GPUusage", *state->gpu_usage);
	json_object_set_int_member(device_state, "RAMusage", *state->ram_usage);

	Message* message;

	json_object_set_object_member(message, "DeviceState", device_state);
	json_object_set_object_member(message, "DeviceInformation", information);


	return message;
}



Message*
get_json_message_from_device_information(DeviceInformation* infor)
{

	JsonObject* information = json_object_new();

	json_object_set_string_member(information,	"CPU", infor->cpu);
	json_object_set_string_member(information,	"GPU", infor->gpu);
	json_object_set_string_member(information,	"OS", infor->OS);
	json_object_set_int_member(information,		"RAM", infor->ram_capacity);

	JsonObject* message = json_object_new();
	json_object_set_object_member(message, "DeviceInformation", information);

	return message;
}


//Define function to calculate CPU Load
static float 
CalculateCPULoad(unsigned long long idleTicks, unsigned long long totalTicks)
{
	static unsigned long long 
		_previousTotalTicks = 0;
	static unsigned long long 
		_previousIdleTicks = 0;

	unsigned long long 
		totalTicksSinceLastTime = totalTicks - _previousTotalTicks;
	unsigned long long 
		idleTicksSinceLastTime = idleTicks - _previousIdleTicks;


	float ret = 1.0f - ((totalTicksSinceLastTime > 0) ? ((float)idleTicksSinceLastTime) / totalTicksSinceLastTime : 0);

	_previousTotalTicks = totalTicks;
	_previousIdleTicks = idleTicks;
	return ret;
}

//Function to convert from file time to Int
static unsigned long long 
FileTimeToInt64(const FILETIME ft)
{
	return (((unsigned long long)(ft.dwHighDateTime)) << 32) | ((unsigned long long)ft.dwLowDateTime);
}

// Returns 1.0f for "CPU fully pinned", 0.0f for "CPU idle", or somewhere in between
// You'll need to call this at regular intervals, since it measures the load between
// the previous call and the current one.  Returns -1.0 on error.



