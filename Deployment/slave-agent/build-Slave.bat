set PATH=%PATH%;C:\gstreamer\1.0\msvc_x86_64\bin
set PATH=%PATH%;C:\Program Files\CMake\bin

cd ..\..\Slave
rmdir /Q /S build
mkdir build
cd build
cmake -G "Visual Studio 16 2019" 
cmake ..

call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvarsall.bat" x86_amd64
cd ./agent/source
msbuild 
cd ../../session-core/source
msbuild 
cd ..\..\..\..

cd ./Slave/bin/Debug/
agent.exe
cd ..\..\..
