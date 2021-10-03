#include <file-transfer-data-channel.h>
#include <file-transfer.h>
#include <file-transfer-type.h>
#include <file-transfer-webrtcbin.h>


#include <logging.h>
#include <human-interface-opcode.h>
#include <exit-code.h>
#include <key-convert.h>

#include <gst/gst.h>
#include <glib-2.0/glib.h>
#include <gst/webrtc/webrtc_fwd.h>
#include <Windows.h>
#include <general-constant.h>






#define FILE_SLICE_SIZE             100000





struct _FileTransferHub
{
    GstWebRTCDataChannel* data_channel;

#ifdef G_OS_WIN32
    HANDLE input_file;
    DWORD file_size;
    DWORD file_offset;
#endif
    gchar* file_path;

    gchar chBuf[FILE_SLICE_SIZE];
};


static FileTransferHub pool;

FileTransferHub* 
init_datachannel_pool()
{
    memset(&pool,0,sizeof(pool));
    return &pool;
}

void
webrtc_data_channel_get_file(FileTransferHub* hub,
                             gchar* file)
{
    OFSTRUCT offstruct;
    hub->input_file = OpenFile(file,&offstruct,OF_READ);
    hub->file_size = offstruct.cBytes;
}



void
send_file_segment(FileTransferHub* hub)
{   
    OVERLAPPED over_lap;
    over_lap.Offset = hub->file_offset;
    DWORD data_left = hub->file_size - hub->file_offset;

    if(data_left < 0)
        return;

    if(data_left < FILE_SLICE_SIZE)
    {
        ReadFile(hub->input_file, hub->chBuf, data_left, NULL, &over_lap);
        GBytes* byte = g_bytes_new(&(hub->chBuf), data_left);
        g_signal_emit_by_name(hub->data_channel,"send-data",byte);
    }
    if (data_left > FILE_SLICE_SIZE)
    {
        ReadFile(hub->input_file, hub->chBuf, FILE_SLICE_SIZE, NULL, &over_lap);
        GBytes* byte = g_bytes_new(&(hub->chBuf), FILE_SLICE_SIZE);
        g_signal_emit_by_name(hub->data_channel,"send-data",byte);
    }     
    else
    {
        ReadFile(hub->input_file, hub->chBuf, FILE_SLICE_SIZE, NULL, &over_lap);
        GBytes* byte = g_bytes_new(&(hub->chBuf), FILE_SLICE_SIZE);
        g_signal_emit_by_name(hub->data_channel,"send-data",byte);
    }
    memset(&(hub->chBuf),0,FILE_SLICE_SIZE);    
}


static void
start_file_transfer(FileTransferService* service)
{
    FileTransferHub* hub = file_transfer_get_transfer_hub(service);
    send_file_segment(hub);
}

static void
channel_on_open(GObject* dc,
                FileTransferService* service)
{
    start_file_transfer(service);
    return;
}

static void
channel_on_error_and_close(GObject* datachannel,
                           FileTransferService* svc)
{

}

static void
on_message_string(GObject* dc,
    gchar* string,
    FileTransferHub* core)
{
    GError* error = NULL;
    Message* message = get_json_object_from_string(string,&error);

    FileTransferOpcode opcode = json_object_get_int_member(message,"Opcode");
    gchar* data =               json_object_get_string_member(message,"Data");


}


/// <summary>
/// Connect webrtcbin to data channel, connect data channel signal to callback function
/// </summary>
/// <param name="core"></param>
/// <param name="user_data"></param>
/// <returns></returns>
gboolean
connect_data_channel_signals(FileTransferService* service)
{
    FileTransferHub* hub = file_transfer_get_transfer_hub(service);
    WebRTCbin *bin  = file_transfer_get_webrtcbin(service);
    GstElement* webrtcbin = webrtcbin_get_element(bin);

    g_signal_emit_by_name(webrtcbin, 
        "create-data-channel", "file-transfer", 
        NULL, &hub->data_channel);


    g_signal_connect(hub->data_channel,"on-open",
        G_CALLBACK(channel_on_open),service);
    g_signal_connect(hub->data_channel,"on-close",
        G_CALLBACK(channel_on_open),service);
    g_signal_connect(hub->data_channel,"on-error",
        G_CALLBACK(channel_on_open),service);
    g_signal_connect(hub->data_channel,"on-message-string",
        G_CALLBACK(on_message_string),service);
    return TRUE;
}

