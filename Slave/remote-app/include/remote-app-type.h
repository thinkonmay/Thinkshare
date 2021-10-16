/// <summary>
/// @file remote-app-type.h
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-05
/// 
/// 
/// @copyright Copyright (c) 2021
#include <json-glib/json-glib.h>
#include <glib-2.0/glib.h>

/*session core state*/ 
#define REMOTE_APP_INITIALIZING					"Session core initializing"
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







/// <summary>
/// Pipeline is a struct contain all GstElement neccessary for
/// session core to encode video and audio
/// </summary> 
typedef struct 			_Pipeline 				                Pipeline;

/// <summary>
/// Session core is a struct represent for session core module
/// </summary> 
typedef struct 			_RemoteApp 			                RemoteApp;

/// <summary>
/// qoe struct responsible for store recorded qoe metrict and other variable related to 
/// quality of experience of session core 
/// </summary> 
typedef struct 			_QoE					                QoE;

/// <summary>
/// webrtchub struct responsible for handle datachannel message from client
/// </summary>
typedef struct			_WebRTCHub				                WebRTCHub;

/// <summary>
/// signalling hub responsible for handle ice candidate and sdp negotiation with client module 
/// through signalling server.
/// </summary>
typedef struct 			_SignallingHub			                SignallingHub;

/// <summary>
/// 
/// </summary>
typedef struct			_HidInput								HidInput;


/// <summary>
/// session core state in form of string
/// </summary>
typedef					gchar*					                CoreState;

/// <summary>
/// PipelineState in form of string
/// </summary>
typedef					gchar*					                PipelineState;

/// <summary>
/// signalling state in form of string
/// </summary>
typedef					gchar*					                SignallingServerState;

/// <summary>
/// peer call state in form of string
/// </summary>
typedef					gchar*					                PeerCallState;




