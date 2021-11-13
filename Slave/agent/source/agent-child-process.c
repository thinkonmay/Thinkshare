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

#include <tchar.h>
#include <stdio.h> 
#include <strsafe.h>

#include <error-code.h>
#include <child-process-constant.h>

#define BUFSIZE 5000

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
    gboolean completed;
};

static ChildProcess process_pool[MAX_CHILD_PROCESS] = {0};

void
initialize_child_process_system(AgentObject* agent)
{
    memset(&process_pool,0,sizeof(process_pool));
    for(gint i = 0; i < MAX_CHILD_PROCESS;i++)
    {
        process_pool[i].agent = agent;
        process_pool[i].process_id = i;
        process_pool[i].completed = TRUE;

        agent_set_child_process(agent,i,&(process_pool[i]));
    }
}






ChildProcess* 
get_available_child_process()
{
    for(gint i = 1; i < MAX_CHILD_PROCESS; i++)
    {
        if(process_pool[i].completed)
        {
            process_pool[i].completed = FALSE;
            return &(process_pool[i]);
        }
    }
    // if there is no available process, then wait for 1 second
    Sleep(1000);
    return get_available_child_process();    
}






gpointer 
handle_child_process_io(gpointer data)
{
    ChildProcess* proc = (ChildProcess*)data;
    while (TRUE)
    {
        DWORD dwRead, dwWritten;
        CHAR chBuf[BUFSIZE];
        memset(chBuf,0, BUFSIZE);
        BOOL bSuccess = FALSE;
        if (proc->completed) {return;}

        for (;;)
        {
            Sleep(10);
            bSuccess = ReadFileEx(proc->standard_out, chBuf, BUFSIZE, &dwRead, NULL);
            if (!bSuccess || dwRead == 0) { break; }

            GBytes* data = g_bytes_new(chBuf, strlen(chBuf));
            proc->func(data, proc->process_id, proc->agent);
            memset(chBuf,0, BUFSIZE);
            break;
        }
    }
}

gpointer 
handle_child_process_state(gpointer data)
{
    ChildProcess* proc = (ChildProcess*)data;
    while (TRUE)
    {
        if (proc->completed) {return;}
        DWORD ret;
        GetExitCodeProcess(proc->process, &ret);
        proc->handler(proc,ret,proc->agent);
        Sleep(100);
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

    WriteFile(self->standard_in,
        buffer, size, &written, NULL);

    WriteFile(self->standard_in,
        "\n", 1, &written, NULL);
}


void
close_child_process(ChildProcess* proc)
{
    write_to_log_file(AGENT_GENERAL_LOG,"Child process closed");
    TerminateProcess(proc->process, FORCE_EXIT);
    proc->completed = TRUE;
}

ChildProcess*
create_new_child_process(gchar* process_name,
                        gint process_id,
                        gchar* parsed_command,
                        ChildStdHandle func,
                        ChildStateHandle handler,
                        AgentObject* agent)
{
    ChildProcess* child_process = &process_pool[process_id];
    child_process->agent = agent;
    child_process->process_id = process_id;
    child_process->func = func;
    child_process->handler = handler;


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
    // startup_infor.hStdError = hdl->standard_out;

    GString* string_process = g_string_new(process_name);
    g_string_append(string_process,parsed_command);
    gchar* process = g_string_free(string_process,FALSE); 
    /*START process, all standard input and output are controlled by agent*/
    gboolean output = CreateProcess(NULL,
        process,
        NULL,
        NULL,
        TRUE,
        0,
        NULL,
        NULL,
        &startup_infor, &pi);
    g_free(process);

    if(!output)
    {
        DWORD error = GetLastError();
        agent_report_error(agent,ERROR_PROCESS_OPERATION);
        write_to_log_file(AGENT_GENERAL_LOG,ERROR_PROCESS_OPERATION);
        return NULL;        
    }

    memcpy(&child_process->process, &pi.hProcess, sizeof(HANDLE));

    g_thread_new("handle I/O",handle_child_process_io,child_process);
    g_thread_new("handle State",handle_child_process_state,child_process);

    return child_process;
}
 

gboolean 				
get_current_child_process_state(AgentObject* agent,
								gint order)
{
    ChildProcess* child_process = agent_get_child_process(agent, order);
    DWORD state = 0;
    GetExitCodeProcess(child_process->process, &state);

    if(state == STILL_ACTIVE){ return TRUE;}
    else { return FALSE; }
}





gint 
get_child_process_id(ChildProcess* process)
{
    return process->process_id;
}