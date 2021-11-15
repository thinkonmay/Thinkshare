import * as Cookies from "./cookie.js"
import * as Setting from "./setting.js"
import { Initialize, getInfor, setInfor } from "./api.js"
import { reconnectSession } from "./api.js"
import { initializeSession } from "./api.js"
import { isElectron } from "./checkdevice.js"

const coookies_expire = 100 * 1000


////////////// setting
export const sessionInitialize = async (SlaveID) => {
    let deviceCurrent;
    let dbDevice;
    // 0: chrome, 1: gstreamer
    if(isElectron() == true){
        deviceCurrent = "gstreamer";
    } else {
        deviceCurrent = "chrome";
    }
    getInfor().then(async data => {
        let body = await data.json();
        dbDevice = body.defaultSetting['device']
        if(deviceCurrent == 'chrome' && dbDevice == 1){
            dbDevice = 0;
            let body = {};
            body.defaultSetting_device = dbDevice;
            setInfor(body)
        }
    })
    initializeSession(parseInt(SlaveID)).then(async response => {
    if(response.status == 200){
        var json = await response.json();
        var platform = deviceCurrent
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
            var platform = deviceCurrent
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