import { setSetting, getSetting } from "./api.js"
import { reconnectSession } from "./api.js"
import { initializeSession } from "./api.js"
import { isElectron } from "./checkdevice.js"
import { Codec, CoreEngine, DeviceType } from "../pages/setting.js"



////////////// setting


async function setupDevice() {
    let deviceCurrent;
    // 1: chrome, 2: gstreamer
    if (isElectron() == true) {
        deviceCurrent = DeviceType("WINDOW_APP");
    } else {
        deviceCurrent = DeviceType("WEB_APP");
    }

    var body = await(await  getSetting()).json();


    // onlly 
    body.device = deviceCurrent;

    // only window app are capable of handling gstreamer
    if(deviceCurrent == DeviceType("WEB_APP")&&
        body.engine == CoreEngine("GSTREAMER"))
    {
        body.engine = CoreEngine("CHROME");
    }
    
    // only gstreamer are capable of handling h265 video
    if(body.engine == CoreEngine("CHROME") &&
       body.videoCodec == Codec("H265"))
    {
        body.videoCodec = Codec("H264");

    }


    await setSetting(body);
}


export const sessionInitialize = async (SlaveID) => {
    await setupDevice();

    initializeSession(parseInt(SlaveID)).then(async response => {
        if (response.status == 200) {
            var token = await response.json();
            getSetting().then(async _data => {
                let _body = await _data.json();

                if (_body.engine == CoreEngine('GSTREAMER')) {
                    window.location.assign(`thinkmay://token=${token.token}/`);
                } else {
                    getRemotePage(token.token);
                }
            })
        } else {
        }
    })
}

export const sessionReconnect = async (SlaveID) => {
    await setupDevice();

    reconnectSession(parseInt(SlaveID)).then(async response => {
        if (response.status == 200) {
            var token = await response.json();
            getSetting().then(async _data => {
                let _body = await _data.json();

                if (_body.engine == CoreEngine('GSTREAMER')) {
                    window.location.assign(`thinkmay://token=${token.token}/`);
                } else {
                    getRemotePage(token.token);
                }
            })
        } else {
        }
    })
}

const RemotePageUrl = "https://remote.thinkmay.net/Remote"

const getRemotePage = (token) => {
    window.open(RemotePageUrl+"?token="+token, "__blank");
}