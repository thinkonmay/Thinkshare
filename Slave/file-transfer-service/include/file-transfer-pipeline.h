/// <summary>
/// @file file-transfer-pipeline.h
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-05
/// 
/// @copyright Copyright (c) 2021
/// 
#include "file-transfer-type.h"
#include <gst/gst.h>

#include <qoe.h>




/// <summary>
/// setup pipeline then start stream, 
/// the stream will include audio and video
/// </summary>
/// <param name="core"></param>
/// <returns></returns>
gpointer			setup_pipeline					    (FileTransferSvc* core);

/// <summary>
/// get webrtcbin element from pipeline
/// </summary>
/// <param name="pipeline"></param>
/// <returns></returns>
GstElement*			pipeline_get_webrtc_bin			    (WebRTChub* pipeline);

/// <summary>
/// initliaze pipeline,
/// include assigning memory to pipeline
/// </summary>
/// <returns></returns>
WebRTChub*			webrtcbin_initialize				(FileTransferSvc* core);

