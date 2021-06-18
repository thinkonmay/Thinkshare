#pragma once

#include <glib-2.0/glib.h>

typedef enum
{
	CHANGE_MEDIA_MODE,
	COMPOSE_BITRATE,
	TOGGLE_CURSOR,
	CHANGE_RESOLUTION,
	CHANGE_FRAMERATE,
	AGENT_MESSAGE
} ClientMessage;

typedef enum
{
	AUDIO_PIORITY,
	VIDEO_PIORITY
}MediaMode;


typedef enum
{
	AUDIO_PIORITY,
	VIDEO_PIORITY
}QoEMode;

typedef enum
{
	CORE,
	CLIENT,
	LOADER,
	AGENT
}Location;

typedef struct
{
	GstElement* pipeline;
	GstElement* webrtcbin;

	GstElement* soundcapture;
	GstElement* soundencode;
	GstElement* rtp_sound; 
	GstElement* rtp_sound_queue;
	GstElement* rtp_sound_cap;
	GstElement* screencapture; 
	GstElement* videoupload;
	GstElement* videoconvert;
	GstElement* gpu_encode;
	GstElement* rtp_packetize;
}Pipeline;

typedef enum
{
	SESSION_INFORMATION,
	BITRATE_CALIBRATE,
	CLIENT_MESSAGE,


	OPCODE_LAST
}Opcode;


typedef struct
{
	Opcode opcode;
	gpointer data;
}Message;

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

typedef struct
{
	gint SlaveID;

	gchar* signalling_url;

	SessionQoE* qoe;

	gboolean disable_ssl;

	gboolean client_offer;

	gchar* stun_server;
}Session;

typedef struct
{
	SharedMemoryHub* hub;
	SharedMemoryLink* link;

	gint core_id;
	gint loader_id;
	gint agent_id;

	gint block_size;
	gint pipe_size;
}IPC;

typedef struct
{
	gint agent_id;
	gint core_id;
	gint loader_id;
}Hub;


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
/// 
/// </summary>
typedef enum
{
	CODEC_NVH265,
	CODEC_NVH264,
	CODEC_VP9
}Encoder;

typedef struct
{
	GstControlSource* audio;
	GstControlSource* video;

	MediaMode mode;
}QoEController;

typedef struct
{
	SoupWebsocketConnection* ws;

	gint slave_id;

	gchar* signalling_url;

	gboolean disable_ssl;

	gchar* stun_server;

	GstWebRTCDataChannel* hid;

	GstWebRTCDataChannel* control;


	gboolean client_offer;
}WebRTCHub;