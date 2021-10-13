#pragma once
#ifndef __AGNET_DEVICE_H__
#define __AGENT_DEVICE_H__

#include <stdio.h>
#include <sysinfoapi.h>
#include <d3d9.h>
#include <intrin.h>
#include <glib.h>
#include <agent-type.h>





/// <summary>
/// update device thread function,
/// invoke during agent object initialization
/// </summary>
/// <param name="data"></param>
/// <returns></returns>
gpointer				update_device					(AgentObject* data);



Message*                get_registration_message        (gint id);



#endif
