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
	HANDLE* loader_process;

    HANDLE* core_in;
    HANDLE* core_out;
    HANDLE* loader_in;
    HANDLE* loader_out;

	CoreState state;

    GThread* ipc_thread;
};

/// <summary>
/// stdio pipe used by child process, agent should not used this object
/// </summary>
typedef struct
{
    HANDLE* core_in;
    HANDLE* core_out;
    HANDLE* loader_in;
    HANDLE* loader_out;
}ChildPipe;















/// <summary>
/// (OPTIONAL function)
/// used to print out error  related to winapi
/// </summary>
/// <param name="dwErr"></param>
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
                success = ReadFile(*(ipc->core_in), buffer, BUFFER_SIZE, &dwread, NULL);
                if (!success || dwread == 0) goto send;

                success = ReadFile(*(ipc->loader_in), buffer, BUFFER_SIZE, &dwread, NULL);
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

    CloseHandle(ipc->core_in);
    CloseHandle(ipc->core_out);
    TerminateProcess(*(ipc->core_process), 0);
    CloseHandle(ipc->core_process);

    ipc->ipc_thread = NULL;

    return TRUE;
}

gboolean
session_initialize(AgentObject* object)
{
	IPC* ipc = agent_get_ipc(object);

	PROCESS_INFORMATION pi;
	ZeroMemory(&pi, sizeof(pi));

	const gchar* null = "\0";


	/*concantenate command line string by using memcpy*/
	gchar* cmd;
	cmd = (gchar*)malloc(sizeof(gchar) * (strlen(SESSION_CORE_NAME) + 1));

	memcpy(cmd, SESSION_CORE_NAME, strlen(SESSION_CORE_NAME));
	memcpy(cmd + strlen(SESSION_CORE_NAME), null, 1);


    ChildPipe* pipe = initialize_handle(object);
    if (pipe == NULL)
    {
        g_printerr("cannot create std pipe");
        return FALSE;
    }

    /*setup startup infor(included standard input and output)*/
    STARTUPINFO startup_infor;
    startup_infor.cb = sizeof(STARTUPINFO);
    startup_infor.dwFlags = STARTF_USESTDHANDLES;
    startup_infor.hStdInput = pipe->core_in;
    startup_infor.hStdOutput = pipe->core_out;


    ipc->ipc_thread =
        g_thread_new("ipc-thread", (GThreadFunc)handle_thread, object);

	if (CreateProcess(SESSION_CORE_NAME, cmd, NULL, NULL,
		TRUE, CREATE_NEW_CONSOLE,
		NULL, NULL, &startup_infor, &pi) == 0)
	{
		DWORD drError = GetLastError();
		g_print("fail to create process %s", cmd);
		return FALSE;
	}
	else
		return TRUE;
}

gboolean
send_message_to_core(AgentObject* self, gchar* buffer)
{
    IPC* ipc = agent_get_ipc(self);

    DWORD written;
    gboolean success = FALSE;
    while (TRUE)
    {
        success = WriteFile(ipc->core_in, buffer, sizeof(buffer),&written,NULL );
        if (success || written == sizeof(buffer))
            return TRUE;
    }
}


gboolean
send_message_to_loader(AgentObject* self, gchar* buffer)
{
    IPC* ipc = agent_get_ipc(self);

    DWORD written;
    gboolean success = FALSE;
    while (TRUE)
    {
        success = WriteFile(ipc->loader_in, buffer, sizeof(buffer), &written, NULL);
        if (success || written == sizeof(buffer))
            return TRUE;
    }
}

IPC*
initialize_ipc()
{
    IPC* ipc = malloc(sizeof(IPC));
    ipc->state = CORE_STATE_UNKNOWN;
}