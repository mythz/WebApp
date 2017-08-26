dotnet build -c Release
dotnet publish -c Release

RMDIR ..\apps\bin\ /s /q
XCOPY /E bin\Release\netcoreapp2.0\publish ..\apps\bin\