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

            LoggedIn:false,


            ClientConfig: 
            {
              HostUrl: "http://125.212.237.45:81",

              
              Role:"User",
              ClientHub:"/ClientHub",
              AdminHub:"/AdminHub",
              LoginRoute:"/Account/Login",

              fetchSlaveRoute: "/fetchSlave",
              fetchSessionRoute: "/fetchSession",
              UserToken:""
            },
      
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
            var url = new URL(app.ClientConfig.HostUrl + 
                app.ClientConfig.LoginRoute);
            try{
                axios.post(url,body).then((response) => {
                    app.ClientConfig.UserToken = response.data.token;
                    app.LoggedIn = true;
                })
            }catch(error){
                app.setError(error);
            }
        },




        Register(){
            
        },









        setDebug(message)
        {
            console.log(message);
            // app.debugEntries.push(message);
        },
        ///report errror in debug mode
        setError(message)
        {
            console.log(message);
            // app.debugEntries.push(message);
        },
        ///method 
        setStatus(message)
        {
            console.log(message);
            // app.logEntries.push(message);
        }
    },

    watch: {
        LoggedIn(newValue){
            if(newValue === true){
                reload();
            }
        }
    },
});
