import * as Cookies from "./cookie.js"
import { Initialize, setSetting, setInfor, getSetting } from "./api.js"
import { reconnectSession } from "./api.js"
import { initializeSession } from "./api.js"
import { isElectron } from "./checkdevice.js"
import { CoreEngine, DeviceType } from "../pages/setting.js"

const coookies_expire = 100 * 1000


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
    if(deviceCurrent != body.device)
    {
        if(deviceCurrent == DeviceType("WEB_APP"))
        {
            if(body.engine == CoreEngine("GSTREAMER"))
            {
                body.engine = CoreEngine("WEB_APP");
            }
        }
        body.device = deviceCurrent;
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
                    Cookies.setCookie("remoteToken", token.token, coookies_expire)
                    getRemotePage()
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
                    Cookies.setCookie("remoteToken", token.token, coookies_expire)
                    getRemotePage()
                }
            })
        } else {
        }
    })
}

const getRemotePage = () => {
    window.open(Initialize, "__blank")
}