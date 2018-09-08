dotnet pack WebCef.csproj -c release -o nupkg
dotnet tool uninstall -g cef
dotnet tool install --add-source .\nupkg -g cef