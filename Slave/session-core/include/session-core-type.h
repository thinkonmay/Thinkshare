 #pragma once
#ifndef  __SESSION_CORE_TYPE_H__
#define __SESSION_CORE_TYPE_H__

#include <json-glib-1.0/json-glib/json-glib.h>

/*session core state*/ 
#define SESSION_CORE_INITIALIZING					"Session core initializing"
#define	SESSION_INFORMATION_SETTLED					"Session information settled"

#define SESSION_REGISTERED							"Session registered"

#define SESSION_HANDSHAKING							"Session handshaking"

#define REMOTE_CONNECT_STARTED						"Remote connect started"



/*Pipeline state macros*/
#define PIPELINE_NOT_READY							"Pipeline not ready"

#define PIPELINE_READY								"Pipeline ready"

#define PIPELINE_CREATING_ELEMENT					"Pipeline creating element"

#define PIPELINE_SETTING_UP_ELEMENT					"Pipeline setting up element"

#define PIPELINE_CONNECT_ELEMENT_SIGNAL				"Pipeline connect element signal"

#define	PIPELINE_SETUP_DONE							"Pipeline setup done"


/*SignallingState macros*/
#define SIGNALLING_SERVER_NOT_READY					"Signalling server not ready"

#define SIGNALLING_SERVER_READY						"Signalling server ready"

#define	SIGNALLING_SERVER_CONNECTING				"Signalling server connecting"

#define	SIGNALLING_SERVER_CONNECTED					"Signalling server connected"            /* Ready to register */

#define	SIGNALLING_SERVER_REGISTERING				"Signalling server registering"

#define	SIGNALLING_SERVER_REGISTER_DONE				"Signalling server registering done"            /* Ready to call a peer */

#define	SIGNALLING_SERVER_CLOSED					"Signalling server closed"            /* server connection closed by us or the server */

/*PEER CALL MACROS*/
#define PEER_CALL_NOT_READY							"Peer call not ready"

#define PEER_CALL_READY								"Peer call ready"

#define	PEER_CALL_NEGOTIATING						"Peer call negotiating"

#define PEER_CALL_DONE								"Peer call done"










typedef struct 			_Pipeline 				Pipeline;

typedef struct 			_SessionCore 			SessionCore;

typedef struct 			_QoE					QoE;

typedef struct			_Session				Session;

typedef struct			_WebRTCHub				WebRTCHub;

typedef struct 			_SignallingHub			SignallingHub;

typedef	struct			_IPC					IPC;





typedef					gchar*					CoreState;

typedef					gchar*					PipelineState;

typedef					gchar*					SignallingServerState;

typedef					gchar*					PeerCallState;

typedef void            (*ProcessBitrateCalculation) (SessionCore* core);

#endif // ! __SESSION_CORE_TYPE_H__


