#include <agent-cmd.h>
#include <agent-type.h>
#include <agent-message.h>


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


CommandLine* 
initialize_command_line(void)
{
    CommandLine* cmd = malloc(sizeof(CommandLine));
    memset(cmd, 0, sizeof(CommandLine));

    return cmd;
}




gpointer
handle_command_line_thread(gpointer data)
{
    AgentObject* agent = (AgentObject*)data;
    CommandLine** cmd = agent_get_command_line_array(agent);

    while (TRUE)
    {
        DWORD dwread, dwrite;
        gchar buffer[BUFFER_SIZE];
        gboolean success = FALSE;

        for (gint i = 0; i < 8; i++)
        {
            success = ReadFile(
                *((*(cmd + i))->standard_out), buffer, BUFFER_SIZE, &dwread, NULL);

            if (!success || dwread == 0)
            {
                Message* object = json_object_new();
                json_object_set_int_member(object, "order", i);
                json_object_set_string_member(object, "command", buffer);

                Message* msg = message_init(AGENT_MODULE, HOST_MODULE, ON_COMMAND_LINE, object);

                agent_send_message(agent, msg);
            }

            success = ReadFile(*((*(cmd + i))->standard_err), buffer, BUFFER_SIZE, &dwread, NULL);
            if (!success || dwread == 0)
            {
                Message* object = json_object_new();
                json_object_set_int_member(object, "order", i);
                json_object_set_string_member(object, "command", buffer);

                Message* msg = message_init(AGENT_MODULE, HOST_MODULE, ON_COMMAND_LINE, object);

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

    if (!CreatePipe(*(hdl->standard_out), *(self->standard_out), &attr, 0) ||
        CreatePipe(*(hdl->standard_in), *(self->standard_in), &attr, 0) ||
        CreatePipe(*(hdl->standard_err), *(self->standard_err), &attr, 0) )
    {
        g_print("cannot create session core pipe");
        return NULL;
    }

    return hdl;
}


CommandLine*
create_new_command_line_process(void)
{
    CommandLine* cmd = malloc(sizeof(CommandLine));

    PROCESS_INFORMATION pi;
    ZeroMemory(&pi, sizeof(pi)); 


    CmdHANDLE* hdl = initialize_cmd_handle(cmd);

    if (hdl == NULL)
    {
        g_printerr("cannot create std pipe for cmd process");
        return FALSE;
    }

    /*setup startup infor(included standard input and output)*/
    STARTUPINFO startup_infor;
    startup_infor.cb = sizeof(STARTUPINFO);
    startup_infor.dwFlags = STARTF_USESTDHANDLES;
    startup_infor.hStdInput = hdl->standard_in;
    startup_infor.hStdOutput = hdl->standard_out;
    startup_infor.hStdError = hdl->standard_err;


    /*START cmd.exe process, all standard input and output are controlled by agent*/
    if (CreateProcess("cmd.exe", "C:\\Windows\\System32\\cmd.exe",
        NULL, NULL, TRUE, CREATE_NEW_CONSOLE,
        NULL, NULL, &startup_infor, &pi) == 0)
    {
        DWORD drError = GetLastError();
        g_printerr("fail to create commandline process\n");
        return NULL;
    }
    else
        cmd->process=  &pi.hProcess;
        return cmd;
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