class ClientHub
{
    constructor (hostUrl)
    {
        this.connection = new signalR.HubConnectionBuilder()
                                    .withUrl(ClientConfig.HostUrl+ClientConfig.ClientHub)
                                    .configureLogging(signalR.LogLevel.Information)
                                    .build();

        this.connection.onclose(async () => {
            await start();
        });
        
        this.connection.on()

        this.start();
    }

    start()
    {
        try{
            await this.connection.start();
            console.log("connecting to ClientHub");
        }catch (err) {
            console.log(err);
            setTimeout(start, 5000);
        }
    }

    
}