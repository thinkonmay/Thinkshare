
import * as Cookies from "./cookie.js"
import { Initialize } from "./api.js"
import { InitializeSession } from "./api.js"
import { ReconnectSession } from "./api.js"

const coookies_expire = 100 * 1000



export const sessionInitialize = (SlaveID) => {
	Cookies.setCookie("SlaveID",parseInt(SlaveID),coookies_expire)
	Cookies.setCookie("IsInitialize",true,coookies_expire)
    getRemotePage()
}

export const sessionReconnect = (SlaveID) => {
	Cookies.setCookie("SlaveID",parseInt(SlaveID),coookies_expire)
	Cookies.setCookie("IsInitialize",false,coookies_expire)
    getRemotePage()
}

const getRemotePage = () => {
    window.open(Initialize, "__blank")
}