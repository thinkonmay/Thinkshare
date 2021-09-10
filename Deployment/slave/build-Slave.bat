
call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvarsall.bat" x86_amd64
cd ..\..\Slave\build-cmake\agent\source
msbuild 
cd ../../session-core/source
msbuild 
cd ..\..\..\..