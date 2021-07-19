#include <agent-state.h>
#include <agent-type.h>
#include <glib.h>

static void
default_session_initialize  ( AgentObject* agent)
{
    g_printerr("receive unknown session initialize signal");
}

static void
default_session_terminate   (AgentObject* agent)
{
    g_printerr("receive unknown session termination signal");
}                             

static void
default_remote_control_disconnect( AgentObject* agent)
{
    g_printerr("receive unknown remote control disconnect");
}

static void
default_remote_control_reconnect(AgentObject* agent)
{
    g_printerr("receive unknown remote control reconnect");
}


static void
default_send_message_to_host(AgentObject* agent,
                             Message* message)
{
    g_printerr("receive unknown send message to host request");
}

static void
default_send_message_to_local_module(AgentObject* agent,
                                     Message* message)
{
    g_printerr("receive unknown send message to local module request");
}

static void
default_connect_to_host(AgentObject* agent)
{
    g_printerr("receive unknown connect to host request");
}

static void
default_register_to_host(AgentObject* agent)
{
    g_printerr("receive unknown connect to host request");
}

void
default_method(AgentState* state)
{
    state->session_terminate =          default_session_terminate;
    state->remote_control_disconnect =  default_remote_control_disconnect;
    state->session_initialize =         default_session_initialize;
    state->remote_control_reconnect =   default_remote_control_reconnect;
    state->send_message_to_host =       default_send_message_to_host;
    state->send_message_to_session_core = default_send_message_to_local_module;
    state->send_message_to_session_loader = default_send_message_to_local_module;
    state->connect_to_host =            default_connect_to_host;
    state->register_to_host =           default_register_to_host;
}