import { setSetting, getSetting } from "./api.js"
import { reconnectSession } from "./api.js"
import { initializeSession } from "./api.js"
import { isElectron } from "./checkdevice.js"
import { Codec, CoreEngine, DeviceType } from "../pages/setting.js"
import * as Utils from "./utils.js"


////////////// setting
async function setupDevice() {
    let deviceCurrent;
    // 1: chrome, 2: gstreamer
    if (isElectron() == true) {
        deviceCurrent = DeviceType("WINDOW_APP");
    } else {
        deviceCurrent = DeviceType("WEB_APP");
    }

    var body = await (await getSetting()).json();


    // onlly 
    body.device = deviceCurrent;

    // only window app are capable of handling gstreamer
    if (deviceCurrent == DeviceType("WEB_APP") &&
        body.engine == CoreEngine("GSTREAMER")) {
        body.engine = CoreEngine("CHROME");
    }

    // only gstreamer are capable of handling h265 video
    if (body.engine == CoreEngine("CHROME") &&
        body.videoCodec == Codec("H265")) {
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

                await check_remote_condition(SlaveID, token.token, _body.engine);
            })
        } else { }
    })
}

export const sessionReconnect = async (SlaveID) => {
    await setupDevice();

    reconnectSession(parseInt(SlaveID)).then(async response => {
        if (response.status == 200) {
            var token = await response.json();
            getSetting().then(async _data => {
                let _body = await _data.json();

                await check_remote_condition(SlaveID, token.token, _body.engine);
            })
        } else { }
    })
}

var session_queue = [];
export async function check_remote_condition(workerID, token, engine) {
    var item = session_queue.find(x => x.id == workerID);
    if (item == undefined) {
        session_queue.push({ id: workerID, token: token, engine: engine });
    } else {
        if (token == null && item.token != null) {
            getRemotePage(item.token, item.engine);
            for (var i = 0; i < session_queue.length; i++) {
                if (session_queue[i].id === workerID) {
                    session_queue.splice(i, 1);
                }
            }
        } else if (token != null && item.token == null) {
            getRemotePage(token, engine);
            for (var i = 0; i < session_queue.length; i++) {
                if (session_queue[i].id === workerID) {
                    session_queue.splice(i, 1);
                }
            }
        }
    }
    Utils.newSwal.fire({
        title: "Successfully!",
        text: "Connect to device successfully",
        icon: "success",
        showConfirmButton: false,
        timer: 1500
    })
}


export const getRemotePage = async (token, engine) => {
    if (engine == CoreEngine('GSTREAMER')) {
        window.location.assign(`thinkmay://token=${token}/`);
    } else {
        var remote = await ((await fetch('REMOTE.js')).text())

        var width = window.innerWidth * 0.66;
        // define the height in
        var height = width * window.innerHeight / window.innerWidth;
        // Ratio the hight to the width as the user screen ratio
        window.open(`https://${remote}/Remote?token=${token}`, 'newwindow', 'width=' + width + ', height=' + height + ', top=' + ((window.innerHeight - height) / 2) + ', left=' + ((window.innerWidth - width) / 2));
    }
}