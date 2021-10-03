#ifndef __CHILD_PROCESS_CONSTANT_H__
#define __CHILD_PROCESS_CONSTANT_H__



typedef enum _PowerShellProcessId
{
    POWERSHELL_1 = 1,
    POWERSHELL_2,
    POWERSHELL_3,
    POWERSHELL_4,
    POWERSHELL_5,
    POWERSHELL_6,
    POWERSHELL_7,
    POWERSHELL_8,
}PowerShellProcessId;


typedef enum _FileCompressorProcessId
{
    FILE_COMPRESSOR_SERVICE_1 = 1,
    FILE_COMPRESSOR_SERVICE_2,
    FILE_COMPRESSOR_SERVICE_3,
    FILE_COMPRESSOR_SERVICE_4,
    FILE_COMPRESSOR_SERVICE_5,
    FILE_COMPRESSOR_SERVICE_6,
    FILE_COMPRESSOR_SERVICE_7,
    FILE_COMPRESSOR_SERVICE_8,
}FileCompressorProcessId;

typedef enum _FileTransceiverProcessId
{
    FILE_TRANSFER_SERVICE_1 = 1,
    FILE_TRANSFER_SERVICE_2,
    FILE_TRANSFER_SERVICE_3,
    FILE_TRANSFER_SERVICE_4,
    FILE_TRANSFER_SERVICE_5,
    FILE_TRANSFER_SERVICE_6,
    FILE_TRANSFER_SERVICE_7,
    FILE_TRANSFER_SERVICE_8,
}FileTransceiverProcessId;


#define MAX_CHILD_PROCESS               9

#define MAX_FILE_TRANSFER_INSTANCE      8

#define MAX_POWERSHELL_INSTANCE         8


#endif