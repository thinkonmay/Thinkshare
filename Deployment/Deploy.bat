scp -r backbone ubuntu@912.168.1.6:/home/host
scp -r host ubuntu@912.168.1.6:/home/host

ssh ubuntu@192.168.1.6

./build-image.bat

ssh ubuntu@192.168.1.6

./build-Slave.bat
