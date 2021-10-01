/// <summary>
/// @file agent-file-compressor.h
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-10-01
/// 
/// @copyright Copyright (c) 2021
/// 
#ifndef __AGENT_FILE_COMPRESSOR_H__
#define __AGENT_FILE_COMPRESSOR_H__

#include <glib-2.0/glib.h>
#include <agent-type.h>


void            init_file_compressor_pool           ();

/// <summary>
/// 
/// </summary>
/// <param name="compressor"></param>
void            agent_compress_file                 (FileCompressor* compressor);

/// <summary>
/// </summary>
/// 
/// <param name="session"></param>
/// <returns></returns>
FileCompressor* init_file_compressor                (FileTransferSession* session);

/// <summary>
/// </summary>
/// 
/// <param name="compressor"></param>
/// <param name="agent"></param>
void            start_compressor                    (FileCompressor* compressor, 
                                                     AgentObject* agent);

#endif