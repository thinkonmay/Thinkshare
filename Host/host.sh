sudo docker kill $(sudo docker ps -a | grep -v "aef823ad88f1" | awk 'NR>1 {print $1}')
sudo docker rm $(sudo docker ps -a | grep -v "aef823ad88f1" | awk 'NR>1 {print $1}')

sudo docker pull 192.168.1.6:5000/signalling:signalling_v1.0.0
sudo docker pull 192.168.1.6:5000/slavemanager:slavemanager_v1.0.0


sudo docker run -d -p 80:80 192.168.1.6:5000/signalling:signalling_v1.0.0
sudo docker run -d -p 81:80 192.168.1.6:5000/slavemanager:slavemanager_v1.0.0