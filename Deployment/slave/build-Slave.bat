
rmdir /Q /S "C:/ThinkMay"
mkdir "C:/ThinkMay"
xcopy "..\ThinkMay" "C:\ThinkMay" /s /e

cd ..\..\Slave
rmdir /Q /S build

mkdir C:/ThinkMay/bin
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

