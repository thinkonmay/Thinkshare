#pragma once
#include "Framework.h"
#include "Object.h"
#include "agent-object.h"





/// <summary>
/// state of agent block
/// </summary>
typedef enum Agent_State
{
	AGENT_STATE_UNKNOWN,
	AGENT_STATE_ERROR,
	AGENT_STATE_CLOSED,
	HOST_CONNECTED_OFF_SESSION,
	HOST_CONNECTING,
	HOST_REGISTERING,
	HOST_DISCONNECTED,
	ON_REMOTE_CONTROL,
	DISCONNECTED_REMOTE_CONTROL	
}AgentState;


/// <summary>
/// if agent is disconnected from host, disconnection state is reported to decide if it is reconnect or not
/// </summary>
typedef enum 
{
	HOST_CONNECTION_ERROR,
	HOST_CONNECTION_FORCE_END
}DisconnectState;


typedef enum
{
	AUDIO_PIORITY,
	VIDEO_PIORITY
}QoEMode;


/// <summary>
/// Encoder (currently only nvh264 available)
/// </summary>
typedef enum
{
	CODEC_NVH265,
	CODEC_NVH264,
	CODEC_VP9
}Encoder;


/// <summary>
/// message opcode to identify message type
/// </summary>
typedef enum
{
	SESSION_INFORMATION,
	BITRATE_CALIBRATE,
	CLIENT_MESSAGE,


	OPCODE_LAST
}Opcode;

typedef enum
{
	CORE,
	CLIENT,
	LOADER,
	AGENT,
	HOST
}Location;


/// <summary>
/// Message template to send to all device in one session
/// </summary>
typedef struct
{
	Opcode opcode;
	gint from;
	gint to;
	gpointer data;
}Message;


/// <summary>
/// state of SessionCore (used to report session core bug to host)
/// </summary>
typedef enum
{
	APP_STATE_UNKNOWN = 0,
	APP_STATE_ERROR = 1,          /* generic error */
	SERVER_CONNECTING = 1000,
	SERVER_CONNECTION_ERROR,
	SERVER_CONNECTED,             /* Ready to register */
	SERVER_REGISTERING = 2000,
	SERVER_REGISTRATION_ERROR,
	SERVER_REGISTERED,            /* Ready to call a peer */
	SERVER_CLOSED,                /* server connection closed by us or the server */
	PEER_CONNECTING = 3000,
	PEER_CONNECTION_ERROR,
	PEER_CONNECTED,
	PEER_CALL_NEGOTIATING = 4000,
	PEER_CALL_STARTED,
	PEER_CALL_STOPPING,
	PEER_CALL_STOPPED,
	PEER_CALL_ERROR,
	SESSION_DENIED,
	SESSION_INFORMATION_SETTLED,
	WAITING_SESSION_INFORMATION,
	PIPELINE_SETUP_DONE,
	DATA_CHANNEL_CONNECTED,
	HANDSHAKE_SIGNAL_CONNECTED,

	CORE_STATE_LAST
}CoreState;


/// <summary>
/// QoE controller (calibrate video stream bitrate in order to 
/// optimize quality of experience with fluctuate network bandwidth)
/// </summary>
typedef struct
{
	GstControlSource* audio;
	GstControlSource* video;

	QoEMode mode;
}QoEController;


/// <summary>
/// SessionQoE (quality of experience) of the remote control session
/// (included QoE controller)
/// </summary>
typedef struct
{
	gint screen_height;
	gint screen_width;
	gint framerate;

	gint compose_bitrate;

	QoEMode* mode;

	Encoder* video_encoder;
	Encoder* audio_encoder;

	QoEController* audio_controller;
	QoEController* video_controller;
}SessionQoE;


/// <summary>
/// Session information (used to initialize session core)
/// </summary>
typedef struct
{
	gint SessionSlaveID;

	gchar* signalling_url;

	SessionQoE* qoe;

	gboolean disable_ssl;

	gboolean client_offer;

	gchar* stun_server;

	CoreState state;
}Session;

/// <summary>
/// information about local storage to add to the session
/// local storage will be discovered by slave device before the session
/// </summary>
typedef struct
{
	gchar* drive_name;

	gchar* url;

	gchar* group_name;
	gchar* user_name;
	gchar* password;
}LocalStorage;


/// <summary>
/// contain information about shared memory hub and connection
/// </summary>
typedef struct
{
	SharedMemoryHub* hub;
	SharedMemoryLink* core_link;
	SharedMemoryLink* loader_link;

	const gint core_id;
	const gint loader_id;
	const gint agent_id;
	const gint host_id;
	const gint client_id;

	gint block_size;
	gint pipe_size;
}IPC;

/// <summary>
/// contain information about websocket socket with host
/// </summary>
typedef struct
{
	SoupWebsocketConnection* ws;

	gchar* host_url;


}Socket;





/// <summary>
/// Information about slave hardware configuration
/// </summary>
typedef struct
{
	gchar* cpu;
	gchar* gpu;
	gint ram_capacity;
	gchar* OS;
}DeviceInformation;

typedef struct
{
	gint cpu_usage;
	gint gpu_usage;
	gint ram_usage;


}DeviceState;