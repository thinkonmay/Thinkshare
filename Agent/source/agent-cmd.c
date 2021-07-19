#include <agent-cmd.h>
#include <agent-type.h>
#include <agent-message.h>
#include <gmodule.h>

#define BUFFER_SIZE 10000

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
    HANDLE standard_in;
    HANDLE standard_out;
}CmdHANDLE;

struct _CommandLine
{
    HANDLE* process;

    HANDLE standard_in;
    HANDLE standard_out;
};




void
handle_command_line_thread(GTask* task,
    gpointer source_object,
    gpointer data,
    GCancellable* cancellable)
{
    AgentObject* agent = g_task_get_task_data(task);
    CommandLine** cmd = agent_get_command_line_array(agent);

    while (TRUE)
    {
        DWORD dwread = 0;
        gchar buffer[BUFFER_SIZE];
        gboolean success = FALSE;

        for (gint i = 0; i < 8; i++)
        {
            CommandLine* line = *(cmd + i);
            success = ReadFile(line->standard_out, buffer, BUFFER_SIZE, &dwread, NULL);
             
            if (success)
            {
                Message* object = json_object_new();
                json_object_set_int_member(object, "Order", i);
                json_object_set_string_member(object, "Command", buffer);

                Message* msg = message_init(AGENT_MODULE, HOST_MODULE,
                    COMMAND_LINE_FORWARD, object);

                agent_send_message(agent, msg);
            }
        } 
    }
}






/// <summary>
/// initialize anonymous pipe to assign to 
/// session core and session loader child process
/// </summary>
/// <param name="self"></param>
/// <returns></returns>
CmdHANDLE*
initialize_cmd_handle(CommandLine* self)
{
    CmdHANDLE* hdl = malloc(sizeof(CmdHANDLE));

    SECURITY_ATTRIBUTES attr;
    attr.nLength = sizeof(SECURITY_ATTRIBUTES);
    attr.bInheritHandle = TRUE;
    attr.lpSecurityDescriptor = NULL;

    if (!CreatePipe(&hdl->standard_out, &self->standard_out, &attr, 0))
    {
        g_printerr("cannot create session core pipe");
    }
    if (!CreatePipe(&hdl->standard_in, &self->standard_in, &attr, 0))
    {
        g_printerr("cannot create session core pipe");
    }

    SetHandleInformation(hdl->standard_in, HANDLE_FLAG_INHERIT, 0);
    SetHandleInformation(hdl->standard_out, HANDLE_FLAG_INHERIT, 0);
    return hdl;
}


CommandLine*
create_new_command_line_process(void)
{
    CommandLine* cmd = malloc(sizeof(CommandLine));


    CmdHANDLE* hdl = initialize_cmd_handle(cmd);

    if (hdl == NULL)
    {
        g_printerr("cannot create std pipe for cmd process");
        return FALSE;
    }

    PROCESS_INFORMATION pi;
    ZeroMemory(&pi, sizeof(pi));

    /*setup startup infor(included standard input and output)*/
    STARTUPINFO startup_infor;
    startup_infor.cb = sizeof(STARTUPINFO);
    startup_infor.dwFlags |= STARTF_USESTDHANDLES;
    startup_infor.hStdInput = hdl->standard_in;
    startup_infor.hStdOutput = hdl->standard_out;
    startup_infor.hStdError = hdl->standard_out;


    LPWSTR path = malloc(1000);
    LPWSTR path_ = g_utf8_to_utf16("C:\\Windows\\System32\\cmd.exe\0",
        (glong)strlen("C:\\Windows\\System32\\cmd.exe\0"), 
        NULL, NULL, NULL);

    memcpy(path, path_, sizeof(path_));

    gint t = sizeof(path);
    /*START cmd.exe process, all standard input and output are controlled by agent*/
    if (!CreateProcessW(NULL, 
        (LPWSTR)path,
        NULL,
        NULL, 
        TRUE, 
        0,
        NULL,
        NULL, 
        &startup_infor, &pi) == 0)
    {
        DWORD drError = GetLastError();
        g_printerr("fail to create commandline process\n");
    }
    cmd->process=  &pi.hProcess;

    return cmd;
}



void
close_command_line_process(CommandLine* cmd)
{
    TerminateProcess(*(cmd->process), 0);
    CloseHandle(cmd->standard_out);
    CloseHandle(cmd->standard_in);
    cmd = NULL;
}




gboolean
send_command_line(CommandLine* self, gchar* buffer)
{
    DWORD written;
    gboolean success = FALSE;
    while (TRUE)
    {
        success = WriteFile(self->standard_in,
            buffer, sizeof(buffer), &written, NULL);

        if (success || written == sizeof(buffer))
            return TRUE;
    }
}