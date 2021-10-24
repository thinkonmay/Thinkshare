import * as Cookies from "./cookie.js"
import * as Setting from "./setting.js"
import { Initialize } from "./api.js"
import { reconnectSession } from "./api.js"
import { initializeSession } from "./api.js"

const coookies_expire = 100 * 1000

export const sessionInitialize = async (SlaveID) => {
    initializeSession(parseInt(SlaveID)).then(async response => {
    if(response.status == 200){
        var json = await response.json();
        var platform = Cookies.getCookie("platform");
        if(platform === 'chrome'){
            var cookie = JSON.stringify(json);
            Cookies.setCookie("sessionClient",cookie,coookies_expire)
            getRemotePage()
        }else if(platform === 'gstreamer'){
            window.location.assign('thinkmay://'+
                    'videocodec='+json.qoE.videoCodec+
                    '.audiocodec='+json.qoE.audioCodec+
                    '.sessionid='+json.sessionClientID);
        }
    }else{

    }})
}

export const sessionReconnect = async (SlaveID) => {
    reconnectSession(parseInt(SlaveID)).then(async response => {
        if(response.status == 200){
            var json = await response.json();
            var platform = Cookies.getCookie("platform");
            if(platform === 'chrome'){
                var cookie = JSON.stringify(json);
                Cookies.setCookie("sessionClient",cookie,coookies_expire)
                getRemotePage()
            }else if(platform === 'gstreamer'){
                window.location.assign('thinkmay://'+
                     'videocodec='+json.qoE.videoCodec+
                    '.audiocodec='+json.qoE.audioCodec+
                    '.sessionid='+json.sessionClientID);
            }
        }else{

            
        }})
}

const getRemotePage = () => {
    window.open(Initialize, "__blank")
}