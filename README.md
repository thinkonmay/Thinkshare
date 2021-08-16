# personal-cloud-computing
personal cloud computing is a part of thinkmay project which aim to create a personal cloud computing platform

Project documentation for this project is provided here:
https://vinuniversity.sharepoint.com/:w:/s/Personalcloudcomputing/EZMj0WYoSPVHtA9bIpyAztgBeK0l6vMb4TMkJZEKo4itYA?e=4s5a5b




How to build this project:


on windows: 
in Deployment:

to build Host:


install .NET SDK and runtime

in Deployment/backbone:

build backbone container include postgresql database and registry container
docker-compose up

build signalling and slavemanager container
./Deployment/build-image.bat


run signalling and slavemanager container

install Visual Studio 2019

build and run agent and session-core
download and copy dependencies to C drive

copy ./Deployment/ThinkMay to C drive

add C:/gstreamer/1.0/msvc_x86_64/bin and C:/Cmake/bin to PATH

start build agent and session core:
./Deployment/slave-agent/build-Slave.bat




