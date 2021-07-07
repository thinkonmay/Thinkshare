#pragma once
#ifndef  __SESSION_CORE_TYPE_H__
#define __SESSION_CORE_TYPE_H__

#include <json-glib-1.0/json-glib/json-glib.h>

/*session core state*/
#define	CORE_STATE_UNKNOWN               0
#define	CORE_STATE_ERROR                 1              /* generic error */

#define WAITING_SESSION_INFORMATION		 2
#define	SESSION_INFORMATION_SETTLED      4  

#define	SESSION_DENIED			         5
#define SESSION_REGISTERED				 6	

#define REMOTE_CONNECT_STARTED			 7





/*pipeline macros*/

#define PIPELINE_INITIALIZED			 1
#define PIPELINE_SETTING_UP				 2

#define	PIPELINE_ELEMENT_LINKED			 5


#define	PIPELINE_SETUP_DONE              3
#define PIPELINE_STARTED				 4











/*SignallingState macros*/
#define SIGNALLING_SERVER_NOT_READY					998
#define SIGNALLING_SERVER_READY						999

#define	SIGNALLING_SERVER_CONNECTING				1000
#define SIGNALLING_SERVER_CONNECTION_ERROR          1001
#define	SIGNALLING_SERVER_CONNECTED					1003            /* Ready to register */

#define	SIGNALLING_SERVER_REGISTERING				2000
#define	SIGNALLING_SERVER_REGISTRATION_ERROR		2001
#define	SIGNALLING_SERVER_REGISTERED				2002            /* Ready to call a peer */
#define	SIGNALLING_SERVER_CLOSED					2003            /* server connection closed by us or the server */

/*PeerCallState macors*/          
#define	PEER_CALL_NEGOTIATING           4000
#define	PEER_CALL_ERROR                 4002
#define PEER_CALL_STARTED				4003

#define PEER_CALL_NOT_READY				4004	
#define	PEER_CONNECTION_ERROR			4005
#define PEER_CALL_READY












/*opcode macros*/
#define	SESSION_INFORMATION						1
#define	BITRATE_CALIBRATE						2

#define	REGISTER_SLAVE							3
#define	SLAVE_ACCEPTED							4

#define	REJECT_SLAVE							5
#define	SLAVE_REJECTED							6

#define	UPDATE_SLAVE_STATE						7

#define	SESSION_INITIALIZE						8
#define	SESSION_TERMINATE						9
#define	RECONNECT_REMOTE_CONTROL				10
#define	DISCONNECT_REMOTE_CONTROL				11

#define	SESSION_INITIALIZE_CONFIRM				12
#define	SESSION_TERMINATE_CONFIRM				13
#define	RECONNECT_REMOTE_CONTROL_CONFIRM		14
#define	DISCONNECT_REMOTE_CONTROL_CONFIRM		15

#define SESSION_INFORMATION_REQUEST				16
#define AGENT_END								17

#define	CHANGE_MEDIA_MODE						18
#define	COMPOSE_BITRATE							19
#define	TOGGLE_CURSOR							20
#define	CHANGE_RESOLUTION						21	
#define	CHANGE_FRAMERATE						22
#define	AGENT_MESSAG							23

#define DATA_CHANNEL_ERROR						24	








/*module macros*/
#define	CORE_MODULE								0
#define	CLIENT_MODULE							1
#define	LOADER_MODULE							2
#define	AGENT_MODULE							3
#define	HOST_MODULE								4

/*codec macros*/
#define	CODEC_NVH265							0
#define	CODEC_NVH264							1
#define	CODEC_VP9								2
#define OPUS_ENC								3


#define AUDIO_PIORITY							4
#define VIDEO_PIORITY							5

/*session core exit code*/



typedef struct 			_Pipeline 				Pipeline;

typedef struct 			_SessionCore 			SessionCore;

typedef 				JsonObject				Message;

typedef struct 			_QoE					QoE;

typedef struct			_Session				Session;

typedef struct			_SessionQoE				SessionQoE;

typedef 				int						Codec;

typedef					int						CoreState;

typedef struct			_WebRTCHub				WebRTCHub;

typedef struct 			_SignallingHub			SignallingHub;

typedef					int						QoEMode;

typedef					gint					Module;

typedef					gint					Opcode;

typedef					gint					SignallingServerState;

typedef					gint					PeerCallState;

typedef	struct			_IPC					IPC;

typedef					gint					PipelineState;



#endif // ! __SESSION_CORE_TYPE_H__


