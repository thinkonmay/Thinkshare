#ifndef __GENERAL_CONSTANT_H__
#define __GENERAL_CONSTANT_H__

#define RECONNECT_INTERVAL            10000
#define CURRENT_DIR                   "C:\\ThinkMay"
/////////////////////////////////////////////////////////////////////////////////////////////////
#define SESSION_CORE_BINARY           CURRENT_DIR"\\bin\\session-core.exe "
#define AGENT_BINARY                  CURRENT_DIR"\\bin\\agent.exe "
#define FILE_TRANSCEIVER_BINARY       CURRENT_DIR"\\bin\\file-transfer.exe "
#define POWERSHELL_BINARY             "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe "
//////////////////////////////////////////////////////////////////////////////////////////////////


/////////////////////////////////////////////////////////////////////////////////////////////////
#define SESSION_CORE_GENERAL_LOG      CURRENT_DIR"\\session_core_log\\session_core_general_log.txt"
#define SESSION_CORE_MESSAGE_LOG      CURRENT_DIR"\\session_core_log\\session_core_message_log.txt"
#define SESSION_CORE_NETWORK_LOG      CURRENT_DIR"\\session_core_log\\session_core_network_log.txt"

#define AGENT_NETWORK_LOG             CURRENT_DIR"\\agent_log\\agent_network_log.txt"
#define AGENT_GENERAL_LOG             CURRENT_DIR"\\agent_log\\agent_general_log.txt"
#define AGENT_MESSAGE_LOG             CURRENT_DIR"\\agent_log\\agent_message_log.txt"
/////////////////////////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////
#define SHELL_OUTPUT                  CURRENT_DIR"\\Shell\\Output"
#define SHELL_SCRIPT                  CURRENT_DIR"\\Shell\\Script"

#define SHELL_SCRIPT_BUFFER_1         SHELL_SCRIPT"_1.ps1"   
#define SHELL_SCRIPT_BUFFER_2         SHELL_SCRIPT"_2.ps1"   
#define SHELL_SCRIPT_BUFFER_3         SHELL_SCRIPT"_3.ps1"   
#define SHELL_SCRIPT_BUFFER_4         SHELL_SCRIPT"_4.ps1"   
#define SHELL_SCRIPT_BUFFER_5         SHELL_SCRIPT"_5.ps1"   
#define SHELL_SCRIPT_BUFFER_6         SHELL_SCRIPT"_6.ps1"   
#define SHELL_SCRIPT_BUFFER_7         SHELL_SCRIPT"_7.ps1"   
#define SHELL_SCRIPT_BUFFER_8         SHELL_SCRIPT"_8.ps1"    

#define SHELL_OUTPUT_BUFFER_1         SHELL_OUTPUT"_1.json"   
#define SHELL_OUTPUT_BUFFER_2         SHELL_OUTPUT"_2.json"   
#define SHELL_OUTPUT_BUFFER_3         SHELL_OUTPUT"_3.json"   
#define SHELL_OUTPUT_BUFFER_4         SHELL_OUTPUT"_4.json"   
#define SHELL_OUTPUT_BUFFER_5         SHELL_OUTPUT"_5.json"   
#define SHELL_OUTPUT_BUFFER_6         SHELL_OUTPUT"_6.json"   
#define SHELL_OUTPUT_BUFFER_7         SHELL_OUTPUT"_7.json"  
#define SHELL_OUTPUT_BUFFER_8         SHELL_OUTPUT"_8.json" 
////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////
#define FILE_COMPRESSOR_OUTPUT        CURRENT_DIR"\\Temp\\compressed"

#define FILE_COMPRESSOR_OUTPUT_1      FILE_COMPRESSOR_OUTPUT"_1.zip"  
#define FILE_COMPRESSOR_OUTPUT_2      FILE_COMPRESSOR_OUTPUT"_2.zip" 
#define FILE_COMPRESSOR_OUTPUT_3      FILE_COMPRESSOR_OUTPUT"_3.zip"  
#define FILE_COMPRESSOR_OUTPUT_4      FILE_COMPRESSOR_OUTPUT"_4.zip" 
#define FILE_COMPRESSOR_OUTPUT_5      FILE_COMPRESSOR_OUTPUT"_5.zip"  
#define FILE_COMPRESSOR_OUTPUT_6      FILE_COMPRESSOR_OUTPUT"_6.zip" 
#define FILE_COMPRESSOR_OUTPUT_7      FILE_COMPRESSOR_OUTPUT"_7.zip"  
#define FILE_COMPRESSOR_OUTPUT_8      FILE_COMPRESSOR_OUTPUT"_8.zip" 
////////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////////
#define HOST_CONFIG_FILE              CURRENT_DIR"\\config\\host_configuration.json"

#define SESSION_SLAVE_FILE            CURRENT_DIR"\\config\\session_slave.json" 
////////////////////////////////////////////////////////////////////////




#define DEVICE_ID                     "SlaveID"
#define HOST_URL                      "HostUrl"
#define DISABLE_SSL                   "DisableSSL"

#endif