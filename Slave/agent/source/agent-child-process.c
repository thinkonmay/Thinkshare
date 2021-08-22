#include <agent-child-process.h>
#include <agent-type.h>
#include <agent-object.h>
#include <agent-state-on-session-off-remote.h>


#include <exit-code.h>
#include <child-process-constant.h>
#include <state-indicator.h>
#include <logging.h>
#include <general-constant.h>
#include <child-process-constant.h>
#include <error-code.h>

#include <glib.h>
#include <Windows.h>

#include <windows.h> 
#include <tchar.h>
#include <stdio.h> 
#include <strsafe.h>

#include <error-code.h>
#include <child-process-constant.h>

#define BUFSIZE 100000

/// <summary>
/// information about local storage to add to the session
/// local storage will be discovered by slave device before the session
/// </summary>
struct _ChildPipe
{
    
#ifdef G_OS_WIN32
    HANDLE standard_in;
    HANDLE standard_out;
#endif

};

struct _ChildProcess
{
    AgentObject* agent;
    gint process_id;

#ifdef G_OS_WIN32
    HANDLE process;
    HANDLE standard_in;
    HANDLE standard_out;
#endif

    ChildStdHandle func;
    ChildStateHandle handler;
    GThread* handle_thread;
    gboolean exit;
};





void
initialize_child_process_system(AgentObject* agent)
{
    static ChildProcess  process_array[LAST_CHILD_PROCESS];
    ZeroMemory(&process_array,sizeof(process_array));

    for(gint i = 0; i < LAST_CHILD_PROCESS;i++)
    {
        agent_set_child_process(agent,i,&(process_array[i]));
    }
}




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
        proc->handler(proc,ret,proc->agent);
        if(proc->exit)
        {
            g_thread_exit(NULL);
        }
    }
}



static ChildPipe*
initialize_process_handle(ChildProcess* self,
                          AgentObject* agent)
{
    static ChildPipe hdl;

    SECURITY_ATTRIBUTES attr;
    attr.nLength = sizeof(SECURITY_ATTRIBUTES);
    attr.bInheritHandle = TRUE;
    attr.lpSecurityDescriptor = NULL;

    if (!CreatePipe(&self->standard_out, &hdl.standard_out, &attr, 0))
    {
        agent_report_error(agent,"cannot create session core pipe");
        return NULL;
    }
    if (!CreatePipe(&hdl.standard_in, &self->standard_in, &attr, 0))
    {
        agent_report_error(agent,"cannot create session core pipe");
        return NULL;
    }

    SetHandleInformation(self->standard_in, HANDLE_FLAG_INHERIT, 0);
    SetHandleInformation(self->standard_out, HANDLE_FLAG_INHERIT, 0);
    return &hdl;
}



gboolean
send_message_to_child_process(ChildProcess* self,
    gchar* buffer,
    gint size)
{
    DWORD written;
    gboolean success = FALSE;

    success = WriteFile(self->standard_in,
        buffer, size, &written, NULL);
}


void
close_child_process(ChildProcess* proc)
{
    proc->exit = TRUE;

    write_to_log_file(AGENT_GENERAL_LOG,"Child process closed");
    TerminateProcess(proc->process, FORCE_EXIT);
    
    CloseHandle(proc->standard_out);
    CloseHandle(proc->standard_in);
    ZeroMemory(proc,sizeof(ChildProcess));
}

ChildProcess*
create_new_child_process(gchar* process_name,
                        gint process_id,
                        gchar* parsed_command,
                        ChildStdHandle func,
                        ChildStateHandle handler,
                        AgentObject* agent)
{
    ChildProcess* child_process = agent_get_child_process(agent,process_id);

    child_process->agent = agent;
    child_process->process_id = process_id;
    child_process->func = func;
    child_process->handler = handler;
    child_process->exit = FALSE;


    ChildPipe* hdl = initialize_process_handle(child_process,agent);
    if(hdl == NULL)
    {
        agent_report_error(agent,"Fail to create child process handle");
        write_to_log_file(AGENT_GENERAL_LOG,"Fail co create child process");
        return NULL;
    }

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

    strcat(process_name, parsed_command);
    
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

    if(!output)
    {
        GetLastError();
        agent_report_error(agent,ERROR_PROCESS_OPERATION);
        write_to_log_file(AGENT_GENERAL_LOG,ERROR_PROCESS_OPERATION);
        return NULL;        
    }

    memcpy(&child_process->process, &pi.hProcess, sizeof(HANDLE));

    CloseHandle(pi.hThread);
    CloseHandle(hdl->standard_out);
    CloseHandle(hdl->standard_in);


    child_process->handle_thread =  g_thread_new("child handle thread", 
        handle_child_process_thread, child_process);

    return child_process;
}



