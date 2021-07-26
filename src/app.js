/**
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

 
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
            videoBitRate: (parseInt(window.localStorage.getItem("videoBitRate")) || 2000),
            videoBitRateOptions: [
                { text: '500 kb/s', value: 500 },
                { text: '1 mbps', value: 1000 },
                { text: '2 mbps', value: 2000 },
                { text: '3 mbps', value: 3000 },
                { text: '4 mbps', value: 4000 },
                { text: '8 mbps', value: 8000 },
                { text: '20 mbps', value: 20000 },
                { text: '100 mbps', value: 100000 },
                { text: '150 mbps', value: 150000 },
                { text: '200 mbps', value: 200000 },
            ],
            videoFramerate: (parseInt(window.localStorage.getItem("videoFramerate")) || 30),
            videoFramerateOptions: [
                { text: '15 fps', value: 15 },
                { text: '30 fps', value: 30 },
                { text: '60 fps', value: 60 },
                { text: '100 fps', value: 100 },
            ],
            audioBitRate: (parseInt(window.localStorage.getItem("audioBitRate")) || 32000),
            audioBitRateOptions: [
                { text: '32 kb/s', value: 32000 },
                { text: '64 kb/s', value: 64000 },
                { text: '128 kb/s', value: 128000 },
                { text: '256 kb/s', value: 256000 },
                { text: '320 kb/s', value: 320000 },
            ],
            // session = JSON.parse(
            // {
            //     "sessionClientID": 456,
            //     "SignallingUrl": "wss://localhost:44345/app.session",
            //     "StunServer": "https://stun.l.google.com:19302",
            //     "ClientOffer": false,
                
            //     "QoE":
            //     {
            //         "ScreenWidth": 2560,
            //         "ScreenHeight": 1440,
            //         "Framerate": 60,
            //         "Bitrate": 1000,
            //         "AudioCodec": 1,
            //         "VideoCodec": 3,
            //         "QoEMode": 0,
            //     }
            // }),

            showStart: false,
            showDrawer: false,
            logEntries: [],
            debugEntries: [],
            status: 'connecting',
            loadingText: '',
            audioEnabled: null,
            windowResolution: "",
            connectionStatType: "unknown",
            connectionLatency: 0,
            connectionVideoLatency: 0,
            connectionAudioLatency: 0,
            connectionAudioCodecName: "unknown",
            connectionAudioBitrate: 0,
            connectionPacketsReceived: 0,
            connectionPacketsLost: 0,
            connectionCodec: "unknown",
            connectionVideoDecoder: "unknown",
            connectionResolution: "",
            connectionFrameRate: 0,
            connectionVideoBitrate: 0,
            connectionAvailableBandwidth: 0,
            gpuLoad: 0,	
            gpuMemoryTotal: 0,	
            gpuMemoryUsed: 0,
            debug: (window.localStorage.getItem("debug") === "true"),
            turnSwitch: (window.localStorage.getItem("turnSwitch") === "true"),

            Clientoffer: false,
            ClientID: 456,
            SignallingUrl =  "wss://localhost:44345/Session",
            StunServer = "https://stun.l.google.com:19302",

            // QoEMode = app.session.QoE.QoEMode,
            // AudioCodec = app.session.QoE.AudioCodec,
            // VideoCodec = app.session.QoE.VideoCodec,
            // ScreenWidth = 2560,
            // ScreenHeight = 1440,
            // FrameRate = app.session.QoE.Framerate,



        };
    },

    methods: 
    {
        enterFullscreen() {
            // Request full screen mode.
            console.log("Bo may dang fullscreen day");
            webrtc.element.parentElement.requestFullscreen();
        },
        playVideo() {
            webrtc.playVideo();
            this.showStart = false;
        },
        offerClient(){
            //set client offer is my browser
            console.log("Bo may dang offer day");
            clientoffer = true;
        },
        connectServer(){
            console.log("Dang connect server, doi ty")
            webrtc.connect();
        },
        getServerStatus(){
            serverStatus ? console.log(serverStatus): console.log("Unconnected server")
        }
    },

    watch: {
        videoBitRate(newValue) {
            window.localStorage.setItem("videoBitRate", newValue.toString());
        },
        videoFramerate(newValue) {
            console.log("video frame rate changed to " + newValue);
            window.localStorage.setItem("videoFramerate", newValue.toString());
        },
        audioEnabled(newValue, oldValue) {
            console.log("audio enabled changed from " + oldValue + " to " + newValue);
            if (oldValue !== null && newValue !== oldValue) webrtc.sendDataChannelMessage('_arg_audio,' + newValue);
        },
        audioBitRate(newValue) {
            webrtc.sendDataChannelMessage('ab,' + newValue);
            window.localStorage.setItem("audioBitRate", newValue.toString());
        },
        debug(newValue) {
            window.localStorage.setItem("debug", newValue.toString());
            // Reload the page to force read of stored value on first load.
            setTimeout(() => {
                document.location.reload();
            }, 700);
        },
    },
});

var videoElement = document.getElementById("stream");

// WebRTC entrypoint, connect to the signalling server
/*global WebRTCDemoSignalling, WebRTCDemo*/
var signalling = new Signalling(new URL(app.SignallingUrl), app.ClientID, app.Clientoffer);
var webrtc = new WebRTCDemo(new URL(app.StunServer),signalling, videoElement);


// Function to add timestamp to logs.
var applyTimestamp = (msg) => {
    var now = new Date();
    var ts = now.getHours() + ":" + now.getMinutes() + ":" + now.getSeconds();
    return "[" + ts + "]" + " " + msg;
}



if (app.debug) 
{
    signalling.ondebug = (message) => { app.debugEntries.push(applyTimestamp("[signalling] " + message));};
    webrtc.ondebug = (message) => { app.debugEntries.push(applyTimestamp("[webrtc] " + message)) };
}

//STATUS
// Bind vue status to connection state.
webrtc.onconnectionstatechange = (state) => {
    app.status = state;

    if (state === "connected") 
    {
        // Start watching stats.
        var bytesReceivedStart = 0;
        var audiobytesReceivedStart = 0;
        var statsStart = new Date().getTime() / 1000;
        var statsLoop = () => {
            webrtc.getConnectionStats().then((stats) => {
                //app.audioEnabled = (app.state === 'connected' && stats.audioCodecName) ? true : false;
                if (app.audioEnabled) {
                    app.connectionAudioLatency = parseInt(stats.audioCurrentDelayMs);
                    app.connectionAudioCodecName = stats.audioCodecName;
                    app.connectionLatency = Math.max(app.connectionAudioLatency, app.connectionVideoLatency);
                } else {
                    stats.audiobytesReceived = 0;
                    app.connectionLatency = app.connectionVideoLatency;
                }
                app.connectionStatType = stats.videoLocalCandidateType;
                app.connectionVideoLatency = parseInt(stats.videoCurrentDelayMs);
                app.connectionPacketsReceived = parseInt(stats.videopacketsReceived);
                app.connectionPacketsLost = parseInt(stats.videopacketsLost);
                app.connectionCodec = stats.videoCodecName;
                app.connectionVideoDecoder = stats.videocodecImplementationName;
                app.connectionResolution = stats.videoFrameWidthReceived + "x" + stats.videoFrameHeightReceived;
                app.connectionFrameRate = stats.videoFrameRateOutput;
                app.connectionAvailableBandwidth = (parseInt(stats.videoAvailableReceiveBandwidth) / 1e+6).toFixed(2) + " mbps";

                // Compute current video bitrate in mbps
                var now = new Date().getTime() / 1000;
                app.connectionVideoBitrate = (((parseInt(stats.videobytesReceived) - bytesReceivedStart) / (now - statsStart)) * 8 / 1e+6).toFixed(2);
                bytesReceivedStart = parseInt(stats.videobytesReceived);

                // Compute current audio bitrate in kbps
                if (app.audioEnabled) {
                    app.connectionAudioBitrate = (((parseInt(stats.audiobytesReceived) - audiobytesReceivedStart) / (now - statsStart)) * 8 / 1e+3).toFixed(2);
                    audiobytesReceivedStart = parseInt(stats.audiobytesReceived);
                }

                statsStart = now;

                // Stats refresh loop.
                setTimeout(statsLoop, 1000);
            });
        };
        statsLoop();
    }
}

webrtc.ondatachannelopen = () => 
{
    webrtc.input.attach();

    // Send client-side metrics over data channel every 5 seconds
    setInterval(() => {

        var message =
        {
            "Opcode":HidOpcode.FRAMERATE_REPORT,
            "FrameRate":app.connectionFrameRate,
            "Latency":app.connectionLatency,
            "AudioBitrate":app.audioBitRate || (parseInt(window.localStorage.getItem("audioBitRate")) || 64000),
            "VideoBitrate":video_bit_rate = app.videoBitRate || (parseInt(window.localStorage.getItem("videoBitRate")) || 2000)
        }

        webrtc.sendHIDDataChannelMessage(JSON.stringify(message));
    }, 5000)
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

webrtc.input.onofferclienthotkey = () => {
    app.offerClient();
}

webrtc.input.connectserverhotkey = () => {
    app.connectServer();
}

webrtc.input.ongetstatusserverhotkey = () => {
    app.getServerStatus();
}



webrtc.input.onresizeend = () => {
    app.windowResolution = webrtc.input.getWindowResolution();
    console.log(`Window size changed: ${app.windowResolution[0]}x${app.windowResolution[1]}`);
}

webrtc.onplayvideorequired = () => {
    app.showStart = true;
}
webrtc.input.on






