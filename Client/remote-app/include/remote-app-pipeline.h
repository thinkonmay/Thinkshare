/// <summary>
/// @file remote-app-pipeline.h
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-05
/// 
/// @copyright Copyright (c) 2021
/// 
#include <remote-app-type.h>
#include <gst/gst.h>

#include <remote-app-type.h>
#include <qoe.h>




/// <summary>
/// setup pipeline then start stream, 
/// the stream will include audio and video
/// </summary>
/// <param name="core"></param>
/// <returns></returns>
gpointer			setup_pipeline					(RemoteApp* core);


/// <summary>
/// get GstElement pipeline of this pipeline
/// </summary>
/// <param name="pipeline"></param>
/// <returns>pipeline element of </returns>
GstElement*			pipeline_get_pipline			(Pipeline* pipeline);

/// <summary>
/// get webrtcbin element from pipeline
/// </summary>
/// <param name="pipeline"></param>
/// <returns></returns>
GstElement*			pipeline_get_webrtc_bin			(Pipeline* pipeline);

/// <summary>
/// initliaze pipeline,
/// include assigning memory to pipeline
/// </summary>
/// <returns></returns>
Pipeline*			pipeline_initialize				(RemoteApp* core);