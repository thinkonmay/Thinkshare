#pragma once
#ifndef __AGENT_TYPE_H__
#define __AGENT_TYPE_H__

#include <glib.h>
#include <json-glib/json-glib.h>

#include <Windows.h>



/// <summary>
/// Socket is a object use to manage connection with host, 
/// included send and receive,
/// </summary>
typedef struct _Socket 				Socket;

/// <summary>
/// device state is the hardware state of slave device 
/// included gpu and ram usage
/// </summary>
typedef struct _DeviceState			DeviceState;

/// <summary>
/// device information use to register 
/// information with host, include GPU, CPU, RAM
/// </summary>
typedef struct _DeviceInformation 	DeviceInformation;

/// <summary>
/// agent object is an object use to abstract agent module
/// </summary>
typedef struct _AgentObject			AgentObject;

/// <summary>
/// Child pipe contain standard i/o pipe of child process of agent
/// </summary>
typedef struct _ChildPipe			ChildPipe;

/// <summary>
/// childprocess object abstract the existen of child process
/// </summary>
typedef	struct _ChildProcess		ChildProcess;

/// <summary>
/// state of slave device, use to manage session service state of the slave device
/// and terminate session core.
/// </summary>
typedef struct _AgentState			AgentState;

/// <summary>
/// 
/// </summary>
typedef struct _ShellSession        ShellSession;


typedef struct _FileTransferSession FileTransferSession;

/// <summary>
/// 
/// </summary>
typedef struct _FileCompressor      FileCompressor;

/// <summary>
/// 
/// </summary>
typedef struct _FileTransceiver     FileTransceiver;

/// <summary>
/// for better maintainance, 
/// message will be use to replace JsonObject
/// </summary>
typedef		   JsonObject			Message;

/// <summary>
/// child standard input output handle, 
/// this function is settled inside a looped thread 
/// and sould be invoked when child process emit output to stdout  
/// </summary>
typedef void  (*ChildStdHandle)    (GBytes* buffer,
                                     gint process_id,
                                     AgentObject* agent);
/// <summary>
/// child state handle function is setteled inside a looped thread 
/// and responsible for manage state of child process thread
/// (Ex: report session disconnected to host if session core process ended)
/// </summary>
typedef void  (*ChildStateHandle)  (ChildProcess* ProcessID,
                                    DWORD ProcessState,
                                    AgentObject* agent);






#endif