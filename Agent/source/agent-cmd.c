#include <agent-cmd.h>


/// <summary>
/// information about local storage to add to the session
/// local storage will be discovered by slave device before the session
/// </summary>
typedef struct _LocalStorage
{
	gchar* drive_name;

	gchar* url;

	gchar* group_name;
	gchar* user_name;
	gchar* password;
};