class SlaveModel
{
    constructor()
    {
        this.SlaveArray = [];
        this.SessionArray = [];

        
        this.config = {
            headers: { Authorization: `Bearer ${token}` }
        };

        this.bodyParameters = {
        };
    };


    fetchSlave()
    {
        axios.get(ClientConfig.HostUrl+ClientConfig.fetchSlaveRoute,
            this.bodyParameters,
            this.config).then((respond)=>{
                this.SlaveArray = JSON.parse(respond);
            }, (error) =>{
                console.log(error);
            })
    }


    fetchSession()
    {
        axios.get(ClientConfig.HostUrl+ClientConfig.fetchSessionRoute,
            this.bodyParameters,
            this.config).then((respond)=>{
                this.SessionArray = JSON.parse(respond);
            }, (error) =>{
                console.log(error);
            })
    }


    onNewSlave(slave)
    {
        this.SlaveArray.push(slave);
    }

    onNewSession(slave)
    {
        this.SessionArray.push(slave);
    }

    onSlaveChangeSlave(SlaveID, state)
    {
        var slave = this.SessionArray.find(o=>o.Id == SlaveID);

        slave.slaveServiceState = state;
    }
}
