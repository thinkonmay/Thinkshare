import * as Cookies from "./cookie.js"
import { Initialize, setSetting, setInfor, getSetting } from "./api.js"
import { reconnectSession } from "./api.js"
import { initializeSession } from "./api.js"
import { isElectron } from "./checkdevice.js"

const coookies_expire = 100 * 1000


////////////// setting
export const sessionInitialize = async (SlaveID) => {
    let deviceCurrent;
    let dbDevice;
    // 1: chrome, 2: gstreamer
    if (isElectron() == true) {
        deviceCurrent = "gstreamer";
    } else {
        deviceCurrent = "chrome";
    }
    getSetting().then(async data => {
        let body = data.json();
        dbDevice = body.defaultSetting['device']
        if (deviceCurrent == 'chrome' && dbDevice == 2) {
            dbDevice = 1;
            let body = {};
            body.defaultSetting_device = 1;
            setInfor(body)
        }
    })
    initializeSession(parseInt(SlaveID)).then(async response => {
        if (response.status == 200) {
            var token = await response.json();
            var platform = 1;
            getSetting().then(async _data => {
                let _body = await _data.json();

                if (_body.defaultSetting['device'] == 2) {
                    platform = 'gstreamer';
                } else if (_body.defaultSetting['device'] == 1) {
                    platform = 'chrome'
                }
                console.log(platform)
                if (platform == 'chrome') {
                    Cookies.setCookie("remoteToken", token, coookies_expire)
                    getRemotePage()
                } else if (platform == 'gstreamer') {
                    window.location.assign(`thinkmay://token=${token}/`);
                }
            })
        } else {
        }
    })
}

export const sessionReconnect = async (SlaveID) => {
    reconnectSession(parseInt(SlaveID)).then(async response => {
        if (response.status == 200) {
            var token = await response.json();
            var platform = 1;
            getInfor().then(async _data => {
                let _body = await _data.json();
                if (_body.defaultSetting['device'] == 2) {
                    platform = 'gstreamer';
                } else if (_body.defaultSetting['device'] == 1) {
                    platform = 'chrome'
                }
                console.log(platform)
                if (platform == 'chrome') {
                    Cookies.setCookie("remoteToken", token, coookies_expire)
                    getRemotePage()
                } else if (platform == 'gstreamer') {
                    window.location.assign(`thinkmay://token=${token}/`);
                }
            })
        } else {
        }
    })
}

const getRemotePage = () => {
    window.open(Initialize, "__blank")
}