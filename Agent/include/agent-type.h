#pragma once
#ifndef __AGENT_TYPE_H__
#define __AGENT_TYPE_H__

#include <glib.h>
#include <json-glib/json-glib.h>

/*opcode*/
#define	SESSION_INFORMATION						1
#define	BITRATE_CALIBRATE						2

#define	REGISTER_SLAVE							3
#define	SLAVE_ACCEPTED							4

#define	REJECT_SLAVE							5
#define	SLAVE_REJECTED							6

#define	UPDATE_SLAVE_STATE						7

#define	SESSION_INITIALIZE						8
#define	SESSION_TERMINATE						9
#define	RECONNECT_REMOTE_CONTROL				10
#define	DISCONNECT_REMOTE_CONTROL				11

#define	SESSION_INITIALIZE_CONFIRM				12
#define	SESSION_TERMINATE_CONFIRM				13
#define	RECONNECT_REMOTE_CONTROL_CONFIRM		14
#define	DISCONNECT_REMOTE_CONTROL_CONFIRM		15

#define SESSION_CORE_STARTUP_DONE				16
#define AGENT_END								17

#define	SESSION_INITIALIZE_FAILED				18
#define	SESSION_TERMINATE_FAILED				19
#define	RECONNECT_REMOTE_CONTROL_FAILED			20
#define	DISCONNECT_REMOTE_CONTROL_FAILED		21

/*module*/
#define	CORE_MODULE								0
#define	CLIENT_MODULE							1
#define	LOADER_MODULE							2
#define	AGENT_MODULE							3
#define	HOST_MODULE								4


/*agent state*/
#define			ATTEMP_TO_RECONNECT			0
#define			AGENT_CLOSED				1
#define			ON_SESSION					2
#define			ON_SESSION_OFF_REMOTE		3
#define			AGENT_OPEN					4
#define			SLAVE_REGISTERING			5
#define			AGENT_NEW					6


/*SESSION CORE state*/
#define	CORE_STATE_UNKNOWN               0
#define	CORE_STATE_ERROR                 1              /* generic error */

#define	SERVER_CONNECTING               1000
#define SERVER_CONNECTION_ERROR         1001
#define	SERVER_CONNECTED                1003            /* Ready to register */
#define	SERVER_REGISTERING              2000
#define	SERVER_REGISTRATION_ERROR       2001
#define	SERVER_REGISTERED               2002            /* Ready to call a peer */
#define	SERVER_CLOSED                   2003            /* server connection closed by us or the server */

#define	PEER_CONNECTING                 2004
#define	PEER_CONNECTION_ERROR           2005
#define	PEER_CALL_NEGOTIATING           4000
#define	PEER_CALL_STOPPING              4001
#define	PEER_CALL_ERROR                 4002

#define	SESSION_DENIED                  4003
#define	SESSION_INFORMATION_SETTLED     4004  

#define	WAITING_SESSION_INFORMATION     4005
#define	PIPELINE_SETUP_DONE             4006
#define	DATA_CHANNEL_CONNECTED          4007
#define	HANDSHAKE_SIGNAL_CONNECTED      4008

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

typedef struct _Session 			Session;

typedef struct _IPC 				IPC;

typedef struct _LocalStorage  		LocalStorage;

typedef struct _DeviceState			DeviceState;

typedef struct _DeviceInformation 	DeviceInformation;

typedef struct _AgentObject			AgentObject;

typedef		   JsonObject			Message;

typedef		   gint					CoreState;

typedef		   gint					AgentState;

typedef		   gint					Module;

typedef		   gint					Opcode;

typedef		   gint					CoreState;































#endif