#ifndef __SESSION_H__
#define __SESSION_H__
#include <glib-2.0/glib.h>

typedef struct 
{
    gint SlaveID;

    gchar* host_url;

    gchar* signalling_url;

    SessionQoE qoe;

    gboolean client_offer;

    Hub* memory_hub;
}Session;

typedef struct 
{
    gint agent_id;
    gint core_id;
    gint loader_id;
}Hub;


typedef struct 
{
    gint screen_height;
    gint screen_width;
    gint framerate;

    gint compose_bitrate;

    Encoder* video_encoder;
    Encoder* audio_encoder;
}SessionQoE;



typedef enum
{
    CODEC_H265,
    CODEC_H264,
    CODEC_VP9,


}Encoder;

enum 
{
    
}


#endif __SESSION_H__