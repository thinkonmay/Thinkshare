rmdir /Q /S build
mkdir build 
cd build
cmake .. -G Ninja
cmake --build --clean-first
cd ..
ninja -C build