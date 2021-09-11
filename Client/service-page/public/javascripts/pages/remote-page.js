import * as API from "../util/api.js"

const fetch = require("node-fetch")
var data = await API.initializeSession();

const sessionSlave = data.json();

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
            SessionClientID: sessionSlave.SessionClientID, 
            ClientID: sessionSlave.ClientID,

            showStart: false,
            showDrawer: false,

            loadingText: '',

            connectionVideoDecoder: "unknown",
            connectionStatType: "unknown",
            connectionAudioCodecName: "unknown",
            connectionVideoCodecName: "unknown",
            connectionResolution: "",

            /*parameter serve for session initialization */
            Clientoffer: sessionSlave.ClientOffer,
            SignallingUrl: sessionSlave.Html.Raw(SignallingUrl),

            /*default value from client session fetch from server*/
            QoEMode: sessionSlave.QoEMode,
            AudioCodec: sessionSlave.AudioCodec,
            VideoCodec: sessionSlave.VideoCodec,

            /**
            * default Value of QoE metric, fetch from server
            */
            Screen:
            {
                /*
                * slave screen (determined in session initialize step)
                */
                "SlaveWidth":sessionSlave.ScreenWidth,
                "SlaveHeight": sessionSlave.ScreenHeight,

                /*
                * frame resolution used to transport to client
                */
                "StreamWidth": 0,
                "StreamHeight": 0,


                /*
                * client resolution display on client screen
                */
                "ClientWidth": 0,
                "ClientHeight": 0,

                "fraction": 0
            },

            /**
            * variable used to convert client and slave mouse position
            */
            Mouse: {
                /**
                 * relation between frame size and actual window size
                 * (used to determine relation between client mouse and its position on slave screen)
                 */
                "mouseMultiX": 0,
                "mouseMultiY": 0,

                /**
                 * 
                 */
                "mouseOffsetX": 0,
                "mouseOffsetY": 0,

                /**
                *
                */
                "centerOffsetX": 0,
                "centerOffsetY": 0,

                /*
                *
                */
                "scrollX": 0,
                "scrollY": 0,

                /*
                *
                */
                "frameW":0,
                "frameH":0,
            },

            /*
            * Metric serve for adaptive streaming algorithm
            */
            adaptive:
            {
                "currentTime": 0,
                "AudioBitrate": 0,
                "VideoBitrate": 0,
                "Framerate": 0,


                "PacketsLost": 0,
                "AudioLatency": 0,
                "VideoLatency": 0,
                "TotalBandwidth":  0,
            },

            /**
             * RTP config, use to establish webrtc connection
             */
            RTPconfig:   
            {"iceServers":    
                [
                    {
                        "urls": [sessionSlave.Html.Raw(StunServer)]
                    },
                    {
                        "urls": ["stun:stun.l.google.com:19302"] 
                    }
                ],
                "bundle-policy":"max-compat"
            },


            /**/
            Websocket: null,
            Webrtc: null,
            VideoElement: null,
            ControlDC: null,
            HidDC: null,

            local_stream_promise:null,

            /**
             * signalling state, use to track state error
            */
            signalling_state: null,
            clipboardStatus: true,
            adaptiveScreenSize: false, ///on testing feature

            enableAudio: true,
            
            EventListeners: [],
            logEntries: [],
            debugEntries: []
        };
    },

    methods: 
    {
        ///enter full screen mode, all functional keywill be activated
        enterFullscreen() {
            // Request full screen mode.
            app.VideoElement.parentElement.requestFullscreen();
        },
        //connect to server method, this method wil be invoked automatically in publish mode 
        connectServer(){
            app.VideoElement =  document.getElementById("stream");
            app.setDebug("Connecting to server");
            SignallingConnect();
        },
        ///show debug key, only functional in debug mode
        showDrawer(newValue){
            if(newValue) {
                DetachEvent();
            }
            else{
                AttachEvent();
            }        
        },
        onClipboard(Data)
        {
            if (app.clipboardStatus === 'enabled') 
            {
                navigator.clipboard.writeText(content).catch(err =>  {
                        app.setDebug('Could not copy text to clipboard: ' + err);
                    }
                );
            }
        },
        fileTransfer()
        {
            
        },
        ///report qoe reset to slave, after that, slave device will be restarted
        QoeReset()
        {
            var MESSAGE = 
            {
                "Opcode": Opcode.RESET_QOE,
                "From": Module.CLIENT_MODULE,
                "To": Module.AGENT_MODULE,
                "Data": {
                    "ScreenHeight": app.ScreenHeight,
                    "ScreenWidth": app.ScreenWidth,
                    "QoEMode": app.QoEMode,
                    "VideoCodec": app.VideoCodec,
                    "AudioCodec":app.AudioCodec          
                }
            }
            sendControlDC(
                Opcode.RESET_QOE,
                Module.AGENT_MODULE,
                JSON.stringify(MESSAGE)
            );
        },
        ///debug entries for monitoring potential error
        setDebug(message)
        {
            console.log(message);
            app.debugEntries.push(applyTimestamp(message));
        },
        ///report errror in debug mode
        setError(message)
        {
            console.log(message);
            app.debugEntries.push(applyTimestamp(message));
        },
        ///method 
        setStatus(message)
        {
            console.log(message);
            app.logEntries.push(applyTimestamp(message));
        }
    },

    watch: {
        showDrawer(newValue) {
            // Detach inputs when menu is shown.
            if (newValue === true) {
                webrtc.input.detach();
            } else {
                webrtc.input.attach();
            }
        },
        audioEnabled(newValue, oldValue) {
            if(newValue === true)
            {
                                
            }
        },
        signalling_state(newValue) {
            switch (newValue) {
                case "connected":
                    app.setStatus("Connection complete");
                    break;

                case "disconnected":
                    app.setError("Peer connection disconnected");
                    app.VideoElement.load();
                    break;

                case "failed":
                    app.setError("Peer connection failed");
                    app.VideoElement.load();
                    break;
                default:
            }
        }
    },
});