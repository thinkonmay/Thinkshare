#pragma once
#ifndef __AGENT_TYPE_H__
#define __AGENT_TYPE_H__


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




#define	CORE									0
#define	CLIENT									1
#define	LOADER									2
#define	AGENT									3
#define	HOST									4

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

typedef struct _Message 			Message;

typedef struct _SessionQoE 			SessionQoE;

typedef struct _Session 			Session;

typedef struct _IPC 				IPC;

typedef struct _LocalStorage  		LocalStorage;

typedef struct _DeviceState			DeviceState;

typedef struct _DeviceInformation 	DeviceInformation;




/// <summary>
/// if agent is disconnected from host, disconnection state is reported to decide if it is reconnect or not
/// </summary>
typedef enum 
{
	HOST_CONNECTION_ERROR,
	HOST_CONNECTION_FORCE_END
}DisconnectState;


































#endif