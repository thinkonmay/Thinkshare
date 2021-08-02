 #pragma once
#ifndef  __SESSION_CORE_TYPE_H__
#define __SESSION_CORE_TYPE_H__

#include <json-glib-1.0/json-glib/json-glib.h>

/*session core state*/ 
#define SESSION_CORE_INITIALIZING					1
#define	SESSION_INFORMATION_SETTLED					2  

#define SESSION_REGISTERED							3	

#define SESSION_HANDSHAKING							4

#define REMOTE_CONNECT_STARTED						5





/*pipeline macros*/
#define PIPELINE_NOT_READY							1
#define PIPELINE_READY								2

#define PIPELINE_CREATING_ELEMENT					3
#define PIPELINE_SETTING_UP_ELEMENT					4
#define PIPELINE_CONNECT_ELEMENT_SIGNAL				5
#define	PIPELINE_LINKING_ELEMENT					6

#define	PIPELINE_SETUP_DONE							7




/*SignallingState macros*/
#define SIGNALLING_SERVER_NOT_READY					1
#define SIGNALLING_SERVER_READY						2

#define	SIGNALLING_SERVER_CONNECTING				3
#define	SIGNALLING_SERVER_CONNECTED					4            /* Ready to register */

#define	SIGNALLING_SERVER_REGISTERING				5
#define	SIGNALLING_SERVER_REGISTER_DONE				6            /* Ready to call a peer */
#define	SIGNALLING_SERVER_CLOSED					7            /* server connection closed by us or the server */

/*PEER CALL MACROS*/
#define PEER_CALL_NOT_READY							1
#define PEER_CALL_READY								2

#define	PEER_CALL_NEGOTIATING						3

#define PEER_CALL_DONE								4


/*session core error code*/

#define UNKNOWN_ERROR								0
#define DATA_CHANNEL_ERROR							1

#define UNKNOWN_MESSAGE								2
#define SIGNALLING_ERROR							3

/*session core exit code*/

#define UNKNOWN_ERROR								0
#define	CORE_STATE_CONFLICT					        1              /* generic error */

#define	SIGNALLING_SERVER_REGISTRATION_ERROR		2
#define SIGNALLING_SERVER_CONNECTION_ERROR          3
#define PIPELINE_ERROR								4
#define DATA_CHANNEL_ERROR							5

#define SESSION_DENIED								6
#define CORRUPTED_SESSION_INFORMATION				7
#define PLUGINS_MISSING								8


/*opcode macros*/
#define	SESSION_INFORMATION						1

#define	REGISTER_SLAVE							2

#define	SLAVE_ACCEPTED							3
#define	DENY_SLAVE  							4

#define	REJECT_SLAVE							5

#define	UPDATE_SLAVE_STATE						6

#define	SESSION_INITIALIZE						7
#define	SESSION_TERMINATE						8
#define	RECONNECT_REMOTE_CONTROL				9
#define	DISCONNECT_REMOTE_CONTROL				10

#define SESSION_INFORMATION_REQUEST				11

#define	COMMAND_LINE_FORWARD					12

#define EXIT_CODE_REPORT						13
#define ERROR_REPORT							14
	
#define NEW_COMMAND_LINE_SESSION                15
#define END_COMMAND_LINE_SESSION                16

#define QOE_REPORT								17

/*HID data channel opcode*/
typedef enum
{
	KEYUP,
	KEYDOWN,

	MOUSE_WHEEL,
	MOUSE_MOVE,
	MOUSE_UP,
	MOUSE_DOWN,

	CHANGE_MEDIA_MODE,

	CHANGE_RESOLUTION,
	CHANGE_FRAMERATE,
	DISPLAY_POINTER,

	CLIPBOARD,
	FILE_TRANSFER,
}HidOpcode;


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
#define AAC_ENC									4


#define AUDIO_PIORITY							4
#define VIDEO_PIORITY							5




typedef struct 			_Pipeline 				Pipeline;

typedef struct 			_SessionCore 			SessionCore;

typedef struct 			_QoE					QoE;

typedef struct			_Session				Session;

typedef struct			_SessionQoE				SessionQoE;

typedef struct			_WebRTCHub				WebRTCHub;

typedef struct 			_SignallingHub			SignallingHub;

typedef	struct			_IPC					IPC;

typedef 				JsonObject				Message;





typedef 				gint					Codec;

typedef					gint					QoEMode;



typedef					gint					Opcode;

typedef					gint					Module;



typedef					gint					CoreState;

typedef					gint					PipelineState;

typedef					gint					SignallingServerState;

typedef					gint					PeerCallState;

typedef					gint					ExitCode;

typedef					gint					ErrorCode;



#endif // ! __SESSION_CORE_TYPE_H__


