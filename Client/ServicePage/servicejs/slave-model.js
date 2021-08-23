
        
const config = {
    headers: { Authorization: `Bearer ${token}` }
};


function fetchSlave()
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


function fetchSession()
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


function onNewSlave(slave)
{
    app.SlaveArray.push(slave);
}

function onNewSession(slave)
{
    app.SessionArray.push(slave);
}

function onSlaveChangeSlave(SlaveID, state)
{
    var slave = this.SessionArray.find(o=>o.Id == SlaveID);

    slave.slaveServiceState = state;
}

