

cd ..
cd Host
cd Signalling




dotnet build .


copy Dockerfile ..
cd ..

docker build -t 125.212.237.45:5000/signalling:signalling_v1.0.1 .
docker push 125.212.237.45:5000/signalling:signalling_v1.0.1

del Dockerfile


cd  SlaveManager

rmdir /S /Q Migrations
dotnet build .

dotnet ef database drop
dotnet ef migrations add Deployment
dotnet ef database update

copy Dockerfile .. 
cd ..

docker build -t 125.212.237.45:5000/slavemanager:slavemanager_v1.0.1 .
docker push 125.212.237.45:5000/slavemanager:slavemanager_v1.0.1

del Dockerfile




cd ..
cd Deployment



