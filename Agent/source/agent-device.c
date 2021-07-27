#include <agent-device.h>
#include <tchar.h>
#include <json-glib/json-glib.h>
#include <agent-object.h>
#include <agent-cmd.h>

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

	gchar** cpu;
	gchar** gpu;
	gint* ram_capacity;
	gchar** OS;
};

struct _DeviceState
{
	gint* cpu_usage;
	gint* gpu_usage;
	gint* ram_usage;
};











static DeviceState*
get_device_state();

JsonObject*
track_device();


static DeviceInformation*
get_device_information() 
{

	DeviceInformation* device_info = malloc(sizeof(DeviceInformation));

	device_info->id = ID;

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
	device_info->cpu = &CPUBrandString;

	MEMORYSTATUSEX statex;
	statex.dwLength = sizeof(statex);
	GlobalMemoryStatusEx(&statex);
	guint64 ram_cap = (statex.ullTotalPhys / 1024) / 1024;

	device_info->ram_capacity = &ram_cap;

	IDirect3D9* d3Object = Direct3DCreate9(D3D_SDK_VERSION);
	UINT adaptercount = d3Object->lpVtbl->GetAdapterCount(d3Object);
	D3DADAPTER_IDENTIFIER9* adapters = (D3DADAPTER_IDENTIFIER9*)malloc(sizeof(D3DADAPTER_IDENTIFIER9) * adaptercount);

	for (int i = 0; i < adaptercount; i++)
	{
		d3Object->lpVtbl->GetAdapterIdentifier(d3Object, i, 0, &(adapters[i]));
	}

	device_info->gpu = &adapters->Description;



	*/
};


/*Message*
get_json_message_from_device_information(DeviceInformation* infor)
{
	JsonObject* information = json_object_new();

	json_object_set_string_member(information,	"CPU", infor->cpu);
	json_object_set_string_member(information,	"GPU", infor->gpu);
	json_object_set_string_member(information,	"OS", infor->OS);
	json_object_set_int_member(information,		"RAM", infor->ram_capacity);
	json_object_set_int_member(information, "ID", infor->id);
	return information;
}*/

/// <summary>
/// update device thread function,
/// invoke during agent object initialization
/// </summary>
/// <param name="data"></param>
/// <returns></returns>
gpointer
update_device(AgentObject* agent)
{
	while (TRUE)
	{
		JsonObject* device = track_device();
		json_object_set_int_member(device, "Time", g_get_real_time());

		JsonNode* root;
		JsonGenerator* generator;
		gchar* text;

		/* Make it the root node */
		root = json_node_init_object(json_node_alloc(), device);
		generator = json_generator_new();
		json_generator_set_root(generator, root);
		text = json_generator_to_data(generator, NULL);

		strcat(text, "\n");

		GFile* file = agent_get_device_log(agent);

		GFileOutputStream* stream = g_file_append_to(file, G_FILE_CREATE_NONE, NULL, NULL);
		g_output_stream_write(stream, text, strlen(text), NULL, NULL);

		Sleep(1000);
	}
	return NULL;
}


//Function to Update get device state;
static DeviceState* 
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


JsonObject*
track_device()
{
	DeviceInformation* infor =	get_device_information();
	DeviceState* state =		get_device_state();

	JsonObject* information = json_object_new();
	JsonObject* device_state = json_object_new();

	json_object_set_string_member(information, "CPU", infor->cpu);
	json_object_set_string_member(information, "GPU", infor->gpu);
	json_object_set_string_member(information, "OS", infor->OS);
	json_object_set_int_member(information, "RAMcapacity", infor->ram_capacity);
	json_object_set_int_member(information, "ID", infor->id);

	json_object_set_int_member(device_state, "CPUusage", state->cpu_usage);
	json_object_set_int_member(device_state, "GPUusage", state->gpu_usage);
	json_object_set_int_member(device_state, "RAMusage", state->ram_usage);

	Message* message;

	json_object_set_object_member(message, "DeviceState", device_state);
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



