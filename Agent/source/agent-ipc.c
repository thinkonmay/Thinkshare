#include <agent-ipc.h>
#include <agent-socket.h>
#include <agent-type.h>
#include <agent-object.h>
#include <agent-message.h>
#include <agent-state.h>
#include <agent-state-open.h>


#include <gmodule.h>
#include <Windows.h>
#include <stdio.h>

#define SESSION_CORE_NAME  "SessionCore.exe"

#define BUFFER_SIZE 10000










/// <summary>
/// contain information about shared memory hub and connection
/// </summary>
struct _IPC
{
	HANDLE* core_process;

    HANDLE* core_in;
    HANDLE* core_out;

    GThread* ipc_thread;
};

/// <summary>
/// stdio pipe used by child process, agent should not used this object
/// </summary>
typedef struct
{
    HANDLE* core_in;
    HANDLE* core_out;
}ChildPipe;















/// <summary>
/// (OPTIONAL function)
/// used to print out error  related to winapi
/// </summary>
/// <param name="dwErr"></param>
/*
void
print_window_error(DWORD dwErr)
{

    WCHAR   wszMsgBuff[512];  // Buffer for text.

    DWORD   dwChars;  // Number of chars returned.

    // Try to get the message from the system errors.
    dwChars = FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM |
        FORMAT_MESSAGE_IGNORE_INSERTS,
        NULL,
        dwErr,
        0,
        wszMsgBuff,
        512,
        NULL);

    if (0 == dwChars)
    {
        // The error code did not exist in the system errors.
        // Try Ntdsbmsg.dll for the error code.

        HINSTANCE hInst;

        // Load the library.
        hInst = LoadLibrary(L"Ntdsbmsg.dll");
        if (NULL == hInst)
        {
            g_printerr("cannot load Ntdsbmsg.dll\n");
            exit(1);  // Could 'return' instead of 'exit'.
        }

        // Try getting message text from ntdsbmsg.
        dwChars = FormatMessage(FORMAT_MESSAGE_FROM_HMODULE |
            FORMAT_MESSAGE_IGNORE_INSERTS,
            hInst,
            dwErr,
            0,
            wszMsgBuff,
            512,
            NULL);

        // Free the library.
        FreeLibrary(hInst);

    }

    // Display the error message, or generic text if not found.
    g_printerr("Error value: %d Message: %ws\n",
        dwErr,
        dwChars ? wszMsgBuff : L"Error message not found.");
}
*/


/// <summary>
/// initialize anonymous pipe to assign to 
/// session core and session loader child process
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
ChildPipe*
initialize_handle(AgentObject* self)
{
    IPC* ipc = agent_get_ipc(self);
    ChildPipe* pipe = malloc(sizeof(ChildPipe));

    SECURITY_ATTRIBUTES attr;
    attr.nLength = sizeof(SECURITY_ATTRIBUTES);
    attr.bInheritHandle = TRUE;
    attr.lpSecurityDescriptor = NULL;

    if (!CreatePipe(ipc->core_in, pipe->core_out , &attr, 0)||
         CreatePipe(ipc->core_out, pipe->core_in , &attr, 0))
    {
        g_print("cannot create session core pipe");
        return NULL;
    }

    return pipe;
}


/// <summary>
/// handle_thread responsible for handle data from 
/// session core and session loader standard input output
/// </summary>
/// <param name="data"></param>
/// <returns></returns>
gpointer
handle_thread(gpointer data)
{
    AgentObject* object = (AgentObject*)data;
    IPC* ipc = agent_get_ipc(object);

    while (TRUE)
    {
        if (!agent_get_state(object) == ON_SESSION)
        {
            return NULL;
        }
        else
        {
            DWORD dwread, dwrite;
            gchar buffer[BUFFER_SIZE];
            gboolean success = FALSE;
            while (TRUE)
            {
                success = ReadFile(*(ipc->core_in), 
                    buffer, BUFFER_SIZE, &dwread, NULL);
                if (!success || dwread == 0) goto send;
            }

        send:
            on_agent_message(object, buffer);
        }
    }
}


gboolean
session_terminate(AgentObject* agent)
{
	IPC* ipc = agent_get_ipc(agent);

    TerminateProcess(*(ipc->core_process), 0);
    CloseHandle(ipc->core_in);
    CloseHandle(ipc->core_out);
    CloseHandle(ipc->core_process);

    ipc->core_in = NULL;
    ipc->core_out = NULL;
    ipc->core_process = NULL;

    ipc->ipc_thread = NULL;

    return TRUE;
}

gboolean
session_initialize(AgentObject* object)
{
    ChildProcess* session_core = create_new_child_process("C:\\Windows\\System32\\SessionCore.exe", 0,NULL,
        (ChildHandleFunc)handle_session_core_function, object);
    ChildProcess* child_process_o = agent_get_child_process(object, 0);
    child_process_o = session_core;
}

gboolean
send_message_to_core(AgentObject* self, gchar* buffer)
{
    send_message_to_child_process(agent_get_child_process(self,0),buffer,strlen(buffer)*sizeof(gchar));
}