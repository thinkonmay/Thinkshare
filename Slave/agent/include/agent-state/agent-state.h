#ifndef __AGENT_STATE_H__
#define __AGENT_STATE_H__
#include <glib.h>
#include <agent-type.h>



typedef void            (*SessionTerminate)             (AgentObject* agent);

typedef void            (*SessionInitialize)            (AgentObject* agent);

typedef void            (*RemoteControlDisconnect)      (AgentObject* agent);

typedef void            (*RemoteControlReconnect)       (AgentObject* agent);

typedef void            (*SendMessageToHost)            (AgentObject* agent, gchar* message);

typedef void            (*SendMessageToSessionCore)     (AgentObject* agent,gchar* message);

typedef void            (*SendMessageToSessionLoader)   (AgentObject* agent, gchar* message);

typedef void            (*ConnectToHost)                (AgentObject* agent);

typedef void            (*RegisterToHost)               ( AgentObject* agent);

typedef void            (*OnSessionCoreExit)            (AgentObject* agent);

typedef void            (*OnShellSessionExit)            (AgentObject* agent, gint ProcessID);

typedef gchar*          (*GetCurrentState)              (void);

void                    default_method                  (AgentState* state);

struct _AgentState
{
    SessionTerminate session_terminate;

    SessionInitialize session_initialize;

    RemoteControlDisconnect remote_control_disconnect;

    RemoteControlReconnect remote_control_reconnect;

    SendMessageToHost send_message_to_host;

    SendMessageToSessionCore send_message_to_session_core;

    SendMessageToSessionLoader send_message_to_session_loader;

    OnSessionCoreExit on_session_core_exit;

    ConnectToHost connect_to_host;

    RegisterToHost register_to_host;

    OnShellSessionExit on_shell_process_exit;

    GetCurrentState get_current_state;
};

#endif