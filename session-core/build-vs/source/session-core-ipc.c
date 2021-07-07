#include <session-core-ipc.h>
#include <session-core.h>
#include <session-core-message.h>

#include <Windows.h>
#define BUFFER_SIZE  10000


struct _IPC
{
	HANDLE* input_pipe;
	HANDLE* output_pipe;

	GThread* input_thread;
};




gpointer
handle_thread(gpointer data)
{
    SessionCore* core = (SessionCore*)data;
    IPC* ipc = session_core_get_ipc(core);

    while (TRUE)
    {
         DWORD dwread, dwrite;
         gchar buffer[BUFFER_SIZE];
         gboolean success = FALSE;
         while (TRUE)
         {
             HANDLE input = *(ipc->input_pipe);
             success = ReadFile(input, buffer, BUFFER_SIZE, &dwread, NULL);
             if (!success || dwread == 0) goto send;
         }

    send:
         session_core_on_message(core, buffer);        
    }
}


void
handle_thread_start(SessionCore* core)
{
    IPC* ipc = session_core_get_ipc(core);

    ipc->input_thread =
        g_thread_new("input-thread", (GThreadFunc)handle_thread, ipc);
}


IPC* 
ipc_initialize(SessionCore* core)
{
	IPC* ipc = malloc(sizeof(IPC));

    ipc->input_pipe = GetStdHandle(STD_INPUT_HANDLE);
    ipc->output_pipe = GetStdHandle(STD_OUTPUT_HANDLE);

    ipc->input_thread = NULL;

    return ipc;
}

void
send_message_to_agent(SessionCore* core,
                        gchar* buffer)
{
    IPC* ipc = session_core_get_ipc(core);

    DWORD written;
    gboolean success = FALSE;
    while (TRUE)
    {
        success = WriteFile(ipc->output_pipe, buffer, sizeof(buffer), &written, NULL);
        if (success || written == sizeof(buffer))
            return TRUE;
    }
}