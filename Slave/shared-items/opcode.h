#ifndef __OPCODE_H__
#define __OPCODE_H__

typedef enum
{
    SESSION_INFORMATION	,

    REGISTER_SLAVE	,

    SLAVE_ACCEPTED	,
    DENY_SLAVE	,

    REJECT_SLAVE,

    SESSION_INITIALIZE,
    SESSION_TERMINATE,
    RECONNECT_REMOTE_CONTROL,
    DISCONNECT_REMOTE_CONTROL,

    QOE_REPORT,
    CHANGE_MEDIA_MODE,
    CHANGE_RESOLUTION,
    CHANGE_FRAMERATE,
    BITRATE_CALIBRATE,

    COMMAND_LINE_FORWARD,

    EXIT_CODE_REPORT,
    ERROR_REPORT,

    NEW_COMMAND_LINE_SESSION,
    END_COMMAND_LINE_SESSION,
}Opcode;






#endif