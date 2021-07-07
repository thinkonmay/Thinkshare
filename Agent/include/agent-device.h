#pragma once
#ifndef __AGNET_DEVICE_H__
#define __AGENT_DEVICE_H__

#include <Windows.h>	
#include <stdio.h>
#include <sysinfoapi.h>
#include <d3d9.h>
#include <intrin.h>
#include <glib.h>
#include <agent-type.h>



#pragma comment(lib, "d3d9.lib")

/*Bao should create device information query component in this source file*/;
DeviceInformation* 									get_device_information();

DeviceState* 										get_device_state();

#endif
