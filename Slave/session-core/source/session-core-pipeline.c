#include <session-core-pipeline.h>
#include <session-core-type.h>
#include <session-core-data-channel.h>
#include <session-core-signalling.h>
#include <session-core-remote-config.h>


#include <general-constant.h>
#include <logging.h>
#include <qoe.h>
#include <exit-code.h>

#include <gst\gst.h>
#include <glib-2.0\glib.h>
#include <gst\webrtc\webrtc.h>
#include <gst\rtp\gstrtppayloads.h>

#include <Windows.h>




/// <summary>
/// gstreamer video element enumaration,
/// the order of element in enum must follow the 
/// </summary>
enum
{
    /*screen capture source*/
    DX9_SCREEN_CAPTURE_SOURCE,

    /*preprocess before encoding*/
    CUDA_UPLOAD,
    CUDA_CONVERT,
    VIDEO_CONVERT,
    /*video encoder*/

    NVIDIA_H264_ENCODER,
    NVIDIA_H265_ENCODER,
    H264_MEDIA_FOUNDATION,
    H265_MEDIA_FOUNDATION,

    VP9_ENCODER,
    VP8_ENCODER,


    /*payload packetize*/
    RTP_H264_PAYLOAD,
    RTP_H265_PAYLOAD,
    RTP_VP9_PAYLOAD,
    RTP_VP8_PAYLOAD,

    VIDEO_ELEMENT_LAST
};

/// <summary>
/// gstreamer audio element enumaration,
/// the order of element in enum must follow the 
/// </summary>
enum
{
    /*audio capture source*/
    PULSE_SOURCE_SOUND,
    WASAPI_SOURCE_SOUND,

    /*audio encoder*/
    OPUS_ENCODER,
    MP3_ENCODER,
    AAC_ENCODER,

    /*rtp packetize and queue*/
    RTP_OPUS_PAYLOAD,
    RTP_RTX_QUEUE,

    AUDIO_ELEMENT_LAST
};


struct _Pipeline
{
    PipelineState state;

	GstElement* pipeline;
	GstElement* webrtcbin;

    GstElement* video_element[VIDEO_ELEMENT_LAST];
    GstElement* audio_element[AUDIO_ELEMENT_LAST];

    GstCaps* video_caps[VIDEO_ELEMENT_LAST];
    GstCaps* audio_caps[AUDIO_ELEMENT_LAST];
};



Pipeline*
pipeline_initialize(SessionCore* core)
{
    SignallingHub* hub = session_core_get_signalling_hub(core);

    static Pipeline pipeline;
    ZeroMemory(&pipeline,sizeof(pipeline));
    

    pipeline.state = PIPELINE_NOT_READY;
    return &pipeline;
}

static gboolean
start_pipeline(SessionCore* core)
{
    GstStateChangeReturn ret;
    Pipeline* pipe = session_core_get_pipeline(core);

    ret = GST_IS_ELEMENT(pipe->pipeline);    

    ret = gst_element_set_state(GST_ELEMENT(pipe->pipeline), GST_STATE_PLAYING);
    if (ret == GST_STATE_CHANGE_FAILURE)
    {
        GError error;
        error.message = "Fail to start pipeline, this may due to pipeline setup failure";
        session_core_finalize(core, PIPELINE_ERROR_EXIT,&error);
    }
    write_to_log_file(SESSION_CORE_GENERAL_LOG,"Starting pipeline\n");
    return TRUE;
}

#define SCREEN_CAP    "video/x-raw,framerate=60/1 ! "
#define RTP_CAPS_OPUS "application/x-rtp,media=audio,payload=96,encoding-name="
#define RTP_CAPS_VIDEO "application/x-rtp,media=video,payload=97,encoding-name="

/// <summary>
/// information about all element which have been used in pipeline
/// </summary>

///////////////////////////////////////////////////////////////////////////////////////////////////
//
// Factory Details:
//   Rank                     secondary (128)
//   Long-name                Media Foundation Intel? Quick Sync Video H.264 Encoder MFT
//   Klass                    Codec/Encoder/Video/Hardware
//   Description              Microsoft Media Foundation H.264 Encoder
//   Author                   Seungha Yang <seungha.yang@navercorp.com>

// Plugin Details:
//   Name                     mediafoundation
//   Description              Microsoft Media Foundation plugin
//   Filename                 C:\gstreamer\1.0\msvc_x86_64\lib\gstreamer-1.0\gstmediafoundation.dll
//   Version                  1.18.4
//   License                  LGPL
//   Source module            gst-plugins-bad
//   Source release date      2021-03-15
//   Binary package           GStreamer Bad Plug-ins source release
//   Origin URL               Unknown package origin

// GObject
//  +----GInitiallyUnowned
//        +----GstObject
//              +----GstElement
//                    +----GstVideoEncoder
//                          +----GstMFVideoEnc
//                                +----GstMFH264Enc

// Implemented Interfaces:
//   GstPreset

// Pad Templates:
//   SRC template: 'src'
//     Availability: Always
//     Capabilities:
//       video/x-h264
//           stream-format: byte-stream
//               alignment: au
//                 profile: { (string)high, (string)main, (string)baseline }
//                   width: [ 64, 8192 ]
//                  height: [ 64, 8192 ]
  
//   SINK template: 'sink'
//     Availability: Always
//     Capabilities:
//       video/x-raw
//                  format: { (string)NV12, (string)BGRA }
//                   width: [ 64, 8192 ]
//                  height: [ 64, 8192 ]

// Element has no clocking capabilities.
// Element has no URI handling capabilities.

// Pads:
//   SINK: 'sink'
//     Pad Template: 'sink'
//   SRC: 'src'
//     Pad Template: 'src'

// Element Properties:
//   bitrate             : Bitrate in kbit/sec
//                         flags: readable, writable
//                         Unsigned Integer. Range: 1 - 4194303 Default: 2048 
//   cabac               : Enable CABAC entropy coding
//                         flags: readable, writable, conditionally available
//                         Boolean. Default: true
//   gop-size            : The number of pictures from one GOP header to the next, (0 = MFT default)
//                         flags: readable, writable, conditionally available
//                         Unsigned Integer. Range: 0 - 4294967294 Default: 0 
//   low-latency         : Enable low latency encoding
//                         flags: readable, writable, conditionally available
//                         Boolean. Default: false
//   max-bitrate         : The maximum bitrate applied when rc-mode is "pcvbr" in kbit/sec
//                         flags: readable, writable, conditionally available
//                         Unsigned Integer. Range: 0 - 4194303 Default: 0 
//   max-qp              : The maximum allowed QP applied to all rc-mode
//                         flags: readable, writable, conditionally available
//                         Unsigned Integer. Range: 0 - 51 Default: 51 
//   min-force-key-unit-interval: Minimum interval between force-keyunit requests in nanoseconds
//                         flags: readable, writable
//                         Unsigned Integer64. Range: 0 - 18446744073709551615 Default: 0 
//   min-qp              : The minimum allowed QP applied to all rc-mode
//                         flags: readable, writable, conditionally available
//                         Unsigned Integer. Range: 0 - 51 Default: 0 
//   name                : The name of the object
//                         flags: readable, writable, 0x2000
//                         String. Default: "mfh264enc0"
//   parent              : The parent of the object
//                         flags: readable, writable, 0x2000
//                         Object of type "GstObject"
//   qos                 : Handle Quality-of-Service events from downstream
//                         flags: readable, writable
//                         Boolean. Default: false
//   qp                  : QP applied when rc-mode is "qvbr"
//                         flags: readable, writable, conditionally available
//                         Unsigned Integer. Range: 16 - 51 Default: 24 
//   qp-b                : QP applied to B frames
//                         flags: readable, writable, conditionally available
//                         Unsigned Integer. Range: 0 - 51 Default: 26 
//   qp-i                : QP applied to I frames
//                         flags: readable, writable, conditionally available
//                         Unsigned Integer. Range: 0 - 51 Default: 26 
//   qp-p                : QP applied to P frames
//                         flags: readable, writable, conditionally available
//                         Unsigned Integer. Range: 0 - 51 Default: 26 
//   quality-vs-speed    : Quality and speed tradeoff, [0, 33]: Low complexity, [34, 66]: Medium complexity, [67, 100]: High complexity
//                         flags: readable, writable, conditionally available
//                         Unsigned Integer. Range: 0 - 100 Default: 50 
//   rc-mode             : Rate Control Mode
//                         flags: readable, writable, conditionally available
//                         Enum "GstMFH264EncRCMode" Default: 2, "uvbr"
//                            (0): cbr              - Constant bitrate
//                            (1): pcvbr            - Peak Constrained variable bitrate
//                            (2): uvbr             - Unconstrained variable bitrate
//                            (3): qvbr             - Quality-based variable bitrate
//   ref                 : The number of reference frames
//                         flags: readable, writable, conditionally available
//                         Unsigned Integer. Range: 0 - 16 Default: 2 
//   vbv-buffer-size     : VBV(HRD) Buffer Size in bytes (0 = MFT default)
//                         flags: readable, writable, conditionally available
//                         Unsigned Integer. Range: 0 - 4294967294 Default: 0 
//
///////////////////////////////////////////////////////////////////////////////////////////////////




///////////////////////////////////////////////////////////////////////////////////////////////////
//
// Factory Details:
//   Rank                     none (0)
//   Long-name                DirectX 9 screen capture source
//   Klass                    Source/Video
//   Description              Captures screen
//   Author                   Haakon Sporsheim <hakon.sporsheim@tandberg.com>

// Plugin Details:
//   Name                     winscreencap
//   Description              Screen capture plugin for Windows
//   Filename                 C:\gstreamer\1.0\msvc_x86_64\lib\gstreamer-1.0\gstwinscreencap.dll
//   Version                  1.18.4
//   License                  LGPL
//   Source module            gst-plugins-bad
//   Source release date      2021-03-15
//   Binary package           GStreamer Bad Plug-ins source release
//   Origin URL               Unknown package origin

// GObject
//  +----GInitiallyUnowned
//        +----GstObject
//              +----GstElement
//                    +----GstBaseSrc
//                          +----GstPushSrc
//                                +----GstDX9ScreenCapSrc

// Pad Templates:
//   SRC template: 'src'
//     Availability: Always
//     Capabilities:
//       video/x-raw
//                  format: BGR
//                   width: [ 1, 2147483647 ]
//                  height: [ 1, 2147483647 ]
//               framerate: [ 0/1, 2147483647/1 ]

// Element has no clocking capabilities.
// Element has no URI handling capabilities.

// Pads:
//   SRC: 'src'
//     Pad Template: 'src'

// Element Properties:
//   blocksize           : Size in bytes to read per buffer (-1 = default)
//                         flags: readable, writable
//                         Unsigned Integer. Range: 0 - 4294967295 Default: 4096 
//   cursor              : Whether to show mouse cursor (default off)
//                         flags: readable, writable
//                         Boolean. Default: false
//   do-timestamp        : Apply current stream time to buffers
//                         flags: readable, writable
//                         Boolean. Default: false
//
//   height              : Height of screen capture area (0 = maximum)
//                         flags: readable, writable
//                         Integer. Range: 0 - 2147483647 Default: 0 
//   monitor             : Which monitor to use (0 = 1st monitor and default)
//                         flags: readable, writable
//                         Integer. Range: 0 - 2147483647 Default: 0 
//   name                : The name of the object
//                         flags: readable, writable, 0x2000
//                         String. Default: "dx9screencapsrc0"
//   num-buffers         : Number of buffers to output before sending EOS (-1 = unlimited)
//                         flags: readable, writable
//                         Integer. Range: -1 - 2147483647 Default: -1 
//   parent              : The parent of the object
//                         flags: readable, writable, 0x2000
//                         Object of type "GstObject"
//   typefind            : Run typefind before negotiating (deprecated, non-functional)
//                         flags: readable, writable, deprecated
//                         Boolean. Default: false
//   width               : Width of screen capture area (0 = maximum)
//                         flags: readable, writable
//                         Integer. Range: 0 - 2147483647 Default: 0 
//   x                   : Horizontal coordinate of top left corner for the screen capture area
//                         flags: readable, writable
//                         Integer. Range: 0 - 2147483647 Default: 0 
//   y                   : Vertical coordinate of top left corner for the screen capture area
//                         flags: readable, writable
//                         Integer. Range: 0 - 2147483647 Default: 0 
//
///////////////////////////////////////////////////////////////////////////////////////////////////



///////////////////////////////////////////////////////////////////////////////////////////////////
//
//
//
//Factory Details:
//   Rank                     secondary (128)
//   Long-name                RTP H264 payloader
//   Klass                    Codec/Payloader/Network/RTP
//   Description              Payload-encode H264 video into RTP packets (RFC 3984)
//   Author                   Laurent Glayal <spglegle@yahoo.fr>

// Plugin Details:
//   Name                     rtp
//   Description              Real-time protocol plugins
//   Filename                 C:\gstreamer\1.0\msvc_x86_64\lib\gstreamer-1.0\gstrtp.dll
//   Version                  1.18.4
//   License                  LGPL
//   Source module            gst-plugins-good
//   Source release date      2021-03-15
//   Binary package           GStreamer Good Plug-ins source release
//   Origin URL               Unknown package origin

// GObject
//  +----GInitiallyUnowned
//        +----GstObject
//              +----GstElement
//                    +----GstRTPBasePayload
//                          +----GstRtpH264Pay

// Pad Templates:
//   SINK template: 'sink'
//     Availability: Always
//     Capabilities:
//       video/x-h264
//           stream-format: avc
//               alignment: au
//       video/x-h264
//           stream-format: byte-stream
//               alignment: { (string)nal, (string)au }
  
//   SRC template: 'src'
//     Availability: Always
//     Capabilities:
//       application/x-rtp
//                   media: video
//                 payload: [ 96, 127 ]
//              clock-rate: 90000
//           encoding-name: H264

// Element has no clocking capabilities.
// Element has no URI handling capabilities.

// Pads:
//   SRC: 'src'
//     Pad Template: 'src'
//   SINK: 'sink'
//     Pad Template: 'sink'

// Element Properties:
//   aggregate-mode      : Bundle suitable SPS/PPS NAL units into STAP-A aggregate packets
//                         flags: readable, writable
//                         Enum "GstRtpH264AggregateMode" Default: 0, "none"
//                            (0): none             - Do not aggregate NAL units
//                            (1): zero-latency     - Aggregate NAL units until a VCL unit is included
//                            (2): max-stap         - Aggregate all NAL units with the same timestamp (adds one frame of latency)
//   config-interval     : Send SPS and PPS Insertion Interval in seconds (sprop parameter sets will be multiplexed in the data stream when detected.) (0 = disabled, -1 = send with every IDR frame)
//                         flags: readable, writable
//                         Integer. Range: -1 - 3600 Default: 0 
//   max-ptime           : Maximum duration of the packet data in ns (-1 = unlimited up to MTU)
//                         flags: readable, writable
//                         Integer64. Range: -1 - 9223372036854775807 Default: -1 
//   min-ptime           : Minimum duration of the packet data in ns (can't go above MTU)
//                         flags: readable, writable
//                         Integer64. Range: 0 - 9223372036854775807 Default: 0 
//   mtu                 : Maximum size of one packet
//                         flags: readable, writable
//                         Unsigned Integer. Range: 28 - 4294967295 Default: 1400 
//   name                : The name of the object
//                         flags: readable, writable, 0x2000
//                         String. Default: "rtph264pay0"
//   onvif-no-rate-control: Enable ONVIF Rate-Control=no timestamping mode
//                         flags: readable, writable
//                         Boolean. Default: false
//   parent              : The parent of the object
//                         flags: readable, writable, 0x2000
//                         Object of type "GstObject"
//   perfect-rtptime     : Generate perfect RTP timestamps when possible
//                         flags: readable, writable
//                         Boolean. Default: true
//   pt                  : The payload type of the packets
//                         flags: readable, writable
//                         Unsigned Integer. Range: 0 - 127 Default: 96 
//   ptime-multiple      : Force buffers to be multiples of this duration in ns (0 disables)
//                         flags: readable, writable
//                         Integer64. Range: 0 - 9223372036854775807 Default: 0 
//   scale-rtptime       : Whether the RTP timestamp should be scaled with the rate (speed)
//                         flags: readable, writable
//                         Boolean. Default: true
//   seqnum              : The RTP sequence number of the last processed packet
//                         flags: readable
//                         Unsigned Integer. Range: 0 - 65535 Default: 0 
//   seqnum-offset       : Offset to add to all outgoing seqnum (-1 = random)
//                         flags: readable, writable
//                         Integer. Range: -1 - 65535 Default: -1 
//   source-info         : Write CSRC based on buffer meta RTP source information
//                         flags: readable, writable
//                         Boolean. Default: false
//   sprop-parameter-sets: The base64 sprop-parameter-sets to set in out caps (set to NULL to extract from stream)
//                         flags: readable, writable, deprecated
//                         String. Default: null
//   ssrc                : The SSRC of the packets (default == random)
//                         flags: readable, writable
//                         Unsigned Integer. Range: 0 - 4294967295 Default: 4294967295 
//   stats               : Various statistics
//                         flags: readable
//                         Boxed pointer of type "GstStructure"
//                                                           clock-rate: 0
//                                                         running-time: 18446744073709551615
//                                                               seqnum: 0
//                                                            timestamp: 0
//                                                                 ssrc: 0
//                                                                   pt: 96
//                                                        seqnum-offset: 0
//                                                      timestamp-offset: 0
//
//   timestamp           : The RTP timestamp of the last processed packet
//                         flags: readable
//                         Unsigned Integer. Range: 0 - 4294967295 Default: 0 
//   timestamp-offset    : Offset to add to all outgoing timestamps (default = random)
//                         flags: readable, writable
//                         Unsigned Integer. Range: 0 - 4294967295 Default: 4294967295 
//
//
///////////////////////////////////////////////////////////////////////////////////////////////////



///////////////////////////////////////////////////////////////////////////////////////////////////
// Factory Details:
//   Rank                     primary (256)
//   Long-name                Opus audio encoder
//   Klass                    Codec/Encoder/Audio
//   Description              Encodes audio in Opus format
//   Author                   Vincent Penquerc'h <vincent.penquerch@collabora.co.uk>

// Plugin Details:
//   Name                     opus
//   Description              OPUS plugin library
//   Filename                 C:\gstreamer\1.0\msvc_x86_64\lib\gstreamer-1.0\gstopus.dll
//   Version                  1.18.4
//   License                  LGPL
//   Source module            gst-plugins-base
//   Source release date      2021-03-15
//   Binary package           GStreamer Base Plug-ins source release
//   Origin URL               Unknown package origin

// GObject
//  +----GInitiallyUnowned
//        +----GstObject
//              +----GstElement
//                    +----GstAudioEncoder
//                          +----GstOpusEnc
//
// Implemented Interfaces:
//   GstPreset
//   GstTagSetter
//
// Pad Templates:
//   SINK template: 'sink'
//     Availability: Always
//     Capabilities:
//       audio/x-raw
//                  format: S16LE
//                  layout: interleaved
//                    rate: 48000
//                channels: [ 1, 8 ]
//       audio/x-raw
//                  format: S16LE
//                  layout: interleaved
//                    rate: { (int)8000, (int)12000, (int)16000, (int)24000 }
//                channels: [ 1, 8 ]
//
//   SRC template: 'src'
//     Availability: Always
//     Capabilities:
//       audio/x-opus
//
// Element has no clocking capabilities.
// Element has no URI handling capabilities.
//
// Pads:
//   SINK: 'sink'
//     Pad Template: 'sink'
//   SRC: 'src'
//     Pad Template: 'src'
//
// Element Properties:
//   audio-type          : What type of audio to optimize for
//                         flags: readable, writable
//                         Enum "GstOpusEncAudioType" Default: 2049, "generic"
//                            (2049): generic          - Generic audio
//                            (2048): voice            - Voice
//                            (2051): restricted-lowdelay - Restricted low delay
//   bandwidth           : Audio Band Width
//                         flags: readable, writable, changeable in NULL, READY, PAUSED or PLAYING state
//                         Enum "GstOpusEncBandwidth" Default: 1105, "fullband"
//                            (1101): narrowband       - Narrow band
//                            (1102): mediumband       - Medium band
//                            (1103): wideband         - Wide band
//                            (1104): superwideband    - Super wide band
//                            (1105): fullband         - Full band
//                            (-1000): auto             - Auto
//   bitrate             : Specify an encoding bit-rate (in bps).
//                         flags: readable, writable, changeable in NULL, READY, PAUSED or PLAYING state
//                         Integer. Range: 4000 - 650000 Default: 64000 
//   bitrate-type        : Bitrate type
//                         flags: readable, writable, changeable in NULL, READY, PAUSED or PLAYING state
//                         Enum "GstOpusEncBitrateType" Default: 0, "cbr"
//                            (0): cbr              - CBR
//                            (1): vbr              - VBR
//                            (2): constrained-vbr  - Constrained VBR
//   complexity          : Complexity
//                         flags: readable, writable, changeable in NULL, READY, PAUSED or PLAYING state
//                         Integer. Range: 0 - 10 Default: 10 
//   dtx                 : DTX
//                         flags: readable, writable, changeable in NULL, READY, PAUSED or PLAYING state
//                         Boolean. Default: false
//   frame-size          : The duration of an audio frame, in ms
//                         flags: readable, writable, changeable in NULL, READY, PAUSED or PLAYING state
//                         Enum "GstOpusEncFrameSize" Default: 20, "20"
//                            (2): 2.5              - 2.5
//                            (5): 5                - 5
//                            (10): 10               - 10
//                            (20): 20               - 20
//                            (40): 40               - 40
//                            (60): 60               - 60
//   hard-resync         : Perform clipping and sample flushing upon discontinuity
//                         flags: readable, writable
//                         Boolean. Default: false
//   inband-fec          : Enable forward error correction
//                         flags: readable, writable, changeable in NULL, READY, PAUSED or PLAYING state
//                         Boolean. Default: false
//   mark-granule        : Apply granule semantics to buffer metadata (implies perfect-timestamp)
//                         flags: readable
//                         Boolean. Default: false
//   max-payload-size    : Maximum payload size in bytes
//                         flags: readable, writable, changeable in NULL, READY, PAUSED or PLAYING state
//                         Unsigned Integer. Range: 2 - 4000 Default: 4000 
//   name                : The name of the object
//                         flags: readable, writable, 0x2000
//                         String. Default: "opusenc0"
//   packet-loss-percentage: Packet loss percentage
//                         flags: readable, writable, changeable in NULL, READY, PAUSED or PLAYING state
//                         Integer. Range: 0 - 100 Default: 0 
//   parent              : The parent of the object
//                         flags: readable, writable, 0x2000
//                         Object of type "GstObject"
//   perfect-timestamp   : Favour perfect timestamps over tracking upstream timestamps
//                         flags: readable, writable
//                         Boolean. Default: false
//   tolerance           : Consider discontinuity if timestamp jitter/imperfection exceeds tolerance (ns)
//                         flags: readable, writable
//                         Integer64. Range: 0 - 9223372036854775807 Default: 40000000 
///////////////////////////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////////////////////////
// Factory Details:
//   Rank                     primary (256)
//   Long-name                RTP Opus payloader
//   Klass                    Codec/Payloader/Network/RTP
//   Description              Puts Opus audio in RTP packets
//   Author                   Danilo Cesar Lemes de Paula <danilo.cesar@collabora.co.uk>

// Plugin Details:
//   Name                     rtp
//   Description              Real-time protocol plugins
//   Filename                 C:\gstreamer\1.0\msvc_x86_64\lib\gstreamer-1.0\gstrtp.dll
//   Version                  1.18.4
//   License                  LGPL
//   Source module            gst-plugins-good
//   Source release date      2021-03-15
//   Binary package           GStreamer Good Plug-ins source release
//   Origin URL               Unknown package origin

// GObject
//  +----GInitiallyUnowned
//        +----GstObject
//              +----GstElement
//                    +----GstRTPBasePayload
//                          +----GstRtpOPUSPay

// Pad Templates:
//   SINK template: 'sink'
//     Availability: Always
//     Capabilities:
//       audio/x-opus
//         channel-mapping-family: 0
  
//   SRC template: 'src'
//     Availability: Always
//     Capabilities:
//       application/x-rtp
//                   media: audio
//                 payload: [ 96, 127 ]
//              clock-rate: 48000
//         encoding-params: 2
//           encoding-name: { (string)OPUS, (string)X-GST-OPUS-DRAFT-SPITTKA-00 }

// Element has no clocking capabilities.
// Element has no URI handling capabilities.

// Pads:
//   SRC: 'src'
//     Pad Template: 'src'
//   SINK: 'sink'
//     Pad Template: 'sink'

// Element Properties:
//   max-ptime           : Maximum duration of the packet data in ns (-1 = unlimited up to MTU)
//                         flags: readable, writable
//                         Integer64. Range: -1 - 9223372036854775807 Default: -1 
//   min-ptime           : Minimum duration of the packet data in ns (can't go above MTU)
//                         flags: readable, writable
//                         Integer64. Range: 0 - 9223372036854775807 Default: 0 
//   mtu                 : Maximum size of one packet
//                         flags: readable, writable
//                         Unsigned Integer. Range: 28 - 4294967295 Default: 1400 
//   name                : The name of the object
//                         flags: readable, writable, 0x2000
//                         String. Default: "rtpopuspay0"
//   onvif-no-rate-control: Enable ONVIF Rate-Control=no timestamping mode
//                         flags: readable, writable
//                         Boolean. Default: false
//   parent              : The parent of the object
//                         flags: readable, writable, 0x2000
//                         Object of type "GstObject"
//   perfect-rtptime     : Generate perfect RTP timestamps when possible
//                         flags: readable, writable
//                         Boolean. Default: true
//   pt                  : The payload type of the packets
//                         flags: readable, writable
//                         Unsigned Integer. Range: 0 - 127 Default: 96 
//   ptime-multiple      : Force buffers to be multiples of this duration in ns (0 disables)
//                         flags: readable, writable
//                         Integer64. Range: 0 - 9223372036854775807 Default: 0 
//   scale-rtptime       : Whether the RTP timestamp should be scaled with the rate (speed)
//                         flags: readable, writable
//                         Boolean. Default: true
//   seqnum              : The RTP sequence number of the last processed packet
//                         flags: readable
//                         Unsigned Integer. Range: 0 - 65535 Default: 0 
//   seqnum-offset       : Offset to add to all outgoing seqnum (-1 = random)
//                         flags: readable, writable
//                         Integer. Range: -1 - 65535 Default: -1 
//   source-info         : Write CSRC based on buffer meta RTP source information
//                         flags: readable, writable
//                         Boolean. Default: false
//   ssrc                : The SSRC of the packets (default == random)
//                         flags: readable, writable
//                         Unsigned Integer. Range: 0 - 4294967295 Default: 4294967295 
//   stats               : Various statistics
//                         flags: readable
//                         Boxed pointer of type "GstStructure"
//                                                           clock-rate: 0
//                                                         running-time: 18446744073709551615
//                                                               seqnum: 0
//                                                            timestamp: 0
//                                                                 ssrc: 0
//                                                                   pt: 96
//                                                        seqnum-offset: 0
//                                                      timestamp-offset: 0

//   timestamp           : The RTP timestamp of the last processed packet
//                         flags: readable
//                         Unsigned Integer. Range: 0 - 4294967295 Default: 0 
//   timestamp-offset    : Offset to add to all outgoing timestamps (default = random)
//                         flags: readable, writable
//                         Unsigned Integer. Range: 0 - 4294967295 Default: 4294967295 

///////////////////////////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////////////////////////
// Factory Details:
//   Rank                     primary (256)
//   Long-name                WasapiSrc
//   Klass                    Source/Audio/Hardware
//   Description              Stream audio from an audio capture device through WASAPI
//   Author                   Nirbheek Chauhan <nirbheek@centricular.com>, Ole André Vadla Ravnås <ole.andre.ravnas@tandberg.com>

// Plugin Details:
//   Name                     wasapi
//   Description              Windows audio session API plugin
//   Filename                 C:\gstreamer\1.0\msvc_x86_64\lib\gstreamer-1.0\gstwasapi.dll
//   Version                  1.18.4
//   License                  LGPL
//   Source module            gst-plugins-bad
//   Source release date      2021-03-15
//   Binary package           GStreamer Bad Plug-ins source release
//   Origin URL               Unknown package origin

// GObject
//  +----GInitiallyUnowned
//        +----GstObject
//              +----GstElement
//                    +----GstBaseSrc
//                          +----GstPushSrc
//                                +----GstAudioBaseSrc
//                                      +----GstAudioSrc
//                                            +----GstWasapiSrc

// Pad Templates:
//   SRC template: 'src'
//     Availability: Always
//     Capabilities:
//       audio/x-raw
//                  format: { (string)F64LE, (string)F64BE, (string)F32LE, (string)F32BE, (string)S32LE, (string)S32BE, (string)U32LE, (string)U32BE, (string)S24_32LE, (string)S24_32BE, (string)U24_32LE, (string)U24_32BE, (string)S24LE, (string)S24BE, (string)U24LE, (string)U24BE, (string)S20LE, (string)S20BE, (string)U20LE, (string)U20BE, (string)S18LE, (string)S18BE, (string)U18LE, (string)U18BE, (string)S16LE, (string)S16BE, (string)U16LE, (string)U16BE, (string)S8, (string)U8 }
//                  layout: interleaved
//                    rate: [ 1, 2147483647 ]
//                channels: [ 1, 2147483647 ]

// Clocking Interaction:
//   element is supposed to provide a clock but returned NULL
// Element has no URI handling capabilities.

// Pads:
//   SRC: 'src'
//     Pad Template: 'src'
// Element Properties:
//   actual-buffer-time  : Actual configured size of audio buffer in microseconds
//                         flags: readable
//                         Integer64. Range: -1 - 9223372036854775807 Default: -1 
//   actual-latency-time : Actual configured audio latency in microseconds
//                         flags: readable
//                         Integer64. Range: -1 - 9223372036854775807 Default: -1 
//   blocksize           : Size in bytes to read per buffer (-1 = default)
//                         flags: readable, writable
//                         Unsigned Integer. Range: 0 - 4294967295 Default: 0 
//   buffer-time         : Size of audio buffer in microseconds. This is the maximum amount of data that is buffered in the device and the maximum latency that the source reports. This value might be ignored by the element if necessary; see "actual-buffer-time"
//                         flags: readable, writable
//                         Integer64. Range: 1 - 9223372036854775807 Default: 200000 
//   device              : WASAPI playback device as a GUID string
//                         flags: readable, writable
//                         String. Default: null
//   do-timestamp        : Apply current stream time to buffers
//                         flags: readable, writable
//                         Boolean. Default: false
//   exclusive           : Open the device in exclusive mode
//                         flags: readable, writable
//                         Boolean. Default: false
//   latency-time        : The minimum amount of data to read in each iteration in microseconds. This is the minimum latency that the source reports. This value might be ignored by the element if necessary; see "actual-latency-time"
//                         flags: readable, writable
//                         Integer64. Range: 1 - 9223372036854775807 Default: 10000 
//   loopback            : Open the sink device for loopback recording
//                         flags: readable, writable
//                         Boolean. Default: false
//   low-latency         : Optimize all settings for lowest latency. Always safe to enable.
//                         flags: readable, writable
//                         Boolean. Default: false
//   name                : The name of the object
//                         flags: readable, writable, 0x2000
//                         String. Default: "wasapisrc0"
//   num-buffers         : Number of buffers to output before sending EOS (-1 = unlimited)
//                         flags: readable, writable
//                         Integer. Range: -1 - 2147483647 Default: -1 
//   parent              : The parent of the object
//                         flags: readable, writable, 0x2000
//                         Object of type "GstObject"
//   provide-clock       : Provide a clock to be used as the global pipeline clock
//                         flags: readable, writable
//                         Boolean. Default: true
//   role                : Role of the device: communications, multimedia, etc
//                         flags: readable, writable, changeable only in NULL or READY state
//                         Enum "GstWasapiDeviceRole" Default: 0, "console"
//                            (0): console          - Games, system notifications, voice commands
//                            (1): multimedia       - Music, movies, recorded media
//                            (2): comms            - Voice communications
//   slave-method        : Algorithm used to match the rate of the masterclock
//                         flags: readable, writable
//                         Enum "GstAudioBaseSrcSlaveMethod" Default: 2, "skew"
//                            (0): resample         - GST_AUDIO_BASE_SRC_SLAVE_RESAMPLE
//                            (1): re-timestamp     - GST_AUDIO_BASE_SRC_SLAVE_RE_TIMESTAMP
//                            (2): skew             - GST_AUDIO_BASE_SRC_SLAVE_SKEW
//                            (3): none             - GST_AUDIO_BASE_SRC_SLAVE_NONE
//   typefind            : Run typefind before negotiating (deprecated, non-functional)
//                         flags: readable, writable, deprecated
//                         Boolean. Default: false
//   use-audioclient3    : Whether to use the Windows 10 AudioClient3 API when available
//                         flags: readable, writable
//                         Boolean. Default: false
///////////////////////////////////////////////////////////////////////////////////////////////////




static void
setup_element_factory(SessionCore* core,
                      Codec video, 
                      Codec audio)
{
    Pipeline* pipe = session_core_get_pipeline(core);
    GError* error = NULL;
    
    if (video == CODEC_H264)
    {
        if (audio == OPUS_ENC) 
        {
            // setup default nvenc encoder (nvidia encoder)
            pipe->pipeline =
                gst_parse_launch("webrtcbin bundle-policy=max-bundle name=sendrecv "
                    "dx9screencapsrc name=screencap ! "SCREEN_CAP
                    "queue ! videoconvert ! queue ! "
                    "mfh264enc name=videoencoder ! queue ! rtph264pay name=rtp ! "
                    "queue ! " RTP_CAPS_VIDEO "H264 ! sendrecv. "
                    "wasapisrc name=audiocapsrc name=audiocapsrc ! audioconvert ! audioresample ! queue ! "
                    "opusenc name=audioencoder ! rtpopuspay ! "
                    "queue ! " RTP_CAPS_OPUS "OPUS ! sendrecv. ", &error);

            // if nvenv plugin is not found, switch to mediafoundation plugin (hardware acceleration on intel chip)
            if(error != NULL)
            {
                pipe->pipeline =
                    gst_parse_launch("webrtcbin bundle-policy=max-bundle name=sendrecv "
                        "dx9screencapsrc name=screencap ! "SCREEN_CAP
                        "queue ! videoconvert ! queue ! "
                        "mfh264enc name=videoencoder ! queue ! rtph264pay name=rtp ! "
                        "queue ! " RTP_CAPS_VIDEO "H264 ! sendrecv. "
                        "wasapisrc name=audiocapsrc name=audiocapsrc ! audioconvert ! audioresample ! queue ! "
                        "opusenc name=audioencoder ! rtpopuspay ! "
                        "queue ! " RTP_CAPS_OPUS "OPUS ! sendrecv. ", &error);
            }
            pipe->audio_element[WASAPI_SOURCE_SOUND] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "audiocapsrc");
            pipe->video_element[H264_MEDIA_FOUNDATION] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "videoencoder");
            pipe->video_element[RTP_H264_PAYLOAD] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "rtp");
            pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "screencap");
            pipe->audio_element[OPUS_ENCODER] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "audioencoder");
        }
    }
    else if (video == CODEC_H265)
    {
        if (audio == OPUS_ENC)
        {
            // setup default nvenc encoder (nvidia encoder)
            pipe->pipeline =
                gst_parse_launch("webrtcbin bundle-policy=max-bundle name=sendrecv "
                    "dx9screencapsrc name=screencap ! "SCREEN_CAP
                    " ! queue ! videoconvert ! queue ! "
                    "mfh265enc name=videoencoder ! rtph265pay name=rtp ! "
                    "queue ! " RTP_CAPS_VIDEO "H265 ! sendrecv. "
                    "wasapisrc name=audiocapsrc ! audioconvert ! audioresample ! queue ! "
                    "opusenc name=audioencoder ! rtpopuspay ! "
                    "queue ! " RTP_CAPS_OPUS "OPUS ! sendrecv. ", &error);

            // if nvenv plugin is not found, switch to mediafoundation plugin (hardware acceleration on intel chip)
            if(error != NULL)
            {
                pipe->pipeline =
                    gst_parse_launch("webrtcbin bundle-policy=max-bundle name=sendrecv "
                        "dx9screencapsrc name=screencap ! "SCREEN_CAP
                        "queue ! videoconvert ! queue ! "
                        "mfh264enc name=videoencoder ! queue ! rtph264pay name=rtp ! "
                        "queue ! " RTP_CAPS_VIDEO "H264 ! sendrecv. "
                        "wasapisrc name=audiocapsrc name=audiocapsrc ! audioconvert ! audioresample ! queue ! "
                        "opusenc name=audioencoder ! rtpopuspay ! "
                        "queue ! " RTP_CAPS_OPUS "OPUS ! sendrecv. ", &error);
            }
            pipe->audio_element[WASAPI_SOURCE_SOUND] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "audiocapsrc");
            pipe->video_element[H265_MEDIA_FOUNDATION] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "videoencoder");
            pipe->video_element[RTP_H265_PAYLOAD] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "rtp");
            pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "screencap");
            pipe->audio_element[OPUS_ENCODER] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "audioencoder");
        }
    }
    else if (video == CODEC_VP9)
    {
        if (audio == OPUS_ENC)
        {
            pipe->pipeline =
                gst_parse_launch("webrtcbin bundle-policy=max-bundle name=sendrecv "
                    "dx9screencapsrc name=screencap ! "SCREEN_CAP
                    " ! queue ! videoconvert ! queue ! "
                    "vp9enc name=videoencoder ! rtpvp9pay name=rtp ! "
                    "queue ! " RTP_CAPS_VIDEO "VP9 ! sendrecv. "
                    "wasapisrc name=audiocapsrc ! audioconvert ! audioresample ! queue ! "
                    "opusenc name=audioencoder ! rtpopuspay ! "
                    "queue ! " RTP_CAPS_OPUS "OPUS ! sendrecv. ", &error);


            pipe->audio_element[WASAPI_SOURCE_SOUND] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "audiocapsrc");
            pipe->video_element[VP9_ENCODER] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "videoencoder");
            pipe->video_element[RTP_VP9_PAYLOAD] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "rtp");
            pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "screencap");
            pipe->audio_element[OPUS_ENCODER] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "audioencoder");
        }
    }
    else if (video == CODEC_VP8)
    {
        if (audio == OPUS_ENC)
        {
            pipe->pipeline =
                gst_parse_launch("webrtcbin bundle-policy=max-bundle name=sendrecv "
                    "dx9screencapsrc name=screencap ! "SCREEN_CAP
                    " ! queue ! videoconvert ! queue ! "
                    "vp8enc name=videoencoder ! rtpvp8pay name=rtp ! "
                    "queue ! " RTP_CAPS_VIDEO "VP8 ! sendrecv. "
                    "wasapisrc name=audiocapsrc ! audioconvert ! audioresample ! queue ! "
                    "opusenc name=audioencoder ! rtpopuspay ! "
                    "queue ! " RTP_CAPS_OPUS "OPUS ! sendrecv. ", &error);


            pipe->audio_element[WASAPI_SOURCE_SOUND] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "audiocapsrc");
            pipe->video_element[VP8_ENCODER] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "videoencoder");
            pipe->video_element[RTP_VP8_PAYLOAD] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "rtp");
            pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "screencap");
            pipe->audio_element[OPUS_ENCODER] = 
                gst_bin_get_by_name(GST_BIN(pipe->pipeline), "audioencoder");
        }
    }
    if (!error == NULL) 
    {
        session_core_finalize(core,PIPELINE_ERROR_EXIT,error);
    }
    pipe->webrtcbin =
        gst_bin_get_by_name(GST_BIN(pipe->pipeline), "sendrecv");

}


/// <summary>
/// handle incoming webrtc stream
/// </summary>
/// <param name="element"></param>
/// <param name="pad"></param>
/// <param name="data"></param>
void
incoming_stream(GstElement* element, GstPad* pad, gpointer data)
{
    return;
}



/// <summary>
/// connect webrtc bin to ice and sdp signal handler
/// </summary>
/// <param name="core"></param>
static void
connect_signalling_handler(SessionCore* core)
{
    Pipeline* pipe = session_core_get_pipeline(core);
    SignallingHub* hub = session_core_get_signalling_hub(core);

    g_main_context_push_thread_default(session_core_get_main_context(core));
    /* Add stun server */
    g_object_set(pipe->webrtcbin, "stun-server", 
       signalling_hub_get_stun_server(hub), NULL);

    /* This is the gstwebrtc entry point where we create the offer and so on. It
     * will be called when the pipeline goes to PLAYING. */
    g_signal_connect(pipe->webrtcbin, "on-negotiation-needed",
        G_CALLBACK(on_negotiation_needed), core);
    g_signal_connect(pipe->webrtcbin, "on-ice-candidate",
        G_CALLBACK(send_ice_candidate_message), core);
    g_signal_connect(pipe->webrtcbin, "notify::ice-gathering-state",
        G_CALLBACK(on_ice_gathering_state_notify), core);
    g_signal_connect(pipe->webrtcbin, "pad-added", 
       G_CALLBACK(incoming_stream), NULL);
    g_main_context_pop_thread_default(session_core_get_main_context(core));
}






/// <summary>
/// setup proerty of gst element,
/// this function should be called after pipeline factory has been done,
/// each element are assigned to an element in pipeline
/// </summary>
/// <param name="core"></param>
static void
setup_element_property(SessionCore* core)
{
    Pipeline* pipe = session_core_get_pipeline(core);
    SignallingHub* hub = session_core_get_signalling_hub(core);
    QoE* qoe = session_core_get_qoe(core);



    /*turn off screeen cursor*/
    if (pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE]) { g_object_set(pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE], "cursor", FALSE, NULL); }

    /*monitor to display*/
    if (pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE]) { g_object_set(pipe->video_element[DX9_SCREEN_CAPTURE_SOURCE], "monitor", 0, NULL);}

    /*variable bitrate mode*/
    if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "rc-mode", "cbr", NULL);}

    if (pipe->video_element[H264_MEDIA_FOUNDATION]) { g_object_set(pipe->video_element[H264_MEDIA_FOUNDATION], "rc-mode", 0, NULL);}

    if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "bitrate", 20000, NULL);}

    if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "qos", TRUE, NULL);}

    if (pipe->video_element[H264_MEDIA_FOUNDATION]) { g_object_set(pipe->video_element[H264_MEDIA_FOUNDATION], "qos", TRUE, NULL);}

    /*low latency preset*/
    if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "preset", "low-latency", NULL);}

    if (pipe->video_element[H264_MEDIA_FOUNDATION]) { g_object_set(pipe->video_element[H264_MEDIA_FOUNDATION], "low-latency", TRUE, NULL);}

    if (pipe->video_element[H264_MEDIA_FOUNDATION]) { g_object_set(pipe->video_element[H264_MEDIA_FOUNDATION], "quality-vs-speed", 10, NULL);}


    /*set b-frame numbers property*/
    if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "bframes", 0, NULL);}

    /**/
    if (pipe->video_element[NVIDIA_H264_ENCODER]) { g_object_set(pipe->video_element[NVIDIA_H264_ENCODER], "zerolatency", TRUE, NULL);}

    /*set zero latency aggregate mode*/
    if (pipe->video_element[RTP_H264_PAYLOAD]) { g_object_set(pipe->video_element[RTP_H264_PAYLOAD], "aggregate-mode", 1, NULL);}


    if (pipe->audio_element[WASAPI_SOURCE_SOUND]) { g_object_set(pipe->audio_element[WASAPI_SOURCE_SOUND], "low-latency", TRUE, NULL);}

    /*
    * Set the queue max time to 16ms (16000000ns)
    * If the pipeline is behind by more than 1s, the packets
    * will be dropped.
    * This helps buffer out latency in the audio source.
    */
    if (pipe->audio_element[RTP_RTX_QUEUE]) { g_object_set(pipe->audio_element[RTP_RTX_QUEUE], "max-size-time", 16000000, NULL);}

    /*
    * Set the other queue sizes to 0 to make it only time-based.
    */
    if (pipe->audio_element[RTP_RTX_QUEUE]) { g_object_set(pipe->audio_element[RTP_RTX_QUEUE], "max-size-packet", 0, NULL);}


}





gpointer
setup_pipeline(SessionCore* core)
{
    SignallingHub* signalling = session_core_get_signalling_hub(core);
    Pipeline* pipe = session_core_get_pipeline(core);
    QoE* qoe= session_core_get_qoe(core);


    pipe->state = PIPELINE_CREATING_ELEMENT;

    

    setup_element_factory(core, 
        qoe_get_video_codec(qoe),
        qoe_get_audio_codec(qoe));

    pipe->state = PIPELINE_CONNECT_ELEMENT_SIGNAL;
    connect_signalling_handler(core);
    
    pipe->state = PIPELINE_SETTING_UP_ELEMENT;
    setup_element_property(core);



    gst_element_change_state(pipe->pipeline, GST_STATE_READY);

    connect_data_channel_signals(core);
    pipe->state = PIPELINE_SETUP_DONE;

    start_pipeline(core);

    session_core_set_state(core, REMOTE_CONNECT_STARTED);
    signalling_hub_set_peer_call_state(signalling, PEER_CALL_DONE);
}







GstElement*
pipeline_get_webrtc_bin(Pipeline* pipe)
{
    return pipe->webrtcbin;
}

GstElement*
pipeline_get_pipline(Pipeline* pipe)
{
    return pipe->pipeline;
}

PipelineState
pipeline_get_state(Pipeline* pipe)
{
    return pipe->state;
}

void
pipeline_set_state(Pipeline* pipe,
                    PipelineState state)
{
    pipe->state = state;
}

GstElement*
pipeline_get_video_encoder(Pipeline* pipe, Codec video)
{
    if (pipe->video_element[H264_MEDIA_FOUNDATION] != NULL) 
    { return pipe->video_element[H264_MEDIA_FOUNDATION];}
    if (pipe->video_element[NVIDIA_H264_ENCODER] != NULL) 
    { return pipe->video_element[NVIDIA_H264_ENCODER];}
    if (pipe->video_element[H265_MEDIA_FOUNDATION] != NULL) 
    { return pipe->video_element[H265_MEDIA_FOUNDATION];}
    if (pipe->video_element[NVIDIA_H265_ENCODER] != NULL) 
    { return pipe->video_element[NVIDIA_H265_ENCODER];}

    if (pipe->video_element[VP9_ENCODER] != NULL) 
    { return pipe->video_element[VP9_ENCODER];}
    if (pipe->video_element[VP8_ENCODER] != NULL) 
    { return pipe->video_element[VP8_ENCODER];}    
    return NULL;
}

GstElement*
pipeline_get_audio_encoder(Pipeline* pipe, Codec audio)
{
    
    if (audio == OPUS_ENC) { return pipe->audio_element[OPUS_ENCODER];}
    else if (audio == AAC_ENC) { return pipe->audio_element[AAC_ENCODER];}
    return NULL;
}

