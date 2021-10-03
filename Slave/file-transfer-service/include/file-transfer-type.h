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
/// WebRTCbin is a struct contain all GstElement neccessary for
/// session core to encode video and audio
/// </summary> 
typedef struct 			_WebRTCbin 				                        WebRTCbin;

/// <summary>
/// Session core is a struct represent for session core module
/// </summary> 
typedef struct 			_FileTransferService 			                FileTransferService;

/// <summary>
/// FileTransferHub struct responsible for handle datachannel message from client
/// </summary>
typedef struct			_FileTransferHub				                FileTransferHub;

/// <summary>
/// signalling hub responsible for handle ice candidate and sdp negotiation with client module 
/// through signalling server.
/// </summary>
typedef struct 			_FileTransferSignalling			                FileTransferSignalling;



