#include <agent-device.h>
#include <agent-object.h>
#include <agent-message.h>

#include <logging.h>
#include <general-constant.h>

#include <sysinfoapi.h>
#include <Windows.h>
#include <stdio.h>
#include <tchar.h>
#include <json-glib/json-glib.h>

#define DIV 1048576
#define ID  0

#include <intrin.h>







/// <summary>
/// Information about slave hardware configuration
/// </summary>
struct _DeviceInformation
{
	gchar cpu[100];
	gchar gpu[512];
	gint ram_capacity;
	gchar OS[100];
};

struct _DeviceState
{
	gint cpu_usage;
	gint gpu_usage;
	gint ram_usage;
};










DeviceInformation*
get_device_information() 
{
	DeviceInformation* device_info = malloc(sizeof(DeviceInformation));
	ZeroMemory(device_info, sizeof(DeviceInformation));

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
	memcpy(device_info->cpu , CPUBrandString,64);

	MEMORYSTATUSEX statex;
	statex.dwLength = sizeof(statex);
	GlobalMemoryStatusEx(&statex);
	guint64 ram_cap = (statex.ullTotalPhys / 1024) / 1024;

	device_info->ram_capacity = ram_cap;

	IDirect3D9* d3Object = Direct3DCreate9(D3D_SDK_VERSION);
	UINT adaptercount = d3Object->lpVtbl->GetAdapterCount(d3Object);
	D3DADAPTER_IDENTIFIER9* adapters = (D3DADAPTER_IDENTIFIER9*)malloc(sizeof(D3DADAPTER_IDENTIFIER9) * adaptercount);

	for (int i = 0; i < adaptercount; i++)
	{
		d3Object->lpVtbl->GetAdapterIdentifier(d3Object, i, 0, &(adapters[i]));
	}

	memcpy(device_info->gpu , &adapters->Description,512);


#ifdef WIN32

	gchar OS[100] ;
	ZeroMemory(&OS, sizeof(OS));
	DWORD minor_version = (HIBYTE(LOWORD(GetVersion())));
	DWORD major_version = (LOBYTE(LOWORD(GetVersion())));

	gchar major[5];
	gchar minor[5];


	itoa(major_version, major, 10);
	itoa(minor_version, minor, 10);

	strcat(OS, "Window10 Version ");
	strcat(OS, major);
	strcat(OS, ".");
	strcat(OS, minor);
#endif // 



	memcpy(device_info->OS , &OS,strlen(OS));

	return device_info;
}


Message*
get_registration_message(gint id)
{
	DeviceInformation* infor = get_device_information();
	JsonObject* information = json_object_new();

	json_object_set_string_member(information,	"CPU", infor->cpu);
	json_object_set_string_member(information,	"GPU", infor->gpu);
	json_object_set_string_member(information,	"OS", infor->OS);
	json_object_set_int_member(information,		"RAMcapacity", infor->ram_capacity);
	json_object_set_int_member(information,		"ID", id);
	return information;
}
