/// <summary>
/// @file file-transfer-type.h
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-05
/// 
/// 
/// @copyright Copyright (c) 2021

#include <json-glib-1.0/json-glib/json-glib.h>
#include <glib-2.0/glib.h>



/// <summary>
/// WebRTChub is a struct contain all GstElement neccessary for
/// session core to encode video and audio
/// </summary> 
typedef struct 			_WebRTChub 				                WebRTChub;

/// <summary>
/// Session core is a struct represent for session core module
/// </summary> 
typedef struct 			_FileTransferSvc 			            FileTransferSvc;

/// <summary>
/// WebRTCDataChannelPool struct responsible for handle datachannel message from client
/// </summary>
typedef struct			_WebRTCDataChannelPool				    WebRTCDataChannelPool;

/// <summary>
/// signalling hub responsible for handle ice candidate and sdp negotiation with client module 
/// through signalling server.
/// </summary>
typedef struct 			_SignallingHub			                SignallingHub;



