/// <summary>
/// @file session-core-pipeline.h
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-05
/// 
/// @copyright Copyright (c) 2021
/// 
#include <session-core-type.h>
#include <gst/gst.h>

#include <session-core-type.h>
#include <qoe.h>




/// <summary>
/// setup pipeline then start stream, 
/// the stream will include audio and video
/// </summary>
/// <param name="core"></param>
/// <returns></returns>
gpointer			setup_pipeline					(SessionCore* core);


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
Pipeline*			pipeline_initialize				(SessionCore* core);

/// <summary>
/// 
/// </summary>
/// <param name="pipe"></param>
/// <returns></returns>
PipelineState		pipeline_get_state				(Pipeline* pipe);

/// <summary>
/// set current state of pipeline
/// </summary>
/// <param name="pipe"></param>
/// <param name="state"></param>
void                pipeline_set_state              (Pipeline* pipe,
                                                     PipelineState state);




/// <summary>
/// get audio encoder from pipeline
/// </summary>
/// <param name="pipe"></param>
/// <param name="audio">audio codec of pipeline</param>
/// <returns></returns>
GstElement*         pipeline_get_audio_encoder      (Pipeline* pipe,
                                                     Codec audio);

/// <summary>
/// get video encoder from pipeline
/// </summary>
/// <param name="pipe"></param>
/// <param name="video">video codec</param>
/// <returns></returns>
GstElement*         pipeline_get_video_encoder      (Pipeline* pipe,
                                                     Codec video);


void                toggle_pointer                  (gboolean toggle, 
                                                     SessionCore* core);
