FROM microsoft/dotnet:2.0-sdk
COPY src /app
WORKDIR /app/WebTemplates
RUN ["dotnet", "restore", "--configfile", "NuGet.Config"]
RUN ["dotnet", "build", "-c", "Release"]
COPY src/apps/rockwind-vfs-aws/web.settings /app/WebTemplates/bin/Release/netcoreapp2.0
EXPOSE 5000/tcp
ENV ASPNETCORE_URLS https://*:5000
ENTRYPOINT ["dotnet", "/app/WebTemplates/bin/Release/netcoreapp2.0/web.dll", "--server.urls", "http://*:5000"]
