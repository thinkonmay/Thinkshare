#ifndef __QOE_H__
#define __QOE_H__

typedef enum
{
    CODEC_H265,
    CODEC_H264,
    CODEC_VP8,
    CODEC_VP9,

    OPUS_ENC,
    AAC_ENC
}Codec;


typedef enum
{
    AUDIO_PIORITY,
    VIDEO_PIORITY,

    CUSTOM_BITRATE_CONTROL,
    NON_OVERSAMPLING
}QoEMode;

#endif