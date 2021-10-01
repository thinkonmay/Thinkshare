/// <summary>
/// @file agent-file-transfer-service.h
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-30
/// 
/// @copyright Copyright (c) 2021
/// 
#ifndef __AGENT_FILE_TRANSFER_SERVICE_H__
#define __AGENT_FILE_TRANSFER_SERVICE_H__


#include <agent-type.h>
#include <agent-object.h>


/// <summary>
/// 
/// </summary>
/// <param name="SessionSlaveID"></param>
void            on_file_compress_completed                      (gint SessionSlaveID);


/// <summary>
/// 
/// </summary>
/// <param name="SessionSlaveID"></param>
void            on_file_transceive_completed                    (gint SessionSlaveID);

/// <summary>
/// 
/// </summary>
/// <param name="server_command"></param>
/// <returns></returns>
FileTransferSession*    setup_file_transfer_session             (gchar* server_command);


/// <summary>
/// 
/// </summary>
/// <param name="agent"></param>
void             initialize_file_transfer_service               (AgentObject* agent);


/// <summary>
/// 
/// </summary>
/// <param name="session"></param>
void             start_file_transfer                            (FileTransferSession* session);


/// <summary>
/// 
/// </summary>
/// <param name="session"></param>
/// <returns></returns>
gchar*           file_transfer_session_get_session_id           (FileTransferSession* session);

/// <summary>
/// 
/// </summary>
/// <param name="session"></param>
/// <returns></returns>
gchar*           file_transfer_session_get_signalling_url       (FileTransferSession* session);

/// <summary>
/// 
/// </summary>
/// <param name="session"></param>
/// <returns></returns>
gchar*           file_transfer_session_get_input_file           (FileTransferSession* session);


/// <summary>
/// 
/// </summary>
/// <param name="session"></param>
/// <returns></returns>
gchar*           file_transfer_session_get_turnconnection       (FileTransferSession* session);

/// <summary>
/// 
/// </summary>
/// <param name="session"></param>
/// <param name="path"></param>
/// <returns></returns>
void            file_transfer_session_set_zip_file             (FileTransferSession* session, 
                                                                 gchar* path);

/// <summary>
/// 
/// </summary>
/// <param name="session"></param>
/// <returns></returns>
gchar*          file_transfer_session_get_zip_file              (FileTransferSession* session);

#endif