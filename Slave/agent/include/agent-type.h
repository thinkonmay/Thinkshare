#pragma once
#ifndef __AGENT_TYPE_H__
#define __AGENT_TYPE_H__

#include <glib.h>
#include <json-glib/json-glib.h>




typedef struct _Socket 				Socket;

typedef struct _DeviceState			DeviceState;

typedef struct _DeviceInformation 	DeviceInformation;

typedef struct _AgentObject			AgentObject;

typedef struct _ChildPipe			ChildPipe;

typedef	struct _ChildProcess		ChildProcess;

typedef struct _AgentState			AgentState;

typedef		   JsonObject			Message;

typedef void  (*ChildHandleFunc)    (GBytes* buffer,
                                     gint process_id,
                                     AgentObject* agent);






#endif