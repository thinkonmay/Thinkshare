
import * as Cookies from "./cookie.js"
import { Initialize } from "./api.js"
import { reconnectSession } from "./api.js"
import { initializeSession } from "./api.js"

const coookies_expire = 100 * 1000



export const sessionInitialize = async (SlaveID) => {
    initializeSession(parseInt(SlaveID)).then(async response => {
    if(response.status == 200){
        var json = await response.json();
        var cookie = JSON.stringify(json);

        Cookies.setCookie("sessionClient",cookie)
        getRemotePage()
    }else{

    }
    })
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