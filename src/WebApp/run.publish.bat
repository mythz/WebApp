dotnet build -c Release
dotnet publish -c Release /p:MvcRazorCompileOnPublish=false

RMDIR ..\apps\web\ /s /q
XCOPY /E bin\Release\netcoreapp2.0\publish ..\apps\web\