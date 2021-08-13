cd backbone
docker-compose pull
docker-compose up
cd ..

./build-image.bat

cd host
docker-compose pull
docker-compose up
cd ..

./build-Slave.bat
