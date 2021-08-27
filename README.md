# personal-cloud-computing
personal cloud computing is a part of thinkmay project which aim to create a personal cloud computing platform


How to build this project:


on windows: 
in Deployment:

to build Host:

install .NET SDK and runtime



# In Deployment/backbone:

build backbone container (*include database and registry container):

docker-compose pull
docker-compose up

# In Deployment/host

build host container (*include signalling and slavemanager container):

./Deployment/build-image.bat

after building host container, all container will automatically be pushed to server registry (built in previous step)

In order to re-create host, go-to directory /home/ubuntu/src/personal-cloud-computing/Deployment/host/

then 
docker-compose pull
docker-compose up

#To build slave manager

install Visual Studio 2019

build and run agent and session-core
download and copy dependencies to C drive

copy ./Deployment/ThinkMay to C drive

add C:/gstreamer/1.0/msvc_x86_64/bin and C:/Cmake/bin to PATH

start build agent and session core:
./Deployment/slave-agent/build-Slave.bat




