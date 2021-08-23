
function connectClientHub()
{
    app.ClientSocket = new signalR.HubConnectionBuilder()
                                .withUrl(ClientConfig.HostUrl+ClientConfig.ClientHub)
                                .configureLogging(signalR.LogLevel.Information)
                                .build();

    app.ClientSocket.onclose(async () => {
        await start();
    });

    app.ClientSocket.on("ReportSlaveObtained", function (slaveID) {
        app.onObtainedSlave(slaveID);
    });

    app.ClientSocket.on("ReportNewSlaveAvailable", function (device) {
        app.onNewSlave(device);
    });

    app.ClientSocket.on("ReportSessionDisconnected", function (slaveID) {
        if(app.getSlaveState(slaveID) === "OFF_REMOTE"){
            app.updateSlaveState(slaveID,"ON_SESSION");
        }
        else{
            return;}
    });

    app.ClientSocket.on("ReportSessionDisconnected", function (slaveID) {
        if(app.getSlaveState(slaveID) === "ON_SESSION"){
            app.updateSlaveState(slaveID,"OFF_REMOTE");
        }
        else{
            return;
        }
    });

    app.start();
}



function start()
{
    try{
        app.ClientSocket.start();
        console.log("connecting to ClientHub");
    }catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
}




    
