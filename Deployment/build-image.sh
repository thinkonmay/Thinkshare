cd ..
cd Host
cd Signalling




dotnet build .


cp Dockerfile ..
cd ..

docker build -t 125.212.237.45:5000/signalling:signalling_v1.0.0 .
docker push 125.212.237.45:5000/signalling:signalling_v1.0.0

rm Dockerfile


cd  SlaveManager

rm -rf Migrations
dotnet build .

dotnet ef database drop
dotnet ef migrations add Deployment
dotnet ef database update

copy Dockerfile .. 
cd ..

docker build -t 125.212.237.45:5000/slavemanager:slavemanager_v1.0.0 .
docker push 125.212.237.45:5000/slavemanager:slavemanager_v1.0.0

rm Dockerfile




cd ..
cd Deployment
