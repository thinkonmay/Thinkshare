#include "Framework.h"
#include "Variable.h"


gboolean
check_plugins();

gboolean
cleanup_and_quit_loop(const gchar* msg, enum AppState state);

gboolean
start_pipeline();

void
handle_media_stream(gint screen_width,
    gint screen_height,
    gint framerate,
    const gchar* encoder);






