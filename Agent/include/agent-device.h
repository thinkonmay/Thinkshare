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

/// <summary>
/// get device information directly from window,
/// wrapped by device state update thread
/// </summary>
/// <returns></returns>
DeviceInformation* 				get_device_information						(void);
/// <summary>
/// get device state directly from window,
/// wrapped by device state update thread
/// </summary>
/// <returns></returns>
DeviceState* 					get_device_state							(void);

/// <summary>
/// 
/// </summary>
/// <param name="infor"></param>
/// <returns></returns>
Message*						get_json_message_from_device_information	(DeviceInformation* infor);


Message*						get_json_message_from_device				(AgentObject* object);

#endif
