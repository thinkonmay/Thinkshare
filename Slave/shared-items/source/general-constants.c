#include <general-constant.h>


#include <glib.h>


gchar   dir[200] = {0};
gint terminated_path_size;


gchar* 
get_root_dir()
{
    gchar* ret = malloc(200);
    memset(ret,0,200);
    gint dir_length = strlen(dir) - terminated_path_size;
    memcpy(ret,dir,dir_length);
    return ret;
}

void
thinkmay_init(gchar* binary_path,
              gint terminated_path)
{
    terminated_path_size = terminated_path;
    memcpy(dir,binary_path,strlen(binary_path));
    get_root_dir();

    SHELL_SCRIPT_BUFFER_1    =     get_absolute_path(SHELL_SCRIPT"_1.ps1");   
    SHELL_SCRIPT_BUFFER_2    =     get_absolute_path(SHELL_SCRIPT"_2.ps1");   
    SHELL_SCRIPT_BUFFER_3    =     get_absolute_path(SHELL_SCRIPT"_3.ps1");   
    SHELL_SCRIPT_BUFFER_4    =     get_absolute_path(SHELL_SCRIPT"_4.ps1");   
    SHELL_SCRIPT_BUFFER_5    =     get_absolute_path(SHELL_SCRIPT"_5.ps1");   
    SHELL_SCRIPT_BUFFER_6    =     get_absolute_path(SHELL_SCRIPT"_6.ps1");   
    SHELL_SCRIPT_BUFFER_7    =     get_absolute_path(SHELL_SCRIPT"_7.ps1");   
    SHELL_SCRIPT_BUFFER_8    =     get_absolute_path(SHELL_SCRIPT"_8.ps1");    

    SHELL_OUTPUT_BUFFER_1    =     get_absolute_path(SHELL_OUTPUT"_1.json");   
    SHELL_OUTPUT_BUFFER_2    =     get_absolute_path(SHELL_OUTPUT"_2.json");   
    SHELL_OUTPUT_BUFFER_3    =     get_absolute_path(SHELL_OUTPUT"_3.json");   
    SHELL_OUTPUT_BUFFER_4    =     get_absolute_path(SHELL_OUTPUT"_4.json");   
    SHELL_OUTPUT_BUFFER_5    =     get_absolute_path(SHELL_OUTPUT"_5.json");   
    SHELL_OUTPUT_BUFFER_6    =     get_absolute_path(SHELL_OUTPUT"_6.json");   
    SHELL_OUTPUT_BUFFER_7    =     get_absolute_path(SHELL_OUTPUT"_7.json");  
    SHELL_OUTPUT_BUFFER_8    =     get_absolute_path(SHELL_OUTPUT"_8.json"); 

    FILE_COMPRESSOR_OUTPUT_1 =     get_absolute_path(FILE_COMPRESSOR_OUTPUT"_1.zip");  
    FILE_COMPRESSOR_OUTPUT_2 =     get_absolute_path(FILE_COMPRESSOR_OUTPUT"_2.zip"); 
    FILE_COMPRESSOR_OUTPUT_3 =     get_absolute_path(FILE_COMPRESSOR_OUTPUT"_3.zip");  
    FILE_COMPRESSOR_OUTPUT_4 =     get_absolute_path(FILE_COMPRESSOR_OUTPUT"_4.zip"); 
    FILE_COMPRESSOR_OUTPUT_5 =     get_absolute_path(FILE_COMPRESSOR_OUTPUT"_5.zip");  
    FILE_COMPRESSOR_OUTPUT_6 =     get_absolute_path(FILE_COMPRESSOR_OUTPUT"_6.zip"); 
    FILE_COMPRESSOR_OUTPUT_7 =     get_absolute_path(FILE_COMPRESSOR_OUTPUT"_7.zip");  
    FILE_COMPRESSOR_OUTPUT_8 =     get_absolute_path(FILE_COMPRESSOR_OUTPUT"_8.zip"); 
}



gchar* 
get_absolute_path(const char* relative_path)
{
    gchar* text = get_root_dir();
    GString* string = g_string_new(text);
    g_string_append(string,relative_path);
    gchar* ret = g_string_free(string,FALSE);
    return ret;
}



