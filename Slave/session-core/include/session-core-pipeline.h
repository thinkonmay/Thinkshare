#include <session-core-type.h>
#include <gst/gst.h>

#include <session-core-type.h>
#include <qoe.h>


/// <summary>
/// 
/// </summary>
/// <param name="core"></param>
void				connect_signalling_handler		(SessionCore* core);

/// <summary>
/// 
/// </summary>
/// <param name="core"></param>
/// <returns></returns>
gpointer			setup_pipeline					(SessionCore* core);

/// <summary>
/// 
/// </summary>
/// <param name="core"></param>
/// <returns></returns>
gboolean			start_pipeline					(SessionCore* core);

/// <summary>
/// 
/// </summary>
/// <param name="pipeline"></param>
/// <returns></returns>
GstElement*			pipeline_get_pipline			(Pipeline* pipeline);

/// <summary>
/// 
/// </summary>
/// <param name="pipeline"></param>
/// <returns></returns>
GstElement*			pipeline_get_webrtc_bin			(Pipeline* pipeline);

/// <summary>
/// 
/// </summary>
/// <returns></returns>
Pipeline*			pipeline_initialize				(SessionCore* core);

/// <summary>
/// 
/// </summary>
/// <param name="core"></param>
void				setup_element_property			(SessionCore* core);


/// <summary>
/// 
/// </summary>
/// <param name="core"></param>
void				setup_element_cap				(SessionCore* core);

/// <summary>
/// 
/// </summary>
/// <param name="pipe"></param>
/// <returns></returns>
PipelineState		pipeline_get_state				(Pipeline* pipe);

/// <summary>
/// 
/// </summary>
/// <param name="pipe"></param>
/// <param name="state"></param>
void                pipeline_set_state              (Pipeline* pipe,
                                                     PipelineState state);


void                stop_pipeline                   (Pipeline* pipe);



GstElement*         pipeline_get_audio_encoder      (Pipeline* pipe,
                                                     Codec video);

GstElement*         pipeline_get_video_encoder      (Pipeline* pipe,
                                                     Codec audio);
