dotnet build -c Release
dotnet publish -c Release

RMDIR ..\apps\app\ /s /q
XCOPY /E bin\Release\netcoreapp2.0\publish ..\apps\app\