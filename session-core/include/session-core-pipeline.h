#include <session-core-type.h>
#include <gst/gst.h>






gboolean			connect_signalling_handler		(SessionCore* core);

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
Pipeline*			pipeline_initialize				();

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

