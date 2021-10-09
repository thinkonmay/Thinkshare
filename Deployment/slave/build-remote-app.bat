call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvarsall.bat" x86_amd64
cd ..\..\Client\native-app\remote-app\build
cmake ..
msbuild ALL_BUILD.vcxproj