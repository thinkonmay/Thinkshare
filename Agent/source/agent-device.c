#include <agent-device.h>
#include <tchar.h>
#include <json-glib/json-glib.h>

#define DIV 1048576



DeviceInformation 
get_device_information() 
{

	DeviceInformation device_info;

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

	device_info.cpu = CPUBrandString;

	MEMORYSTATUSEX statex;
	statex.dwLength = sizeof(statex);
	GlobalMemoryStatusEx(&statex);
	device_info.ram_capacity = (statex.ullTotalPhys / 1024) / 1024;

	IDirect3D9* d3Object = Direct3DCreate9(D3D_SDK_VERSION);
	UINT adaptercount = d3Object->lpVtbl->GetAdapterCount(d3Object);
	D3DADAPTER_IDENTIFIER9* adapters = (D3DADAPTER_IDENTIFIER9*)malloc(sizeof(D3DADAPTER_IDENTIFIER9) * adaptercount);

	for (int i = 0; i < adaptercount; i++)
	{
		d3Object->lpVtbl->GetAdapterIdentifier(d3Object, i, 0, &(adapters[i]));
	}

	device_info.gpu = adapters->Description;

	return device_info;
};

//Function to Update get device state;
DeviceState 
get_device_state() 
{

	DeviceState device_state;

	MEMORYSTATUSEX statex;
	statex.dwLength = sizeof(statex);
	GlobalMemoryStatusEx(&statex);

	device_state.ram_usage = (unsigned long)statex.dwMemoryLoad;

	int _c = 0;

	device_state.cpu_usage = GetCPULoad() * 100;

	return device_state;
};



//Define function to calculate CPU Load
static float 
CalculateCPULoad(unsigned long long idleTicks, unsigned long long totalTicks)
{
	static unsigned long long _previousTotalTicks = 0;
	static unsigned long long _previousIdleTicks = 0;

	unsigned long long totalTicksSinceLastTime = totalTicks - _previousTotalTicks;
	unsigned long long idleTicksSinceLastTime = idleTicks - _previousIdleTicks;


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


//Function to Get CPU Load
float GetCPULoad()
{
	FILETIME idleTime, kernelTime, userTime;
	return GetSystemTimes(&idleTime, &kernelTime, &userTime) ? CalculateCPULoad(FileTimeToInt64(idleTime), FileTimeToInt64(kernelTime) + FileTimeToInt64(userTime)) : -1.0f;
}
/// <summary>
/// TRUE if success
/// FALSE of fail
/// </summary>
/// <param name="self"></param>
/// <param name="command"></param>
/// <returns></returns>
gboolean
send_command_line_to_window(gchar* command)
{
	return CreateProcess("C:\\Windows\\System32\\cmd.exe");​
}​

