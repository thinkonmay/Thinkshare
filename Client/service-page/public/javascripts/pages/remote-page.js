import * as API from "../util/api.js"
import { getCookie } from "../util/cookie.js";

var sessionClient;

API.fetch(
    getCookie("remoteUrl"),{
    method: "POST",
    headers: genHeaders(),
    body: JSON.parse(getCookie("remoteBody"))}
).then( response =>{
    if(response.statusCode === 200){
        sessionClient = JSON.parse(response.body)
    }else{
    }
})