
class ClientHub
{
    constructor()
    {
        this.ClientSocket = null;
    }



    connectClientHub()
    {
        this.ClientSocket = new signalR.HubConnectionBuilder()
                                    .withUrl(ClientConfig.HostUrl+ClientConfig.ClientHub)
                                    .configureLogging(signalR.LogLevel.Information)
                                    .build();

        this.ClientSocket.onclose(async () => {
            await start();
        });

        /**
         * new slave device obtained by other user, then it block on available slave will disappear
         */
        this.ClientSocket.on("ReportSlaveObtained", function (slaveID) {
            this.onObtainedSlave(slaveID);
        });

        /**
         * new Slave device available for user to remote 
         */
        this.ClientSocket.on("ReportNewSlaveAvailable", function (device) {
            this.onNewSlave(device);
        });

        /**
         * a session of this user is switch serving state from "ON_SESSION" to "OFF_REMOTE"
         */
        this.ClientSocket.on("ReportSessionDisconnected", function (slaveID) {
            if(this.getSlaveState(slaveID) === "OFF_REMOTE"){
                this.updateSlaveState(slaveID,"ON_SESSION");
            }
            else{
                return;}
        });

        
        /**
         * a session of this user is switch serving state from "OFF_REMOTE" to "ON_SESSION"
         */
        this.ClientSocket.on("ReportSessionDisconnected", function (slaveID) {
            if(this.getSlaveState(slaveID) === "ON_SESSION"){
                this.updateSlaveState(slaveID,"OFF_REMOTE");
            }
            else{
                return;
            }
        });

        this.start();
    }



    start()
    {
        try{
            this.ClientSocket.start();
            console.log("connecting to ClientHub");
        }catch (err) {
            console.log(err);
            setTimeout(start, 5000);
        }
    }
}




        
