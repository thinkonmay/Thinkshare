#include <child-process-resources-assign.h>
#include <child-process-constant.h>
#include <general-constant.h>




gchar*
shell_script_map(gint process_id)
{
    switch(process_id)
    {
        case POWERSHELL_1:
            return SHELL_SCRIPT_BUFFER_1;
        case POWERSHELL_2:
            return SHELL_SCRIPT_BUFFER_2;
        case POWERSHELL_3:
            return SHELL_SCRIPT_BUFFER_3;
        case POWERSHELL_4:
            return SHELL_SCRIPT_BUFFER_4;
        case POWERSHELL_5:
            return SHELL_SCRIPT_BUFFER_5;
        case POWERSHELL_6:
            return SHELL_SCRIPT_BUFFER_6;
        case POWERSHELL_7:
            return SHELL_SCRIPT_BUFFER_7;
        case POWERSHELL_8:
            return SHELL_SCRIPT_BUFFER_8;
    }
}

gchar*
shell_output_map(gint process_id)
{
    switch(process_id)
    {
        case POWERSHELL_1:
            return SHELL_OUTPUT_BUFFER_1;
        case POWERSHELL_2:
            return SHELL_OUTPUT_BUFFER_2;
        case POWERSHELL_3:
            return SHELL_OUTPUT_BUFFER_3;
        case POWERSHELL_4:
            return SHELL_OUTPUT_BUFFER_4;
        case POWERSHELL_5:
            return SHELL_OUTPUT_BUFFER_5;
        case POWERSHELL_6:
            return SHELL_OUTPUT_BUFFER_6;
        case POWERSHELL_7:
            return SHELL_OUTPUT_BUFFER_7;
        case POWERSHELL_8:
            return SHELL_OUTPUT_BUFFER_8;
    }
}


gchar* 
output_zip_map(gint process_id)
{
    switch (process_id)
    {
    case FILE_COMPRESSOR_SERVICE_1:
        return FILE_COMPRESSOR_OUTPUT_1;
    case FILE_COMPRESSOR_SERVICE_2:
        return FILE_COMPRESSOR_OUTPUT_2;
    case FILE_COMPRESSOR_SERVICE_3:
        return FILE_COMPRESSOR_OUTPUT_3;
    case FILE_COMPRESSOR_SERVICE_4:
        return FILE_COMPRESSOR_OUTPUT_4;
    case FILE_COMPRESSOR_SERVICE_5:
        return FILE_COMPRESSOR_OUTPUT_5;
    case FILE_COMPRESSOR_SERVICE_6:
        return FILE_COMPRESSOR_OUTPUT_6;    
    case FILE_COMPRESSOR_SERVICE_7:
        return FILE_COMPRESSOR_OUTPUT_7;
    case FILE_COMPRESSOR_SERVICE_8:
        return FILE_COMPRESSOR_OUTPUT_8;
    }
}



