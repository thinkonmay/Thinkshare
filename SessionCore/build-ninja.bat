rmdir /Q /S build-ninja
mkdir build-ninja
cd build-ninja
cmake .. -G Ninja
cd ..
ninja -C build