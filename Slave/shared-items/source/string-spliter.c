#include <string-spliter.h>

#include <glib-2.0/glib.h>


gchar** 
string_spliter(gchar* input_string,
               gchar delimiter)
{
    gint counter = 0;


    while(*(input_string++))
    {
        if(*input_string == delimiter)
        {
            count ++;
            length ++;
        }
    }

    string -= (length+1);
    gchar **array = (char **)malloc(sizeof(char *) * (length + 1));
    gchar ** base = array;
    for(i = 0; i < (count + 1); i++) 
    {
        j = 0;
        while(string[j] != delimiter) j++;
        j++;
        *array = (char *)malloc(sizeof(char) * j);
        memcpy(*array, string, (j-1));
        (*array)[j-1] = '\0';
        string += j;
        array++;
    }
    *array = '\0';
    return base; 
}