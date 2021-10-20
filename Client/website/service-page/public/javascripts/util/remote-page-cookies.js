
import * as Cookies from "./cookie.js"
import { Initialize } from "./api.js"
import { reconnectSession } from "./api.js"
import { initializeSession } from "./api.js"

const coookies_expire = 100 * 1000

const electron = true;
const webapp = false;

export const sessionInitialize = async (SlaveID) => {
    initializeSession(parseInt(SlaveID)).then(async response => {
    if(response.status == 200){
        var json = await response.json();
        if(webapp){
            var cookie = JSON.stringify(json);
            Cookies.setCookie("sessionClient",cookie,coookies_expire)
            getRemotePage()
        }else if (electron){
            window.location.assign('thinkmay://'+
                'videocodec='+json.qoE.videoCodec+
                '.audiocodec='+json.qoE.audioCodec+
                '.sessionClient='+json.sessionClientID);
        }
    }else{

    }})
}

export const sessionReconnect = async (SlaveID) => {
    reconnectSession(parseInt(SlaveID)).then(async response => {
        if(response.status == 200){
            var json = await response.json();
            if(webapp){
                var cookie = JSON.stringify(json);
                Cookies.setCookie("sessionClient",cookie,coookies_expire)
                getRemotePage()
            }else if (electron){
                window.location.assign('thinkmay://'+
                    'videocodec='+json.qoE.videoCodec+
                    '.audiocodec='+json.qoE.audioCodec+
                    '.sessionCluuuuient='+json.sessionClientID);
            }
        }else{

            
        }})
}

const getRemotePage = () => {
    window.open(Initialize, "__blank")
}