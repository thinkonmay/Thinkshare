                /// <summary>
/// @file file-transfer-signalling.h
/// @author {Do Huy Hoang} ({huyhoangdo0205@gmail.com})
/// </summary>
/// @version 1.0
/// @date 2021-09-05
/// 
/// @copyright Copyright (c) 2021
/// 
#ifndef __FILE_TRANSFER_SIGNALLING_H__
#define __FILE_TRANSFER_SIGNALLING_H__

#include "file-transfer-type.h"
#include "file-transfer.h"
#include "file-transfer-type.h"


#include <message-form.h>

#include <libsoup/soup.h>
#include <glib.h>
#include <gst/gst.h>

/*Used as webrtcbin callback function*/

/// <summary>
/// send ice candidate to peer through signalling server
/// typically emitted when pipeline is set to PLAYING 
/// </summary>
/// <param name="webrtc"></param>
/// <param name="mlineindex"></param>
/// <param name="candidate"></param>
/// <param name="core"></param>
void                            send_ice_candidate_message                              (GstElement* webrtc,
                                                                                        guint mlineindex,
                                                                                        gchar* candidate,
                                                                                        FileTransferSvc* core);

/// <summary>
/// handle on negotiation signal from webrtcbin element, 
/// typically emitted when pipeline is set to PLAYING 
/// <param name="element"></param>
/// <param name="core"></param>
void                            on_negotiation_needed                                   (GstElement* element,
                                                                                        FileTransferSvc* core);


/// <summary>
/// handle on ice gathering state from webrtcbin element,
/// typically emitted when pipeline is set to PLAYING 
/// </summary>
/// <param name="webrtcbin"></param>
/// <param name="pspec"></param>
/// <param name="user_data"></param>
void                            on_ice_gathering_state_notify                           (GstElement* webrtcbin,
                                                                                        GParamSpec* pspec,
                                                                                        gpointer user_data);

/// <summary>
/// initialize signalling hub 
/// </summary>
/// <param name="core"></param>
/// <returns></returns>
FileTransferSignalling*         signalling_hub_initialize                               (FileTransferSvc* core);


/// <summary>
/// connect to signalling server, 
/// this function should be triggered in initialize process
/// </summary> 
/// <param name="core">session core object</param>
void                            connect_to_websocket_signalling_server_async            (FileTransferSvc* core);

/// <summary>
/// reegister with server by sending session slave id
/// </summary> 
/// <param name="core"></param>
/// <returns></returns>
gboolean                        register_with_server                                    (FileTransferSvc* core);

void                            signalling_hub_setup                                    (FileTransferSignalling* hub,
                                                                                        gchar* url,
                                                                                        gint session_slave_id);

/// <summary>
/// close websocket connection with signalling server 
/// (it means session disconnnected event will be triggered)
/// </summary>
/// <param name="hub"></param>
/// <returns></returns>
gboolean                        signalling_close                                        (FileTransferSignalling* hub);

/// <summary>
/// get stun server from signallinghub, 
/// stun server will follow gstreamer format 
/// (Ex: stun://stun.l.google.com:3475)
/// </summary>
/// <param name="hub"></param>
/// <returns></returns>
gchar*                          signalling_hub_get_turn_server                          (FileTransferSignalling* hub);

#endif // !__SESSION_CORE_SIGNALLING_H