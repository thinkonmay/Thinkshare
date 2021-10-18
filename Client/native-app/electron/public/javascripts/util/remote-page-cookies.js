const child_process = require('child_process');
const coookies_expire = 100 * 1000

const getRemotePage() {
    child_process.spawn();
}

const sessionInitialize = async (SlaveID) => {
    initializeSession(parseInt(SlaveID)).then(async response => {
    if(response.status == 200){
        var json = await response.json();
        var cookie = JSON.stringify(json);

        Cookies.setCookie("sessionClient",cookie,coookies_expire)
        child_process.spawn()
    }else{

    }})
}

const sessionReconnect = async (SlaveID) => {
    reconnectSession(parseInt(SlaveID)).then(async response => {
        if(response.status == 200){
            var json = await response.json();
            var cookie = JSON.stringify(json);
    
            Cookies.setCookie("sessionClient",cookie,coookies_expire)
            getRemotePage()
        }else{
            
        }})
}
