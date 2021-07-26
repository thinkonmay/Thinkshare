#pragma once
#ifndef __AGENT_TYPE_H__
#define __AGENT_TYPE_H__

#include <glib.h>
#include <json-glib/json-glib.h>

/*opcode*/
#define	SESSION_INFORMATION						1

#define	REGISTER_SLAVE							2

#define	SLAVE_ACCEPTED							3
#define	DENY_SLAVE								4

#define	REJECT_SLAVE							5

#define	UPDATE_SLAVE_STATE						6

#define	SESSION_INITIALIZE						7
#define	SESSION_TERMINATE						8
#define	RECONNECT_REMOTE_CONTROL				9
#define	DISCONNECT_REMOTE_CONTROL				10

#define SESSION_INFORMATION_REQUEST				19

#define	CHANGE_MEDIA_MODE						20
#define	CHANGE_RESOLUTION						21
#define	CHANGE_FRAMERATE						22
#define	BITRATE_CALIBRATE						23

#define	COMMAND_LINE_FORWARD					24

#define EXIT_CODE_REPORT						26
#define ERROR_REPORT							27

#define NEW_COMMAND_LINE_SESSION                19
#define END_COMMAND_LINE_SESSION                20

/*module*/
#define	CORE_MODULE								0
#define	CLIENT_MODULE							1
#define	LOADER_MODULE							2
#define	AGENT_MODULE							3
#define	HOST_MODULE								4


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


typedef struct _QoEMode 			QoEMode;

typedef struct _Socket 				Socket;

typedef struct _SessionQoE 			SessionQoE;

typedef struct _IPC 				IPC;

typedef struct _LocalStorage  		LocalStorage;

typedef struct _DeviceState			DeviceState;

typedef struct _DeviceInformation 	DeviceInformation;

typedef struct _AgentObject			AgentObject;

typedef	struct _CommandLine			CommandLine;

typedef struct _LocalStorage		LocalStorage;

typedef struct _AgentState			AgentState;

typedef		   JsonObject			Session;

typedef		   JsonObject			Message;

typedef		   gint					Module;

typedef		   gint					Opcode;

typedef		   gint					Codec;

typedef		   gint					QoEMode;





























#endif