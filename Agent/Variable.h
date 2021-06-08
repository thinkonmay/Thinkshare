#pragma once
#include "Framework.h"
#include "Session initialization.h"
#include "Session termination.h"
#include "Variable.h"
#include "Device Information.h"
#include "AgentPipeSrc.h"
#include "AgentSocket.h"

#define AGENT_PIPE_NAME L"\\\\.\\pipe\\str_agent"




enum Agent_State
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
};

enum Disconnection_State
{
	HOST_CONNECTION_ERROR,
	HOST_CONNECTION_FORCE_END
};

enum Disconnection_State disconnection_state;

enum Agent_State agent_state;

static SoupWebsocketConnection* ws_conn = NULL;

static const gchar* Host_URL = "fill here";

static gboolean disable_ssl = FALSE;

static HANDLE file_handle_string;

static GMainLoop* loop;