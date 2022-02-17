import { setSetting, getSetting } from "./api.js"
import { reconnectSession } from "./api.js"
import { initializeSession } from "./api.js"
import { isElectron } from "./checkdevice.js"
import { Codec, CoreEngine, DeviceType } from "./setting-constant.js"
import * as Utils from "./utils.js"


////////////// setting
export async function setupDevice() {
    var body = await (await getSetting()).json();
    body.device = isElectron() ? DeviceType("WINDOW_APP") : DeviceType("WEB_APP");

    if (body.device == DeviceType("WEB_APP"))
        body.engine = CoreEngine("CHROME");

    if (body.engine == CoreEngine("CHROME"))
        body.videoCodec = Codec("H264");

    await setSetting(body);
}



export async function getRemotePage (token, engine) 
{
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