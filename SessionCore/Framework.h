#pragma once
/*START GLIB BASED LIBRARY*/
#include <gst\gst.h>
#include <gst\sdp\sdp.h>
#include <glib-2.0\glib.h>
#include <gst\webrtc\webrtc.h>
#include <gst\rtp\gstrtppayloads.h>
#include <libsoup\soup-types.h>
#include <libsoup\soup-websocket.h>
#include <gst\webrtc\webrtc_fwd.h>
#include <json-glib\json-glib.h>
#include <libsoup\soup.h>
#include <gstreamer-1.0\gst\webrtc\webrtc.h>
#include <gst/controller/gstinterpolationcontrolsource.h>
#include <gst/controller/gstdirectcontrolbinding.h>
/*END GLIB BASED LIBRARY*/

#include "msgpack.h"
#include <shared-memory-hub.h>
#include <shared-memory-link.h>

#include <Windows.h>
#include <iostream>
#include <sstream>
#include <string.h>

#define GST_USE_UNSTABLE_API