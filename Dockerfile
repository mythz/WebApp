FROM microsoft/dotnet:2.0-sdk
COPY src /app
COPY src/apps/rockwind-vfs-aws/web.settings /app/WebTemplates
WORKDIR /app/WebTemplates
RUN ["dotnet", "restore", "--configfile", "NuGet.Config"]
RUN ["dotnet", "build"]
EXPOSE 5000/tcp
ENV ASPNETCORE_URLS https://*:5000
ENTRYPOINT ["dotnet", "run", "web.settings", "--server.urls", "http://*:5000"]
