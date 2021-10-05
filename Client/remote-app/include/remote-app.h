/// <summary>
/// @file remote-app.h
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-05
/// 
/// @copyright Copyright (c) 2021
/// 
#ifndef __REMOTE_APP_H__
#define __REMOTE_APP_H__

#include <remote-app-type.h>
#include <error-code.h>
#include <message-form.h>




/// <summary>
/// setup pipeline, 
/// this function should be called after registered session with signalling server
/// </summary> 
/// <param name="self">session core object</param>
void			remote_app_setup_pipeline				(RemoteApp* self);

/// <summary>
/// session core setup signalling server, 
/// in this step, signalling url will be assigned
/// </summary> 
/// <param name="self">session core object</param>
void	  		remote_app_setup_webrtc_signalling	(RemoteApp* self);

/// <summary>
/// connect to signalling server,
/// this function should be called after all initilize step is done
/// </summary> 
/// <param name="self"></param>
void			remote_app_connect_signalling_server	(RemoteApp* self);
	
/// <summary>
/// session core send message to other module
/// </summary> 
/// <param name="self"></param>
/// <param name="message"></param>
void			remote_app_send_message				(RemoteApp* self,
														 Message* message);

/// <summary>
/// initialize session core, 
/// this function should be called in main function
/// </summary> 
/// <returns>session core object</returns>
RemoteApp*	remote_app_initialize					();

/// <summary>
/// finalize session core object, 
/// this function should be called when an error is occour
/// </summary> 
/// <param name="self"></param>
/// <param name="exit_code">reason of exit</param>
/// <param name="error">error emmited (if available)</param>
void			remote_app_finalize					(RemoteApp* self,
														gint exit_code,
														GError* error);

/// <summary>
/// get pipeline from session core object
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
Pipeline*		remote_app_get_pipeline				(RemoteApp* self);

/// <summary>
/// get webrtchub from session core object, 
/// webrtc hub will be use for data transfer between session core and client 
/// through webrtc datachannel 
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
WebRTCHub*		remote_app_get_rtc_hub				(RemoteApp* self);

/// <summary>
/// get quality of experience from session core object 
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
QoE*			remote_app_get_qoe					(RemoteApp* self);

/// <summary>
/// set state of session core object,
/// </summary>
/// <param name="core">a string represent state of session core</param>
/// <param name="state"></param>
void			remote_app_set_state					(RemoteApp* core, 
														 CoreState state);

/// <summary>
/// get core state from session core object
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
CoreState		remote_app_get_state					(RemoteApp* self);

/// <summary>
/// get signalling hub from session core object,
/// signalling hub will be use to communicate with signalling server
/// for register remote session and exchange sdp and ice 
/// </summary>
/// <param name="core"></param>
/// <returns></returns>
SignallingHub*	remote_app_get_signalling_hub			(RemoteApp* core);


/// <summary>
/// report session core error with server
/// </summary> 
/// <param name="self"></param>
/// <param name="code"></param>
void			report_remote_app_error				(RemoteApp* self,
														 ErrorCode code);

/// <summary>
/// get main context from session core
/// </summary>
/// <param name="core"></param>
/// <returns></returns>
GMainContext*	remote_app_get_main_context			(RemoteApp* core);

#endif 