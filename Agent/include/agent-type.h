#pragma once
#ifndef __AGENT_TYPE_H__
#define __AGENT_TYPE_H__

#include <glib.h>
#include <json-glib/json-glib.h>

typedef enum
{
        SESSION_INFORMATION	,

        REGISTER_SLAVE	,

        SLAVE_ACCEPTED	,
        DENY_SLAVE	,

        REJECT_SLAVE,

        SESSION_INITIALIZE,
        SESSION_TERMINATE,
        RECONNECT_REMOTE_CONTROL,
        DISCONNECT_REMOTE_CONTROL,

        CHANGE_MEDIA_MODE	,
        CHANGE_RESOLUTION,
        CHANGE_FRAMERATE,
        BITRATE_CALIBRATE	,

        COMMAND_LINE_FORWARD,

        EXIT_CODE_REPORT,
        ERROR_REPORT,

        NEW_COMMAND_LINE_SESSION   ,
        END_COMMAND_LINE_SESSION  ,
}Opcode;

typedef enum
{
        CORE_MODULE	,
        CLIENT_MODULE,
        LOADER_MODULE,
        AGENT_MODULE,
        HOST_MODULE	,
}Module;

/*agent state*/
#define			AGENT_UNREGISTERED				1
#define			SLAVE_REGISTERING				2
#define			AGENT_OPEN						3
#define			ON_SESSION						4
#define			ON_SESSION_OFF_REMOTE			5
#define			AGENT_DISCONNECTED				6

/*
*
* 
* Object header file contain object hierarchy structure of  
* 
* 
* 
* 
* 
* 
*/


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