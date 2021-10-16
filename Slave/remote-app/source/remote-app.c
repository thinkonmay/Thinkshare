#include <remote-app-signalling.h>
#include <remote-app-remote-config.h>
#include <remote-app-pipeline.h>
#include <remote-app-data-channel.h>
#include <remote-app.h>
#include <remote-app-message.h>
#include <remote-app-type.h>

#include <exit-code.h>
#include <error-code.h>
#include <module-code.h>
#include <opcode.h>
#include <general-constant.h>


#include <glib.h>
#include <logging.h>
#include <message-form.h>
#include <module-code.h>
#include <gst/base/gstbasesink.h>


struct _RemoteApp
{
	Pipeline* pipe;

	WebRTCHub* hub;

	GMainLoop* loop;

	SignallingHub* signalling;

	QoE* qoe;

};



/// <summary>
/// setup slave session, this step include get value from json config file 
/// </summary>
/// <param name="self"></param>
static void
remote_app_setup_session(RemoteApp* self, 
						gint session_id, 
						gchar* signalling_url,
						gchar* turn, 
						gchar* audio_codec, 
						gchar* video_codec)
{
	Codec audio = 0,video = 0;

	GError* error = NULL;
	if(!g_strcmp0(audio_codec,"aac"))	
		audio = AAC_ENC;
	else if(!g_strcmp0(audio_codec,"opus"))
		audio = OPUS_ENC;


	if(!g_strcmp0(video_codec,"h264"))
		video = CODEC_H264;
	if(!g_strcmp0(video_codec,"h265"))
		video = CODEC_H265;
	if(!g_strcmp0(video_codec,"vp9"))
		video = CODEC_VP9;
	
	signalling_hub_setup(self->signalling,turn,signalling_url,session_id);
	qoe_setup(self->qoe,audio,video);


	signalling_hub_set_signalling_state(self->signalling, SIGNALLING_SERVER_READY);
	signalling_hub_set_peer_call_state(self->signalling, PEER_CALL_READY);
}


static RemoteApp core = {0};


RemoteApp*
remote_app_initialize(gint session_id,
					  gchar* signalling_url,
					  gchar* turn,
					  gchar* audio_codec,
					  gchar* video_codec)
{
	core.hub =				webrtchub_initialize();
	core.signalling =		signalling_hub_initialize(&core);

	core.qoe =				qoe_initialize();
	core.pipe =				pipeline_initialize(&core);
	core.loop =				g_main_loop_new(NULL, FALSE);
	 
	remote_app_setup_session(&core, 
							session_id, 
							signalling_url, 
							turn, 
							audio_codec, 
							video_codec);


	remote_app_setup_pipeline(&core);

	remote_app_connect_signalling_server(&core);
	g_main_loop_run(core.loop);
	return &core;	
}






void
remote_app_connect_signalling_server(RemoteApp* self)
{
	connect_to_websocket_signalling_server_async(self);
}

void
remote_app_setup_pipeline(RemoteApp* self)
{
	setup_pipeline(self);
}				


void
remote_app_send_message(RemoteApp* core, JsonObject* message)
{
	send_message(core, message);
}












void
remote_app_finalize(RemoteApp* self, 
					  gint exit_code, 
					  GError* error)
{
	g_print(error->message);
	ExitProcess(exit_code);
}









Pipeline*
remote_app_get_pipeline(RemoteApp* self)
{
	return self->pipe;
}

WebRTCHub*
remote_app_get_rtc_hub(RemoteApp* self)
{
	return self->hub;
}


QoE*
remote_app_get_qoe(RemoteApp* self)
{
	return self->qoe;
}

SignallingHub*
remote_app_get_signalling_hub(RemoteApp* core)
{
	return core->signalling;
}

GMainContext*
remote_app_get_main_context(RemoteApp* core)
{
	return g_main_loop_get_context(core->loop);
}