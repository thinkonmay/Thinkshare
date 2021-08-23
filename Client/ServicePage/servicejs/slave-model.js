
        
const config = {
    headers: { Authorization: `Bearer ${app.ClientConfig.UserToken}` }
};

/**
 * fetch currrent SlaveDevice available for new session
 * @returns array contain slaveDeviceInformation
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


