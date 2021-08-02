#include <agent-child-process.h>
#include <agent-type.h>
#include <glib.h>
#include <Windows.h>

#include <windows.h> 
#include <tchar.h>
#include <stdio.h> 
#include <strsafe.h>

#define BUFSIZE 100000

/// <summary>
/// information about local storage to add to the session
/// local storage will be discovered by slave device before the session
/// </summary>
struct _ChildPipe
{
    HANDLE standard_in;
    HANDLE standard_out;
};

struct _ChildProcess
{
    AgentObject* agent;
    HANDLE process;
    gint process_id;

    HANDLE standard_in;
    HANDLE standard_out;
    ChildHandleFunc func;
};










static gpointer
handle_child_process_thread(ChildProcess* proc)
{

    while (TRUE)
    {
        DWORD dwRead, dwWritten;
        CHAR chBuf[BUFSIZE];
        ZeroMemory(chBuf, BUFSIZE);
        BOOL bSuccess = FALSE;

        for (;;)
        {
            bSuccess = ReadFile(proc->standard_out, chBuf, BUFSIZE, &dwRead, NULL);
            if (!bSuccess || dwRead == 0) break;


            GBytes* data = g_bytes_new(chBuf, strlen(chBuf));
            proc->func(data, proc->process_id, proc->agent);
            ZeroMemory(chBuf, BUFSIZE);
            break;
        }

        DWORD ret;
        GetExitCodeProcess(proc->process, &ret);
        if (ret != STILL_ACTIVE)
        {
            return;
        }
    }

}



ChildPipe*
initialize_process_handle(ChildProcess* self);


gboolean
send_message_to_child_process(ChildProcess* self,
    gchar* buffer,
    gint size)
{
    DWORD written;
    gboolean success = FALSE;
    for (;;)
    {
        success = WriteFile(self->standard_in,
            buffer, size, &written, NULL);

        if (success)
        {
            return TRUE;
        }
    }
}


void
close_child_process(ChildProcess* proc)
{
    DWORD exit_code = STILL_ACTIVE;
    GetExitCodeProcess(proc->process, exit_code);
    if (exit_code == STILL_ACTIVE)
    {
        TerminateProcess(proc->process, 0);
    }
    CloseHandle(proc->standard_out);
    CloseHandle(proc->standard_in);
}

ChildProcess*
create_new_child_process(gchar* process_name,
    gint process_id,
    gchar** command_array,
    ChildHandleFunc func,
    AgentObject* agent)
{
    static ChildProcess child_process[8];
    child_process[process_id].agent = agent;
    child_process[process_id].process_id = process_id;
    child_process[process_id].func = func;


    ChildPipe* hdl = initialize_process_handle(&child_process[process_id]);

    PROCESS_INFORMATION pi;
    ZeroMemory(&pi, sizeof(pi));

    /*setup startup infor(included standard input and output)*/
    STARTUPINFO startup_infor;
    ZeroMemory(&startup_infor, sizeof(startup_infor));
    startup_infor.cb = sizeof(STARTUPINFO);
    startup_infor.dwFlags |= STARTF_USESTDHANDLES;
    startup_infor.hStdInput = hdl->standard_in;
    startup_infor.hStdOutput = hdl->standard_out;
    startup_infor.hStdError = hdl->standard_out;



    if(command_array != NULL)
    {
        strcat(process_name, *command_array);
    }



    LPSTR path = g_win32_locale_filename_from_utf8(process_name);
    /*START process, all standard input and output are controlled by agent*/
    gboolean output = CreateProcess(NULL,
        path,
        NULL,
        NULL,
        TRUE,
        0,
        NULL,
        NULL,
        &startup_infor, &pi);

    memcpy(&child_process[process_id].process, &pi.hProcess, sizeof(HANDLE));
    CloseHandle(pi.hThread);

    CloseHandle(hdl->standard_out);
    CloseHandle(hdl->standard_in);


    g_thread_new("child handle thread", 
        handle_child_process_thread, &(child_process[process_id]));


    return &child_process;
}


static ChildPipe*
initialize_process_handle(ChildProcess* self)
{
    static ChildPipe hdl;

    SECURITY_ATTRIBUTES attr;
    attr.nLength = sizeof(SECURITY_ATTRIBUTES);
    attr.bInheritHandle = TRUE;
    attr.lpSecurityDescriptor = NULL;

    if (!CreatePipe(&self->standard_out, &hdl.standard_out, &attr, 0))
    {
        g_printerr("cannot create session core pipe");
    }
    if (!CreatePipe(&hdl.standard_in, &self->standard_in, &attr, 0))
    {
        g_printerr("cannot create session core pipe");
    }

    SetHandleInformation(self->standard_in, HANDLE_FLAG_INHERIT, 0);
    SetHandleInformation(self->standard_out, HANDLE_FLAG_INHERIT, 0);
    return &hdl;
}

