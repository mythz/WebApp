
dotnet pack -c release -o nupkg
dotnet tool uninstall -g web
dotnet tool install --add-source .\nupkg -g web