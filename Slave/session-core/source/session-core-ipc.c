#include <session-core-ipc.h>
#include <session-core.h>
#include <session-core-message.h>

#include <Windows.h>
#define BUFFER_SIZE  10000


struct _IPC
{
	HANDLE input_pipe;
	HANDLE output_pipe;

	GThread* input_thread;
};



/// <summary>
/// handle thread will be run until session core process is terminated
/// </summary>
/// <param name="data"></param>
/// <returns></returns>
static gpointer
handle_thread(gpointer data)
{
    SessionCore* core = (SessionCore*)data;
    IPC* ipc = session_core_get_ipc(core);

    // intialize and allocate memory buffer 
    gchar buffer[BUFFER_SIZE];
    ZeroMemory(&buffer, BUFFER_SIZE);

    // handle in infinite loop
    while (TRUE)
    {
        DWORD dwread = 0;
        gboolean success = FALSE;
        while (TRUE)
        {
            HANDLE input = ipc->input_pipe;

            // read from stdout of child process
            success = ReadFile(input, buffer, BUFFER_SIZE, &dwread, NULL);
            if (success && dwread > 0) {goto send;}
        }
    send:
        // if stdin pipe from session core receive message, handle it
        session_core_on_message(core, buffer);        
    }
}



IPC* 
ipc_initialize(SessionCore* core)
{
	static IPC ipc;

    ipc.input_pipe = GetStdHandle(STD_INPUT_HANDLE);
    ipc.output_pipe = GetStdHandle(STD_OUTPUT_HANDLE);

    // run handle thread to check for message from agent
    ipc.input_thread = g_thread_new("input-thread", (GThreadFunc)handle_thread, core);
    return &ipc;
}

void
send_message_to_agent(SessionCore* core,
                        gchar* buffer,
                        gint size)
{
    IPC* ipc = session_core_get_ipc(core);
    DWORD written;
     

    WriteFile(ipc->output_pipe, buffer, size, NULL, NULL);
}