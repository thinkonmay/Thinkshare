@echo off
set VSCMD_START_DIR=.
call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvars32.bat"
lib /machine:"x86" %*
