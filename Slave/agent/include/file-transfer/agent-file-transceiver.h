/// <summary>
/// @file agent-file-transceiver.h
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-10-01
/// 
/// @copyright Copyright (c) 2021
/// 
#ifndef __AGENT_FILE_TRANSCEIVER_H__
#define __AGENT_FILE_TRANSCEIVER_H__
#include <agent-type.h>
#include <agent-object.h>

/// <summary>
/// 
/// </summary>
/// <param name="session"></param>
/// <returns></returns>
FileTransceiver*        init_file_transceiver               (FileTransferSession* session);


/// <summary>
/// 
/// </summary>
/// <param name="agent"></param>
void                    init_file_transceiver_pool          ();
#endif


