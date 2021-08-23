function InitializeSession(ClientRequest)
{
    var bodyParameters = {
        "SlaveId": ClientRequest.SlaveId,
        "ScreenWidth": window.screen.width * window.devicePixelRatio,
        "ScreenHeight": window.screen.height * window.devicePixelRatio,
        "QoEMode": ClientRequest.QoEMode,
        "VideoCodec": ClientRequest.VideoCodec,
        "AudioCodec": ClientRequest.AudioCodec

    }
    axios.get(ClientConfig.HostUrl + "/Session/Initialize",
        bodyParameters,
        {headers :
        {
            "Authorization": "Bearer " + ClientConfig.UserToken}
        }).then((response) => {app.startRemotePage(response.body);}
        ).catch(app.setError("fail to initialize"));
}