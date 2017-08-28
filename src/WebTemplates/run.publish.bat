dotnet build -c Release
dotnet publish -c Release /p:MvcRazorCompileOnPublish=false

RMDIR ..\apps\app\ /s /q
XCOPY /E bin\Release\netcoreapp2.0\publish ..\apps\app\