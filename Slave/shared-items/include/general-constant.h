#ifndef __GENERAL_CONSTANT_H__
#define __GENERAL_CONSTANT_H__

#include "child-process-resources-assign.h"
#define RECONNECT_INTERVAL            10000




void    thinkmay_init                 (gchar* binary_path,
                                       gint terminate_path_size);


gchar*  get_absolute_path             (const char* relative_path);


#define GET_ABSOLUTE_PATH()           get_absolute_path();
/////////////////////////////////////////////////////////////////////////////////////////////////
#define SESSION_CORE_BINARY           get_absolute_path("\\bin\\session-core.exe ")
#define AGENT_BINARY                  get_absolute_path("\\bin\\agent.exe ")
#define FILE_TRANSCEIVER_BINARY       get_absolute_path("\\bin\\file-transfer.exe ")
#define REMOTE_APP_BINARY             get_absolute_path("\\bin\\remote-app.exe ")
#define POWERSHELL_BINARY             "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe "
//////////////////////////////////////////////////////////////////////////////////////////////////


/////////////////////////////////////////////////////////////////////////////////////////////////
#define SESSION_CORE_GENERAL_LOG      get_absolute_path("\\session_core_log\\session_core_general_log.txt")
#define SESSION_CORE_MESSAGE_LOG      get_absolute_path("\\session_core_log\\session_core_message_log.txt")
#define SESSION_CORE_NETWORK_LOG      get_absolute_path("\\session_core_log\\session_core_network_log.txt")

#define AGENT_NETWORK_LOG             get_absolute_path("\\agent_log\\agent_network_log.txt")
#define AGENT_GENERAL_LOG             get_absolute_path("\\agent_log\\agent_general_log.txt")
#define AGENT_MESSAGE_LOG             get_absolute_path("\\agent_log\\agent_message_log.txt")
/////////////////////////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////
#define SHELL_OUTPUT                  "\\Shell\\Output"
#define SHELL_SCRIPT                  "\\Shell\\Script"


#define FILE_COMPRESSOR_OUTPUT        "\\Temp\\compressed"


gchar* SHELL_SCRIPT_BUFFER_1;
gchar* SHELL_SCRIPT_BUFFER_2;
gchar* SHELL_SCRIPT_BUFFER_3;
gchar* SHELL_SCRIPT_BUFFER_4;
gchar* SHELL_SCRIPT_BUFFER_5;
gchar* SHELL_SCRIPT_BUFFER_6;
gchar* SHELL_SCRIPT_BUFFER_7;
gchar* SHELL_SCRIPT_BUFFER_8;

gchar* SHELL_OUTPUT_BUFFER_1;
gchar* SHELL_OUTPUT_BUFFER_2;
gchar* SHELL_OUTPUT_BUFFER_3;
gchar* SHELL_OUTPUT_BUFFER_4;
gchar* SHELL_OUTPUT_BUFFER_5;
gchar* SHELL_OUTPUT_BUFFER_6;
gchar* SHELL_OUTPUT_BUFFER_7;
gchar* SHELL_OUTPUT_BUFFER_8;

gchar* FILE_COMPRESSOR_OUTPUT_1;
gchar* FILE_COMPRESSOR_OUTPUT_2;
gchar* FILE_COMPRESSOR_OUTPUT_3;
gchar* FILE_COMPRESSOR_OUTPUT_4;
gchar* FILE_COMPRESSOR_OUTPUT_5;
gchar* FILE_COMPRESSOR_OUTPUT_6;
gchar* FILE_COMPRESSOR_OUTPUT_7;
gchar* FILE_COMPRESSOR_OUTPUT_8;



////////////////////////////////////////////////////////////////////////
#define HOST_CONFIG_FILE              get_absolute_path("\\config\\host_configuration.json")

#define SESSION_SLAVE_FILE            get_absolute_path("\\config\\session_slave.json" )

#define SESSION_CLIENT_FILE           get_absolute_path("\\config\\session_client.json")
////////////////////////////////////////////////////////////////////////




#define DEVICE_ID                     "SlaveID"
#define HOST_URL                      "HostUrl"
#define DISABLE_SSL                   "DisableSSL"

#endif