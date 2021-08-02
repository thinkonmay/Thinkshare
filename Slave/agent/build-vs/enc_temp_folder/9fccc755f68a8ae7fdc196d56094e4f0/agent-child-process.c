#include <agent-child-process.h>
#include <agent-type.h>
#include <glib.h>
#include <Windows.h>


#define BUFSIZE 10000

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
    HANDLE process;

    HANDLE standard_in;
    HANDLE standard_out;
};


typedef struct
{
    AgentObject* agent;
    ChildHandleFunc func;
    ChildProcess proc;
    gint process_id;
}HandleTaskData;








gpointer
handle_child_process_thread(gpointer data);
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
    ChildProcess child_process;


    ChildPipe* hdl = initialize_process_handle(&child_process);

    PROCESS_INFORMATION pi;
    ZeroMemory(&pi, sizeof(pi));

    /*setup startup infor(included standard input and output)*/
    STARTUPINFO startup_infor;
    ZeroMemory(&startup_infor, sizeof(startup_infor));
    startup_infor.cb = sizeof(STARTUPINFO);
    startup_infor.dwFlags = STARTF_USESTDHANDLES;
    startup_infor.hStdInput = hdl->standard_in;
    startup_infor.hStdOutput = hdl->standard_out;
    startup_infor.hStdError = hdl->standard_out;




    strcat(process_name, *command_array);



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

    memcpy(&child_process.process, &pi.hProcess, sizeof(HANDLE));
    CloseHandle(pi.hThread);

    HandleTaskData* data = malloc(sizeof(HandleTaskData));
    data->agent = agent;
    data->func = func;
    data->proc = child_process;
    data->process_id = process_id;


    g_thread_new("child handle thread", handle_child_process_thread, data);


    return &child_process;
}


static ChildPipe*
initialize_process_handle(ChildProcess* self)
{
    ChildPipe hdl;

    SECURITY_ATTRIBUTES attr;
    attr.nLength = sizeof(SECURITY_ATTRIBUTES);
    attr.bInheritHandle = TRUE;
    attr.lpSecurityDescriptor = NULL;

    if (!CreatePipe(&hdl.standard_out, &self->standard_out, &attr, 0))
    {
        g_printerr("cannot create session core pipe");
    }
    if (!CreatePipe(&hdl.standard_in, &self->standard_in, &attr, 0))
    {
        g_printerr("cannot create session core pipe");
    }

    SetHandleInformation(hdl.standard_in, HANDLE_FLAG_INHERIT, 0);
    SetHandleInformation(hdl.standard_out, HANDLE_FLAG_INHERIT, 0);
    return &hdl;
}

void
clean_process(gpointer data)
{
    HandleTaskData* hdl = (HandleTaskData*)data;
    close_child_process(hdl->proc.process);
}


gpointer
handle_child_process_thread(gpointer data)
{
    HandleTaskData* pkg = (HandleTaskData*)data;
    DWORD exit_code = STILL_ACTIVE;

    while (TRUE)
    {
        DWORD dwRead, dwWritten;
        CHAR chBuf[BUFSIZE];
        ZeroMemory(&chBuf, BUFSIZE);
        BOOL bSuccess = FALSE;

        HANDLE hParentStdOut = GetStdHandle(STD_OUTPUT_HANDLE);

        for (;;)
        {
            for (;;)
            {
                bSuccess = ReadFile(pkg->proc.standard_out, chBuf, BUFSIZE, &dwRead, NULL);
                if (!bSuccess || dwRead == 0) break;

                bSuccess = WriteFile(hParentStdOut, chBuf, dwRead, &dwWritten, NULL);
                if (!bSuccess) break;
            }


            GetExitCodeProcess(pkg->proc.process, &exit_code);
            if (exit_code != STILL_ACTIVE)
                goto end;
            
            ZeroMemory(&chBuf, BUFSIZE);
            //bSuccess = WriteFile(hParentStdOut, chBuf,
            //    dwRead, &dwWritten, NULL);
            //if (!bSuccess) break;
        }
    }

end:
    {
        //JsonObject* json = json_object_new();
        //gchar* message = "childprocess ";
        //gchar* exit_id = malloc(10);
        //gchar prs_id = malloc(10);
        //ZeroMemory(&exit_id, 10);
        //ZeroMemory(&prs_id, 10);

        //itoa(pkg->process_id, &prs_id, 10);
        //itoa(exit_code, &exit_id, 10);

        g_print("child process %d end with code %d ", pkg->process_id, exit_code);


        //json_object_set_string_member(json, "DeviceLog", message);
        //g_print(message);

        //message_init(AGENT_MODULE, HOST_MODULE, UPDATE_SLAVE_STATE, json);
        //send_message(pkg->agent, json);
    }
}