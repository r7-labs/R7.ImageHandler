@echo off
setlocal
pushd

set Z7="C:\Program Files\7-Zip\7z.exe"
set PACKAGE=tmp_Package

cd ..

rmdir /Q /S %PACKAGE%
mkdir %PACKAGE%\bin

cd R7.ImageHandler

xcopy /Y *.ashx ..\%PACKAGE%\
xcopy /Y *.dnn ..\%PACKAGE%\
xcopy /Y *.txt ..\%PACKAGE%\

cd ..
xcopy /Y ..\..\bin\R7.ImageHandler*.dll %PACKAGE%\bin\
xcopy /Y ..\..\bin\zxing*.dll %PACKAGE%\bin\

cd %PACKAGE%
%Z7% a ..\R7.ImageHandler-%1-Install.zip *

cd ..
rmdir /Q /S %PACKAGE%

popd
endlocal
