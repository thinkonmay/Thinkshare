var app = new Vue({

    el: '#app',

    components: {
        ScaleLoader
    },

    data() 
    {
        return {                
            hostUrl: ClientConfig.HostUrl,
            /**
            * default Value of QoE metric, fetch from server
            */
            Screen:
            {
                "ClientWidth": document.documentElement.ClientWidth,
                "ClientHeight": document.documentElement.clientHeight,
            },

            QoeMode: null,
            VideoCodec: null,
            AudioCodec: null,

            Token: null,
            
            username: null,
            password: null,

            /**/
            ClientSocket: null,
            SlaveModel: null,

            SlaveArray: [],
            SessionArray: [],
        };
    },

    methods: 
    {
        ///enter full screen mode, all functional keywill be activated
        Login() {
            // Request full screen mode.
            app.VideoElement.parentElement.requestFullscreen();
        },
        //connect to server method, this method wil be invoked automatically in publish mode 
        Register(){
            app.VideoElement =  document.getElementById("stream");
            app.setDebug("Connecting to server");
            SignallingConnect();
        },
        ///show debug key, only functional in debug mode
        fetchSlave(){
            if(newValue) {
                DetachEvent();
            }
            else{
                AttachEvent();
            }        
        },
        fetchSession()
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
        ///terminate remote control session by sending terminate signal to session controller on host
        terminateSession()
        {
            axios.delete(app.hostUrl+'/Session/Terminate?sessionClientId='+app.SessionClientID);
        },
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
        SlaveArray(newValue) {
            // Detach inputs when menu is shown.
            if (newValue === true) {
                webrtc.input.detach();
            } else {
                webrtc.input.attach();
            }
        },
        SessionArray(newValue, oldValue) {
            if(newValue === true)
            {
                                
            }
        }
    },
});
