#ifndef __ERROR_CODE_H__
#define __ERROR_CODE_H__

#include <glib-2.0/glib.h>

#define         UNKNOWN_ERROR                   "Unknown error"
#define         DATA_CHANNEL_ERROR              "Datachannel error"
#define         UNKNOWN_MESSAGE                 "Unknown message"
#define         SIGNALLING_ERROR                "Signalling error"


typedef         gchar*                          ErrorCode;

#define         UNDEFINED_ERROR                 "UndefinedError"
#define         AGENT_STATE_CONFLICT            "Agent state conflict"
#define         CURRUPTED_CONFIG_FILE           "Corrupted file"
#define         ERROR_FILE_OPERATION            "Error file operation"
#define         ERROR_PROCESS_OPERATION         "Error process operation"


#endif