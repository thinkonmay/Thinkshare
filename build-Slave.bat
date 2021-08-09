
cd ./Slave/build
cmake -G "Visual Studio 16 2019" 
cmake ..

call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvarsall.bat" 
cd ./agent/source
msbuild 
cd ..
cd ..
cd ./session-core/source
msbuild 
cd ..
cd ..
cd ..
cd ..

cd ./Slave/bin/Debug/
agent.exe
cd ..
cd ..
cd ..
