#ifndef __CHILD_PROCESS_RESOURCES_ASSIGN_H__
#define __CHILD_PROCESS_RESOURCES_ASSIGN_H__


#include <glib-2.0/glib.h>

/// <summary>
/// 
/// </summary>
/// <param name="process_id"></param>
/// <returns></returns>
gchar*              shell_output_map                (gint process_id);

/// <summary>
/// 
/// </summary>
/// <param name="process_id"></param>
/// <returns></returns>
gchar*              shell_script_map                (gint process_id);

/// <summary>
/// 
/// </summary>
/// <param name="process_id"></param>
/// <returns></returns>
gchar*              output_zip_map                  (gint process_id);

#endif