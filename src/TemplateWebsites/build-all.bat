dotnet build

dotnet build -r win10-x64
dotnet build -r osx.10.11-x64
dotnet build -r ubuntu.14.04-x64

dotnet publish -c release -r win10-x64
dotnet publish -c release -r osx.10.11-x64
dotnet publish -c release -r ubuntu.14.04-x64

