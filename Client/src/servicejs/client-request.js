/**
 * Initialize Session 
 * 
 * slave device after session initialize return with new remote page will be move from slave queue to session queue
 * @param {int} ClientRequest 
 */
function InitializeSession(ClientRequest)
{
    var bodyParameters = {
        "SlaveId": ClientRequest.SlaveId,
        "ScreenWidth": window.screen.width * window.devicePixelRatio,
        "ScreenHeight": window.screen.height * window.devicePixelRatio,
        "QoEMode": ClientRequest.QoEMode,
        "VideoCodec": ClientRequest.VideoCodec,
        "AudioCodec": ClientRequest.AudioCodec

    }
    axios.get(app.ClientConfig.HostUrl + "/Session/Initialize",
        bodyParameters,
        {headers :
        {
            "Authorization": "Bearer " + app.ClientConfig.UserToken}
        }).then((response) => {

            app.startRemotePage(response.body);
        }
        ).catch(app.setError("fail to initialize"));
}


/**
 * terminate session using slaveID
 * 
 * Session after termination will move from session queue to slave queue
 * @param {int} slaveID 
 */
function TerminateSession(slaveID){

    var SessionClientID = app.getClientSessionID(slaveID);

    var bodyParameters ={
        "sessionClientId": SessionClientID
    }

    axios.get(app.ClientConfig.HostUrl + "/Session/Terminate",
        bodyParameters,
        {headers :
        {
            "Authorization": "Bearer " + app.ClientConfig.UserToken}
        }).catch(app.setError("fail to terminate"));
}

/**
 * disconnect remote control with given slaveID
 * 
 * state update will be done through signalR
 * @param {int} slaveID 
 */
function 
DisconnectRemote(slaveID){
    var SessionClientID = app.getClientSessionID(slaveID);

    var bodyParameters ={
        "sessionClientId": SessionClientID
    }

    axios.get(app.ClientConfig.HostUrl + "/Session/Disconnect",
        bodyParameters,
        {headers :
        {
            "Authorization": "Bearer " + app.ClientConfig.UserToken}
        }).catch(app.setError("fail to initialize"));
}

/**
 * Disconnect Remote Control with given slaveID
 * 
 * state update will be done through signalR
 * @param {int} slaveID 
 */
function 
ReconnectRemoteControl(slaveID){
    var SessionClientID = app.getClientSessionID(slaveID);

    var bodyParameters ={
        "sessionClientId": SessionClientID
    }

    axios.get(app.ClientConfig.HostUrl + "/Session/Reconnect",
        bodyParameters,
        {headers :
        {
            "Authorization": "Bearer " + app.ClientConfig.UserToken}
        }).then((response) => {app.startRemotePage(response.body);}
        ).catch(app.setError("fail to initialize"));
}