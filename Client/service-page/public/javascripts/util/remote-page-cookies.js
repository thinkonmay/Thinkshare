
import * as Cookies from "./cookie.js"
import { Initialize } from "./api.js"
import { reconnectSession } from "./api.js"
import { initializeSession } from "./api.js"

const coookies_expire = 100 * 1000



export const sessionInitialize = async (SlaveID) => {
    var response = await initializeSession(parseInt(SlaveID));
    if(response.status == 200){
        Cookies.setCookie("sessionClient",response.json())
        getRemotePage()
    }else{

    }
}

export const sessionReconnect = async (SlaveID) => {
    var response = await reconnectSession(parseInt(SlaveID));
    if(response.status == 200){
        Cookies.setCookie("sessionClient",response.json())
        getRemotePage()
    }else{

    }
}

const getRemotePage = () => {
    window.open(Initialize, "__blank")
}