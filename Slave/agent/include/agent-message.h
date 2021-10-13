
#ifndef __AGENT_MESSAGE_H__
#define __AGENT_MESSAGE_H__

#include <agent-type.h>
#include <module-code.h>
#include <opcode.h>

#include <Windows.h>




/// <summary>
/// all message from agent-ipc and agent-socket are handled here,
/// if agent is destination, parsing the opcode and data,
/// if not, send it to the correct destination
/// </summary>
/// <param name="conn"></param>
/// <param name="type"></param>
/// <param name="message"></param>
/// <param name="self"></param>
void				on_agent_message            (AgentObject* self,
                                                gchar* data);

/// <summary>
/// 
/// </summary>
/// <param name="self"></param>
/// <param name="message"></param>
void				send_message                (AgentObject* self,
                                                 Message* message);
#endif