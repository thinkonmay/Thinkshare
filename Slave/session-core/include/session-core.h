/// <summary>
/// @file session-core.h
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-05
/// 
/// @copyright Copyright (c) 2021
/// 
#ifndef __SESSION_CORE_H__
#define __SESSION_CORE_H__

#include <session-core-type.h>
#include <error-code.h>
#include <message-form.h>




/// <summary>
/// setup pipeline, 
/// this function should be called after registered session with signalling server
/// </summary> 
/// <param name="self">session core object</param>
void			session_core_setup_pipeline				(SessionCore* self);

/// <summary>
/// session core setup signalling server, 
/// in this step, signalling url will be assigned
/// </summary> 
/// <param name="self">session core object</param>
void	  		session_core_setup_webrtc_signalling	(SessionCore* self);

/// <summary>
/// connect to signalling server,
/// this function should be called after all initilize step is done
/// </summary> 
/// <param name="self"></param>
void			session_core_connect_signalling_server	(SessionCore* self);
	
/// <summary>
/// session core send message to other module
/// </summary> 
/// <param name="self"></param>
/// <param name="message"></param>
void			session_core_send_message				(SessionCore* self,
														 Message* message);

/// <summary>
/// initialize session core, 
/// this function should be called in main function
/// </summary> 
/// <returns>session core object</returns>
SessionCore*	session_core_initialize					();

/// <summary>
/// finalize session core object, 
/// this function should be called when an error is occour
/// </summary> 
/// <param name="self"></param>
/// <param name="exit_code">reason of exit</param>
/// <param name="error">error emmited (if available)</param>
void			session_core_finalize					(SessionCore* self,
														gint exit_code,
														GError* error);

/// <summary>
/// get pipeline from session core object
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
Pipeline*		session_core_get_pipeline				(SessionCore* self);

/// <summary>
/// get webrtchub from session core object, 
/// webrtc hub will be use for data transfer between session core and client 
/// through webrtc datachannel 
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
WebRTCHub*		session_core_get_rtc_hub				(SessionCore* self);

/// <summary>
/// get quality of experience from session core object 
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
QoE*			session_core_get_qoe					(SessionCore* self);

/// <summary>
/// set state of session core object,
/// </summary>
/// <param name="core">a string represent state of session core</param>
/// <param name="state"></param>
void			session_core_set_state					(SessionCore* core, 
														 CoreState state);

/// <summary>
/// get core state from session core object
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
CoreState		session_core_get_state					(SessionCore* self);

/// <summary>
/// get signalling hub from session core object,
/// signalling hub will be use to communicate with signalling server
/// for register remote session and exchange sdp and ice 
/// </summary>
/// <param name="core"></param>
/// <returns></returns>
SignallingHub*	session_core_get_signalling_hub			(SessionCore* core);

/// <summary>
/// get ipc (inter process communication object from session core object)
/// ipc object will be use to exchange data with agent object
/// </summary>
/// <param name="core"></param>
/// <returns></returns>
IPC*			session_core_get_ipc					(SessionCore* core);

/// <summary>
/// report session core error with server
/// </summary> 
/// <param name="self"></param>
/// <param name="code"></param>
void			report_session_core_error				(SessionCore* self,
														 ErrorCode code);

/// <summary>
/// get main context from session core
/// </summary>
/// <param name="core"></param>
/// <returns></returns>
GMainContext*	session_core_get_main_context			(SessionCore* core);

#endif 