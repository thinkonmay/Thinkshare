#include "Framework.h"
#include "Session initialization.h"
#include "Session termination.h"
#include "Variable.h"
#include "Device Information.h"
#include "AgentPipeSrc.h"
#include "AgentSocket.h"

void
read_string_from_loader(gchar* output)
{
    ULONG read = 0;
    gint index = 0;
    do {
        if (!ReadFile(file_handle_string, output + index++, 1, &read, NULL))
        {
            g_printerr("cannot read message from pipe");
        }
    } while (read > 0 && *(output + index - 1) != 0);
}

/*send string to */
void
send_string_to_loader(gchar* input)
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
    file_handle_string = CreateFileW(AGENT_PIPE_NAME,
        GENERIC_READ | GENERIC_WRITE,
        FILE_SHARE_WRITE,
        NULL,
        OPEN_EXISTING, 0, NULL);
}