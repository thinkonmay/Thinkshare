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




        /**
         * 
         */
        fetchSlave(){
            app.SlaveArray = fetchSlave();
        },
        fetchSession(){
            app.SessionArray = fetchSession();
        },


        
        updateSlaveState(slaveID, state){
            for (var i in app.SessionArray) {
              if (app.SessionArray[i].ID == slaveID) {
                 app.SessionArray[i].serviceState = state;
                 break; //Stop this loop, we found it!
              }
            }
        },
        getSlaveState(slaveID){
            for(var i in app.SessionArray) {
                if(app.SessionArray[i].ID == slaveID){
                    return app.SessionArray[i].serviceState;
                }
            }
        },
        onNewSlave(deviceInfor){
            app.SlaveArray.push(deviceInfor);
        },
        onObtainedSlave(slaveID){
            return app.SlaveArray.filter(function(value,index,arr){
                return value.ID === slaveID;
            });            
        },
        onNewSession(slaveID){
            this.onObtainedSlave(slaveID);
            this.fetchSession();
        },
        onTerminatedSession(){
            this.fetchSession();
        },
        getClientSessionID(SlaveID){
            for( var item in this.SessionArray){
                if(this.SessionArray[item].ID === SlaveID){
                    return this.SessionArray[item].SessionClientID;
                }
            }
        },





        /**
         * initialize session with slave 
         * @param {int} SlaveID 
         */
        sessionInitialize(SlaveID){
            var ClientRequest = {
                "SlaveID": SlaveID,
                "QoEMode": app.QoeMode,
                "VideoCodec": app.VideoCodec,
                "AudioCodec": app.AudioCodec
            }
            InitializeSession(ClientRequest);
            this.onNewSession(SlaveID);
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