import * as API from "../util/api.js"


const sessionClient;

API.InitializeSession().then( response =>{
    if(response.statusCode === 200){
        sessionClient = JSON.parse(response.body)
    }else{
    }
})