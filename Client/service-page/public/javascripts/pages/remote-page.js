import * as API from "../util/api.js"
import { getCookie } from "../util/cookie.js";

var response;
if(getCookie("IsInitialize")){
    API.initializeSession().then((response)=>{
        
        if(response.status == 200){
            var sessionClient = response.json();
            app.SetupSession(sessionClient);
            app.connectServer();
        }else{

        }
    })
}


else{response = await API.reconnectSession(getCookie("SlaveID"))}






