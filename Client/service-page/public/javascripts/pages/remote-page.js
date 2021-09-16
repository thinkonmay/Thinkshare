import * as API from "../util/api.js"
import { getCookie } from "../util/cookie.js";

var sessionClient;

var HostUrl = getCookie("remoteUrl");

var initializebody = getCookie("remoteBody");

var response = await fetch(
    HostUrl,{
    method: "POST",
    headers: API.genHeaders(),
    body: initializebody}
)

if(response.statusCode === 200){
    sessionClient = JSON.parse(response.body)
}else{
    window.close();
}
