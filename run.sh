cp ./Host/SharedHost/setting/dev/appsettings.json ./Host/Authenticator        
cp ./Host/SharedHost/setting/dev/appsettings.json ./Host/Conductor            
cp ./Host/SharedHost/setting/dev/appsettings.json ./Host/MetricCollector      
cp ./Host/SharedHost/setting/dev/appsettings.json ./Host/Signalling           
cp ./Host/SharedHost/setting/dev/appsettings.json ./Host/SystemHub            
cp ./Host/SharedHost/setting/dev/appsettings.json ./Host/WorkerManager        

dotnet run ./Host/Authenticator        --urls=http://localhost:5030
dotnet run ./Host/Conductor            --urls=http://localhost:5020 
dotnet run ./Host/MetricCollector      --urls=http://localhost:5040 
dotnet run ./Host/Signalling           --urls=http://localhost:5010 
dotnet run ./Host/SystemHub            --urls=http://localhost:5050 
dotnet run ./Host/WorkerManager        --urls=http://localhost:5000 