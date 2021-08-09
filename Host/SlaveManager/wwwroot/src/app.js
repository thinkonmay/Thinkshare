var ScaleLoader = VueSpinner.ScaleLoader;
var serverStatus = null;
var app = new Vue({

    el: '#app',

    components: {
        ScaleLoader
    },

    data() 
    {
        return {

            /**
             * Client configuration
             */


            hostUrl: HostUrl,
            ClientID: ClientID,
            

            showStart: false,
            showDrawer: false,

            logEntries: [],
            debugEntries: [],

            status: 'connecting',
            loadingText: '',

            audioEnabled: ClientConfig.AudioEnable,

            /**
             * list contain window resolution [width,height]
             */
            windowResolution: null,

            connectionStatType: "unknown",
            connectionAudioCodecName: "unknown",
            connectionVideoCodecName: "unknown",
            connectionResolution: "",

            turnSwitch: (window.localStorage.getItem("turnSwitch") === "true"),


            /*parameter serve for session initialization */
            Clientoffer: clientSession.Clientoffer,
            SessionClientID: clientSession.SessionClientID,
            SignallingUrl: clientSession.SignallingUrl,
            StunServer: clientSession.StunServer,

            /*default value from client session fetch from server*/
            QoEMode: clientSession.QoE.QoEMode,
            AudioCodec: clientSession.QoE.AudioCodec,
            VideoCodec: clientSession.QoE.VideoCodec,

            /**
             * default Value of QoE metric, fetch from server
             */
            defaultScreenWidth: clientSession.QoE.ScreenWidth,
            defaultScreenHeight: clientSession.QoE.ScreenHeight,
            defaultFrameRate: clientSession.QoE.Framerate,
            defaultBitrate: clientSession.QoE.Bitrate,



            /*Metric serve for adaptive bitrate algorithm */
            currentTime: 0,
            adaptiveAudioBitrate: clientSession.QoE.Bitrate,
            adaptiveVideoBitrate: clientSession.QoE.Bitrate,
            adaptiveFramerate: clientSession.QoE.Framerate,
            connectionVideoDecoder: "unknown",

            windowsWidth:0,
            windowsHeight:0,

            adaptivePacketsLost: 0,
            adaptiveAudioLatency: 0,
            adaptiveVideoLatency: 0,
            adaptiveTotalBandwidth: clientSession.QoE.Bitrate,

            
        };
    },

    methods: 
    {
        enterFullscreen() {
            // Request full screen mode.
            app.logEntries.push(applyTimestamp("[VIDEO] [Switch to full screen mode]"));
            videoElement.parentElement.requestFullscreen();
        },
        playVideo() {
            app.logEntries.push(applyTimestamp("[VIDEO] [Start remote control]"));
            webrtc.playVideo();
            this.showStart = false;
        },
        connectServer(){
            app.logEntries.push("[SIGNALLING] [Connecting to server]");
            signalling.connect();
        },
        getServerStatus(){
            serverStatus ? app.logEntries.push(serverStatus): app.logEntries.push("Unconnected server");
        },
        resetStream(){
            /**
             * reset webrtc connection
             */
            webrtc.input.detach();
            webrtc._close();
        },
        addSlaveID()
        {            
            var value = document.getElementsByClassName("huyhoang")[index].value;
            console.log(value);

            const data = {
                id: value 
            }

            axios.post('/Admin/AddSlave', data)
            .then(function (response) {
              console.log(response);
            })
            .catch(function (error) {
              console.log(error);
            });
        },
        showDrawer(newValue){
            if(newValue) {
                webrtc.input.detach();
            }
            else{
                webrtc.input.attach();
            }
        
        }
    },

    watch: {
        videoBitRate(newValue) {
            window.localStorage.setItem("videoBitRate", newValue.toString());
        },
        videoFramerate(newValue) {
            app.logEntries.push("video frame rate changed to " + newValue);
            window.localStorage.setItem("videoFramerate", newValue.toString());
        },
        audioEnabled(newValue, oldValue) {
            app.logEntries.push("audio enabled changed from " + oldValue + " to " + newValue);
        },
        audioBitRate(newValue) {
            webrtc.sendDataChannelMessage('ab,' + newValue);
            window.localStorage.setItem("audioBitRate", newValue.toString());
        },
        debug(newValue) {
            if(ClientConfig.devMode === false){return;}

            window.localStorage.setItem("debug", newValue.toString());
            // Reload the page to force read of stored value on first load.
            setTimeout(() => {
                document.location.reload();
            }, 700);
        },
    },
});


var videoElement = document.getElementById("stream");

var signalling = new Signalling(new URL(app.SignallingUrl), app.SessionClientID, app.Clientoffer);

var webrtc = new WebRTC(signalling, videoElement, ClientConfig.RTPpeerconfig);

-

signalling.connect();


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






