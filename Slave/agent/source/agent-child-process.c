#include <agent-child-process.h>
#include <agent-type.h>
#include <agent-object.h>


#include <exit-code.h>
#include <child-process-constant.h>
#include <state-indicator.h>
#include <logging.h>
#include <general-constant.h>
#include <child-process-constant.h>

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


        /*
        *if child process terminated is session core
        *let agent handle that
        */
        if (ret != STILL_ACTIVE)
        {
            if(proc->process_id  == SESSION_CORE_PROCESS_ID)
            {
                agent_on_session_core_exit(proc->agent);
            }
            return;
        }

        /*
        *if child process is session core, check for current state of agent,
        *Terminate process if agent is not in session,
        */
        if(proc->process_id == SESSION_CORE_PROCESS_ID)
        {
            if(!g_strcmp0(agent_get_current_state_string(proc->agent) , AGENT_ON_SESSION))
            {
                TerminateProcess(proc->process,FORCE_EXIT);
                return;
            }
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
    DWORD exit_code = STILL_ACTIVE;
    GetExitCodeProcess(proc->process, exit_code);

    if (exit_code == STILL_ACTIVE)
    {
        write_to_log_file(AGENT_GENERAL_LOG,"Child process closed");
        TerminateProcess(proc->process, FORCE_EXIT);
    }
    CloseHandle(proc->standard_out);
    CloseHandle(proc->standard_in);
    ZeroMemory(proc,sizeof(ChildProcess));
}

ChildProcess*
create_new_child_process(gchar* process_name,
    gint process_id,
    gchar* parsed_command,
    ChildHandleFunc func,
    AgentObject* agent)
{
    static ChildProcess child_process[8];
    child_process[process_id].agent = agent;
    child_process[process_id].process_id = process_id;
    child_process[process_id].func = func;


    ChildPipe* hdl = initialize_process_handle(&child_process[process_id],agent);
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
        agent_report_error(agent,"Fail to create child process ");
        write_to_log_file(AGENT_GENERAL_LOG,"Fail co create child");
        return NULL;        
    }

    memcpy(&child_process[process_id].process, &pi.hProcess, sizeof(HANDLE));

    CloseHandle(pi.hThread);
    CloseHandle(hdl->standard_out);
    CloseHandle(hdl->standard_in);


    g_thread_new("child handle thread", 
        handle_child_process_thread, &(child_process[process_id]));


    return &child_process;
}



