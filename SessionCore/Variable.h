#pragma once
#include "Framework.h"
#include "Handle data channel.h"
#include "Handle pipeline.h"
#include "RC config.h"
#include "CorePipeSink.h"
#include "Signalling handling.h"

#define STUN_SERVER "stun-server=stun://stun.l.google.com:19302"

#define OUR_ID 205


#define BYTE_PIPE_NAME L"\\\\.\\pipe\\byte_loader"
#define STR_PIPE_NAME L"\\\\.\\pipe\\str_loader"



/*Define app_state in order to track state of application*/
enum AppState
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
    SESSION_DENIED
};
enum Media_mode
{
    AUDIO_PIORITY,
    VIDEO_PIORITY
};

static enum Media_mode mode;

enum Message_type 
{
    CHANGE_MEDIA_MODE,
    COMPOSE_BITRATE,
    TOGGLE_CURSOR,
    CHANGE_RESOLUTION,
    CHANGE_FRAMERATE,
};

static gint screen_width, screen_height, framerate;

static GMainLoop* loop;
static GstElement* pipeline, * webrtcbin;

static SoupWebsocketConnection* ws_conn = NULL;
static const gchar* peer_id = NULL;
static const gchar* server_url = "fill here";
static gboolean disable_ssl = FALSE;
static gboolean Client_send_offer_first = TRUE;
static GObject* SessionLoader, * SessionCore;

static const gchar* stun_server = STUN_SERVER;

static gint SessionSlaveID = OUR_ID;

static gboolean toggle_cursor;

static HANDLE file_handle_byte, file_handle_string;