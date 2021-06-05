#include "Framework.h"
#include "Variable.h"
#include "Handle data channel.h"
#include "Handle pipeline.h"
#include "RC config.h"
#include "CorePipeSink.h"
#include "Signalling handling.h"


void 
read_string(HANDLE fh ,gchar* output)
{
    ULONG read = 0;
    gint index = 0;
    do {
        if (!ReadFile(fh, output + index++, 1, &read, NULL))
        {
            g_printerr("cannot read message from pipe");
        }
    } while (read > 0 && *(output + index - 1) != 0);
}

void
send_byte(HANDLE fh, GBytes* input)
{
    if (!WriteFile(file_handle_byte, input, g_bytes_get_size(input), nullptr, NULL))
    {
        g_printerr("cannot process IPC");
    }
}

void
send_string(HANDLE fh, gchar* input)
{
    if (!WriteFile(file_handle_string, input, strlen(input), nullptr, NULL))
    {
        g_printerr("cannot process IPC");
    }
}

void
core_pipe_sink_handle()
{
    // create file
    file_handle_byte = CreateFileW(BYTE_PIPE_NAME,
        GENERIC_READ | GENERIC_WRITE,
        FILE_SHARE_WRITE,
        NULL,
        OPEN_EXISTING, 0, NULL);
    // create file
    file_handle_string = CreateFileW(STR_PIPE_NAME,
        GENERIC_READ | GENERIC_WRITE,
        FILE_SHARE_WRITE,
        NULL,
        OPEN_EXISTING, 0, NULL);

}