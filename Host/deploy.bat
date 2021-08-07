cd  Signalling
copy Dockerfile ..
cd ..

docker build -t 192.168.1.6:5000/signalling:signalling_v1.0.0 .
docker push 192.168.1.6:5000/signalling:signalling_v1.0.0

del Dockerfile


cd  SlaveManager
copy Dockerfile .. 
cd ..

docker build -t 192.168.1.6:5000/slavemanager:slavemanager_v1.0.0 .
docker push 192.168.1.6:5000/slavemanager:slavemanager_v1.0.0

del Dockerfile