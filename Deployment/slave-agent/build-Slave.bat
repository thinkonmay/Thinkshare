

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
