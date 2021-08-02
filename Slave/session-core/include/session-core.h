#ifndef __SESSION_CORE_H__
#define __SESSION_CORE_H__

#include <session-core-type.h>







void			session_core_setup_pipeline				(SessionCore* self);

gboolean		session_core_setup_data_channel			(SessionCore* self);

void	  		session_core_setup_webrtc_signalling	(SessionCore* self);

gboolean		session_core_start_pipeline				(SessionCore* self);

void			session_core_connect_signalling_server	(SessionCore* self);
	
void			session_core_send_message				(SessionCore* self,
														 Message* message);

SessionCore*	session_core_initialize					();

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

void			report_session_core_error				(SessionCore* self,
														 ErrorCode code);

GFile*			session_core_get_session				(SessionCore* core);

GMainContext*	session_core_get_main_context			(SessionCore* core);

GFile*			session_core_get_log_file				(SessionCore* self);

GFile*			session_core_get_device_config			(SessionCore* core);

#endif 