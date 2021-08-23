var app = new Vue({
    el: '#app',

    components: {
    },

    data() 
    {
        return {

            QoeMode: null,
            VideoCodec: null,
            AudioCodec: null,

            Token: null,
            
            username: null,
            password: null,


            ClientConfig: 
            {
              "HostUrl": "http:192.168.1.6:81",
              "ClientHub":"/ClientHub",
              "AdminHub":"/AdminHub",
              "fetchSlaveRoute": "/fetchSlave",
              "fetchSessionRoute": "/fetchSession",
              "UserToken":""
            },
      
            /**/
            ClientSocket: null,

            SlaveArray: [],
            SessionArray: [],

            debugEntries: [],
            logEntries: []
        };
    },

    methods: 
    {
        ReloadPage(html){
            document.getElementById("app").innerHTML = html;
        },
        startRemotePage(html){
            var window = Window.components("","Remote control","");
            window.document.body.innerHTML = html;
        },





        fetchSlave(){
            app.SlaveArray = fetchSlave();
        },
        fetchSession(){
            app.SessionArray = fetchSession();
        },


        sessionInitialize(SlaveID){
            var ClientRequest = {
                "SlaveID": SlaveID,
                "QoEMode": app.QoeMode,
                "VideoCodec": app.VideoCodec,
                "AudioCodec": app.AudioCodec
            }
            InitializeSession(ClientRequest);
        },



        Login() {

            var body = {
                "email":app.username,
                "password":app.password
            }
            try
            {
                axios.post(new URL(ClientConfig.HostUrl + ClientConfig.LoginRoute),body).then((response) => {
                    app.ReloadPage(response.body);
                })
            }catch(error){
                app.setError("fail to load service page");
            }

        },
        Register(){
            app.VideoElement =  document.getElementById("stream");
            app.setDebug("Connecting to server");
            SignallingConnect();
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
