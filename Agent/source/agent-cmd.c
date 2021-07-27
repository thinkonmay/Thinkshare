#include <agent-cmd.h>
#include <agent-type.h>
#include <agent-message.h>


#define BUFSIZE 10000

/// <summary>
/// information about local storage to add to the session
/// local storage will be discovered by slave device before the session
/// </summary>
struct _LocalStorage
{
	gchar* drive_name;

	gchar* url;

	gchar* group_name;
	gchar* user_name;
	gchar* password;
};

typedef struct
{
    HANDLE* standard_in;
    HANDLE* standard_out;

    HANDLE* standard_err;
}CmdHANDLE;

struct _CommandLine
{
    HANDLE* process;

    HANDLE* standard_in;
    HANDLE* standard_out;

    HANDLE* standard_err;
};

typedef struct
{
    AgentObject* agent;
    ChildHandleFunc func;
    ChildProcess proc;
    gint process_id;
}HandleTaskData;



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
            bSuccess = ReadFile(pkg->proc.standard_out, chBuf, BUFSIZE, &dwRead, NULL);
            //if (!bSuccess || dwRead == 0) break;

            int i = strlen(chBuf);
            if (i != 0)
            {
                pkg->func(pkg->process_id, pkg->agent,&chBuf,i );

                GetExitCodeProcess(pkg->proc.process, &exit_code);
                if (exit_code != STILL_ACTIVE)
                    goto end;
            }
            ZeroMemory(&chBuf, BUFSIZE);
            //bSuccess = WriteFile(hParentStdOut, chBuf,
            //    dwRead, &dwWritten, NULL);
            //if (!bSuccess) break;
        }
    }

end:
    {
        JsonObject* json = json_object_new();
        gchar* message = "childprocess ";
        gchar* exit_id = malloc(10);
        gchar prs_id = malloc(10);

        itoa(pkg->process_id, prs_id, 10);
        itoa(exit_id, exit_code, 10);

        strcat(message, prs_id);
        strcat(message, " ended with exit code ");
        strcat(message, exit_id);


        json_object_set_string_member(json,"DeviceLog", message);
        g_print(message);

        message_init(AGENT_MODULE, HOST_MODULE, UPDATE_SLAVE_STATE, json);
        send_message(pkg->agent, json);
    }
}


void
create_new_cmd_process(gint position, AgentObject* agent, gchar* first_command)
{
    gchar* command[2] = {first_command,NULL};

    ChildProcess* child_process = create_new_child_process(
        "C:\\Windows\\System32\\cmd.exe /k ", position, &command,
        command_line_output_handle, &agent);

   agent_set_child_process(agent,position, 
        child_process);
}



void
command_line_output_handle(gint process_id,
                           AgentObject* agent,
                           gchar* buffer)
{
    Message* object = json_object_new();
    json_object_set_int_member(object, "ProcessID", process_id);
    json_object_set_string_member(object, "Command", buffer);




/// <summary>
/// initialize anonymous pipe to assign to 
/// session core and session loader child process
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
CmdHANDLE*
initialize_cmd_handle(CommandLine* self)
{
    ChildPipe hdl;

    SECURITY_ATTRIBUTES attr;
    attr.nLength = sizeof(SECURITY_ATTRIBUTES);
    attr.bInheritHandle = TRUE;
    attr.lpSecurityDescriptor = NULL;

    if (!CreatePipe(&self->standard_out, &hdl.standard_out,  &attr, 0))
    {
        g_print("cannot create session core pipe");
        return NULL;
    }
    if (!CreatePipe(&hdl.standard_in, &self->standard_in, &attr, 0))
    {
        g_printerr("cannot create session core pipe");
    }

    SetHandleInformation(self->standard_in, HANDLE_FLAG_INHERIT, 0);
    SetHandleInformation(self->standard_out, HANDLE_FLAG_INHERIT, 0);
    return &hdl;
}

void
clean_process(gpointer data)
{
    HandleTaskData* hdl = (HandleTaskData*)data;
    close_child_process(hdl->proc.process);
}



ChildProcess*
create_new_child_process(gchar* process_name,
                         gint process_id,
                         gchar** command_array,
                         ChildHandleFunc func,
                         AgentObject* agent)
{
    CommandLine* cmd = malloc(sizeof(CommandLine));

    PROCESS_INFORMATION pi;
    ZeroMemory(&pi, sizeof(pi)); 


    ChildPipe* hdl = initialize_process_handle(&child_process);
    PROCESS_INFORMATION pi;
    ZeroMemory(&pi, sizeof(pi));

    /*setup startup infor(included standard input and output)*/
    STARTUPINFO startup_infor;
    startup_infor.cb = sizeof(STARTUPINFO);
    startup_infor.dwFlags = STARTF_USESTDHANDLES;
    startup_infor.hStdInput = hdl->standard_in;
    startup_infor.hStdOutput = hdl->standard_out;
    startup_infor.hStdError = hdl->standard_err;



    int i = 0;
    while(*command_array[i] != NULL)
    {
        strcat(process_name, *(command_array + i));
        i++;
    } 



    LPSTR path = g_win32_locale_filename_from_utf8(process_name);
    /*START process, all standard input and output are controlled by agent*/
    CreateProcess(NULL,
        path,
        NULL,
        NULL,
        TRUE,
        0,
        NULL,
        NULL,
        &startup_infor, &pi);

    memcpy(&child_process.process,  &pi.hProcess,sizeof(HANDLE));
    CloseHandle(pi.hThread);

    HandleTaskData* data = malloc(sizeof(HandleTaskData));
    data->agent = agent;
    data->func = func;
    data->proc = child_process;
    data->process_id = process_id;


    g_thread_new("child handle thread", handle_child_process_thread, data);

    
    return &child_process;
}



void
close_command_line_process(CommandLine* cmd)
{
    TerminateProcess(*(cmd->process), 0);
    CloseHandle(*(cmd->standard_err));
    CloseHandle(*(cmd->standard_out));
    CloseHandle(*(cmd->standard_in));
    cmd = NULL;
}




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

        if (success || written == sizeof(buffer))
            return TRUE;
    }
}