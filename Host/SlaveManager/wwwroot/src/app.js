// Function to add timestamp to logs.
var applyTimestamp = (msg) => {
    var now = new Date();
    var ts = now.getHours() + ":" + now.getMinutes() + ":" + now.getSeconds();
    return "[" + ts + "]" + " " + msg;
}

signalling.onstatus = (message) => {
     app.logEntries.push(applyTimestamp("[signalling]" + message));
};

webrtc.onstatus = (message) => {
     app.logEntries.push(applyTimestamp("[webrtc] " + message))
};


signalling.ondebug = (message) => {
     app.debugEntries.push(applyTimestamp("[signalling] " + message));
};

webrtc.ondebug = (message) => { 
    app.debugEntries.push(applyTimestamp("[webrtc] " + message)); 
};






//STATUS
// Bind vue status to connection state.
webrtc.onconnectionstatechange = (state) => {
    app.status = state;

    if (state === "connected") 
    {
        var bytesReceivedStart = 0;
        var audiobytesReceivedStart = 0;
        var statsStart = new Date().getTime() / 1000;


        var statsLoop = () => {
            webrtc.getConnectionStats().then((stats) => {
                if (app.audioEnabled) {
                    app.adaptiveAudioLatency = parseInt(stats.audioCurrentDelayMs);


                } else {
                    stats.audiobytesReceived = 0;
                }
                
                // Compute current video bitrate in mbps
                var now = new Date().getTime() / 1000;
                /**
                 * time value of an sample
                 */
                app.currentTime = now - statsStart;


                app.connectionStatType = stats.videoLocalCandidateType;

                /**
                 * packets lost
                 */
                app.adaptivePacketsLost = parseInt(stats.videopacketsLost);

                /**
                 * video codec,ex HEVC
                 */
                app.connectionVideoCodecName = stats.videoCodecName;
                /**
                 * video decoder ex:ffmpeg
                 */
                app.connectionVideoDecoder = stats.videocodecImplementationName;

                app.connectionResolution = stats.videoFrameWidthReceived + "x" + stats.videoFrameHeightReceived;

                /**
                 * (volatile) framerate of the stream
                 */
                app.adaptiveFramerate = parseInt(stats.videoFrameRateOutput);

                /**
                 * (volatile) total bandwidth of the stream
                 */
                app.adaptiveTotalBandwidth =  parseInt(stats.videoAvailableReceiveBandwidth);

                
                /**
                 * (volatile) video latency
                 */
                app.adaptiveVideoLatency = parseInt(stats.videoCurrentDelayMs);

                 /**
                  * (volatile) video bitrate 
                  */
                app.adaptiveVideoBitrate = Math.round(parseInt((stats.videobytesReceived) - bytesReceivedStart) / (now - statsStart));
                bytesReceivedStart = parseInt(stats.videobytesReceived);


                app.adaptiveAudioBitrate = Math.round((parseInt(stats.audiobytesReceived) - audiobytesReceivedStart) / (now - statsStart));
                audiobytesReceivedStart = parseInt(stats.audiobytesReceived);
                

                statsStart = now;

                // Stats refresh loop.
                setTimeout(statsLoop, 1000);
            });
        };
        statsLoop();
    }
}




webrtc.controldatachannelconnected = () => 
{
    webrtc.input.attach();

    // Send client-side metrics over data channel every 5 seconds
    setInterval(() => {

        var message =
        {
            Opcode:HidOpcode.QOE_REPORT,
            FrameRate: app.adaptiveFramerate,

            AudioLatency: app.adaptiveAudioLatency,
            VideoLatency: app.adaptiveVideoLatency,

            AudioBitrate: app.adaptiveAudioBitrate,
            VideoBitrate: app.adaptiveVideoBitrate,

            TotalBandwidth: app.adaptiveTotalBandwidth,
            PacketsLost: app.adaptivePacketsLost
        }

        webrtc.sendControlDataChannelMessage(Opcode.QOE_REPORT,Module.CORE_MODULE,JSON.stringify(message));
    }, 1000)
}




webrtc.ondatachannelclose = () => {
    webrtc.input.detach();
}

webrtc.input.onmenuhotkey = () => {
    app.showDrawer = !app.showDrawer;
}

webrtc.input.onfullscreenhotkey = () => {
    app.enterFullscreen();
}


webrtc.input.onresizeend = () => {
    app.windowResolution = webrtc.input.getWindowResolution();
    app.logEntries.push(`Window size changed: ${app.windowResolution[0]}x${app.windowResolution[1]}`);
}

webrtc.onplayvideorequired = () => {
    app.showStart = true;
}






