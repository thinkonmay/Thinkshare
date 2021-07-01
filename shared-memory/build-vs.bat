rmdir /Q /S build-vs
mkdir build-vs
cd build-vs
cmake .. -G "Visual Studio 16 2019"
cmake --build --clean-first
cd ..