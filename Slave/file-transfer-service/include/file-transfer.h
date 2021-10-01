/// <summary>
/// @file file-transfer.h
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-05
/// 
/// @copyright Copyright (c) 2021
#include "file-transfer-type.h"
#include <error-code.h>
#include <message-form.h>




/// <summary>
/// setup pipeline, 
/// this function should be called after registered session with signalling server
/// </summary> 
/// <param name="self">session core object</param>
void						file_transfer_setup_pipeline				(FileTransferService* self);

/// <summary>
/// session core setup signalling server, 
/// in this step, signalling url will be assigned
/// </summary> 
/// <param name="self">session core object</param>
void	  					file_transfer_setup_webrtc_signalling	(FileTransferService* self);

/// <summary>
/// connect to signalling server,
/// this function should be called after all initilize step is done
/// </summary> 
/// <param name="self"></param>
void						file_transfer_connect_signalling_server	(FileTransferService* self);
	
/// <summary>
/// session core send message to other module
/// </summary> 
/// <param name="self"></param>
/// <param name="message"></param>
void						file_transfer_send_message				(FileTransferService* self,
														 Message* message);

/// <summary>
/// initialize session core, 
/// this function should be called in main function
/// </summary> 
/// <returns>session core object</returns>
FileTransferService*			file_transfer_initialize					(gchar* signalling, 
																		 gint session_id,
																		 gchar* file,
																		 gchar* turn);

/// <summary>
/// finalize session core object, 
/// this function should be called when an error is occour
/// </summary> 
/// <param name="self"></param>
/// <param name="exit_code">reason of exit</param>
/// <param name="error">error emmited (if available)</param>
void						file_transfer_finalize					(FileTransferService* self,
					  												 ExitCode code,
																	 GError* error);

/// <summary>
/// get pipeline from session core object
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
WebRTCbin*					file_transfer_get_webrtcbin				(FileTransferService* self);

/// <summary>
/// get FileTransferHub from session core object, 
/// webrtc hub will be use for data transfer between session core and client 
/// through webrtc datachannel 
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
FileTransferHub*		file_transfer_get_transfer_hub				(FileTransferService* self);


/// <summary>
/// get signalling hub from session core object,
/// signalling hub will be use to communicate with signalling server
/// for register remote session and exchange sdp and ice 
/// </summary>
/// <param name="core"></param>
/// <returns></returns>
FileTransferSignalling*	file_transfer_get_signalling_hub			(FileTransferService* core);

/// <summary>
/// report session core error with server
/// </summary> 
/// <param name="self"></param>
/// <param name="code"></param>
void			report_file_transfer_error				(FileTransferService* self,
														 ErrorCode code);

/// <summary>
/// get main context from session core
/// </summary>
/// <param name="core"></param>
/// <returns></returns>
GMainContext*	file_transfer_get_main_context			(FileTransferService* core);
