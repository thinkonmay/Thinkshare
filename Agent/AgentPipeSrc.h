#pragma once
#include "Framework.h"
#include "Variable.h"










void
read_string(HANDLE fh, gchar* output);

void
send_byte(HANDLE fh, GBytes* input);

void
send_string(HANDLE fh, gchar* input);

void
agent_pipe_src_handle();