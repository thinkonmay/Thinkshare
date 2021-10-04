/// <summary>
/// @file file-transfer-pipeline.h
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-05
/// 
/// @copyright Copyright (c) 2021
/// 
#ifndef __FILE_TRANSFER_WEBRTCBIN_H__
#define __FILE_TRANSFER_WEBRTCBIN_H__
#include "file-transfer-type.h"
#include <gst/gst.h>

#include <qoe.h>




/// <summary>
/// setup pipeline then start stream, 
/// the stream will include audio and video
/// </summary>
/// <param name="core"></param>
/// <returns></returns>
void                setup_webrtcbin                     (FileTransferService* core);



/// <summary>
/// initliaze pipeline,
/// include assigning memory to pipeline
/// </summary>
/// <returns></returns>
WebRTCbin*			webrtcbin_initialize				(FileTransferService* core);


/// <summary>
/// 
/// </summary>
/// <param name="hub"></param>
/// <param name="turn_connection"></param>
void                webrtcbin_get_turn_connection       (WebRTCbin* hub, 
                                                         gchar* turn_connection);

/// <summary>
/// 
/// </summary>
/// <param name="bin"></param>
/// <returns></returns>
GstElement*         webrtcbin_get_element               (WebRTCbin* bin);
#endif 