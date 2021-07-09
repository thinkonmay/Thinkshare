#ifndef __SESSION_CORE_H__
#define __SESSION_CORE_H__

#include <session-core-type.h>






/// <summary>
/// setup pipeline of sessioncore, 
/// invoked during session initialization,
/// wrap around setup_pipeline function from session core pipeline
/// </summary>
/// <param name="self"></param>
void			session_core_setup_pipeline				(SessionCore* self);


/// <summary>
/// session core setup data channel function,
/// invoke during setup pipeline process,
/// wrap around setup data channel function
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
gboolean		session_core_setup_data_channel			(SessionCore* self);

/// <summary>
/// setup callback function related to webrtc signalling process,
/// wrap aroun setup signalling function
/// </summary>
/// <param name="self"></param>
void	  		session_core_setup_webrtc_signalling	(SessionCore* self);

gboolean		session_core_start_pipeline				(SessionCore* self);

void			session_core_connect_signalling_server	(SessionCore* self);
	
void			session_core_send_message				(SessionCore* self,
														 Message* message);

SessionCore*	session_core_initialize					();


/// <summary>
/// session-core finalize function: 
/// close all remain loop and thread, set state to CLOSED and quit process
/// </summary>
/// <param name="self"></param>
/// <param name="exit_code"></param>
void			session_core_finalize					(SessionCore* self,
														gint exit_code);


Pipeline*		session_core_get_pipeline				(SessionCore* self);

WebRTCHub*		session_core_get_rtc_hub				(SessionCore* self);

SessionQoE*		session_core_get_qoe					(SessionCore* self);

void			session_core_set_state					(SessionCore* core, 
														 CoreState state);

CoreState		session_core_get_state					(SessionCore* self);

void			session_core_setup_session				(SessionCore* object,
														Session* session);

Session*		get_session_information_from_message	(Message* object);

SignallingHub*	session_core_get_signalling_hub			(SessionCore* core);

IPC*			session_core_get_ipc					(SessionCore* core);

#endif 