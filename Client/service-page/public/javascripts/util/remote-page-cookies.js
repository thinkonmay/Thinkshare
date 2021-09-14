
import * as Cookies from "./cookie"
import { Initialize } from "./api"
import { InitializeSession } from "./api"
import { ReconnectSession } from "./api"

const coookies_expire = 10 * 1000



export const sessionInitialize = (SlaveID) => {
    var body = {
        slaveId: SlaveID,
        cap: JSON.parse(Cookies.getCookie("cap"))
    }
	Cookies.setCookie("remoteUrl",InitializeSession,coookies_expire)
	Cookies.setCookie("remoteBody",JSON.stringify(body),coookies_expire)
    getRemotePage(body)
}

export const sessionReconnect = (SlaveID) => {
	Cookies.setCookie("remoteUrl",ReconnectSession+"?SlaveID="+SlaveID,coookies_expire)
	Cookies.setCookie("remoteBody",' ',coookies_expire)
    getRemotePage(body)
}

const getRemotePage = () => {
    window.open(Initialize, "__blank")
}