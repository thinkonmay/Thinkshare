
#ifndef __AGENT_MESSAGE_H__
#define __AGENT_MESSAGE_H__

#include <agent-type.h>



/// <summary>
/// initialize message (json_object) 
/// with given from, destination, opcode and data (Message datatype)
/// </summary>
/// <param name="from"></param>
/// <param name="to"></param>
/// <param name="opcode"></param>
/// <param name="data"></param>
/// <param name="data_size"></param>
/// <returns></returns>
Message*            message_init                (Module from,
                                                Module to,
                                                Opcode opcode,
                                                Message* data);

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