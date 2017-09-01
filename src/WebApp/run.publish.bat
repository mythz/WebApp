dotnet build -c Release
dotnet publish -c Release /p:MvcRazorCompileOnPublish=false

RMDIR ..\apps\web\ /s /q
XCOPY /E bin\Release\netcoreapp2.0\publish ..\apps\web\

PUSHD ..\..\..\Web
for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q || del "%%i" /s/q)
POPD

XCOPY /E bin\Release\netcoreapp2.0\publish ..\..\..\Web
COPY README.md ..\..\..\Web\
