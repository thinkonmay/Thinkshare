
        
const config = {
    headers: { Authorization: `Bearer ${app.ClientConfig.UserToken}` }
};

/**
 * fetch currrent SlaveDevice available for new session
 * @returns array contain slaveDeviceInformation
 * 
 *  example: 
 *  return: 
 * [{
 *     "CPU": "R7 2700",
 *     "GPU": "RTX 3090",
 *     "RAMcapacity": 16000 (MB),
 *     "OS": "Window 10.6.1",
 *     "ID": 123345
 * }]
 */
function 
fetchSlave()
{
    var bodyParameters = {

    }

    var SlaveArray;

    axios.get(ClientConfig.HostUrl+ClientConfig.fetchSlaveRoute,
        bodyParameters,
        config).then((respond)=>{
            SlaveArray = JSON.parse(respond);
        }, (error) =>{
            console.log(error);
        })

    return SlaveArray;
}

/**
 * fetch current session that user are in
 * @returns array contain SlaveDeviceInformation with 
 * additional serviceState attribute and SessionClientID
 * ex:
 * 
 * return: 
 * [{
 *   "CPU": "R7 2700",
 *    "GPU": "RTX 3090",
 *    "RAMcapacity": 16000 (MB),
 *    "OS": "Window 10.6.1",
 *    "ID": 123345,
 *
 *    "SessionClientID": 23645123,
 *
 *    "serviceState":  | "ON_SESSION" (one in two left string)
 *                     | "OFF_REMOTE"
 * }]
 */




function 
fetchSession()
{
    var bodyParameters = {
        
    };

    var SessionArray;

    axios.get(ClientConfig.HostUrl+ClientConfig.fetchSessionRoute,
        bodyParameters,
        config).then((respond)=>{
            SessionArray = JSON.parse(respond);
        }, (error) =>{
            console.log(error);
        })

    return SessionArray;
}


