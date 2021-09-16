import * as API from "../util/api.js"
import { getCookie } from "../util/cookie.js";




var HostUrl = getCookie("remoteUrl");
var initializebody = getCookie("remoteBody");

if (HostUrl == null){
    window.close()
}else if (initializebody == null){
    window.close()
}

var response = await fetch(
HostUrl,
{
    method: "POST",
    headers: API.genHeaders(),
    body: initializebody
});

if(response.status ==200)
{
    var sessionClient = await response.json();
    app.SetupSession(sessionClient);
}else{
    window.close();
}






app.connectServer();