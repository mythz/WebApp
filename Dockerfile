FROM microsoft/dotnet:2.0-sdk
COPY src /app
WORKDIR /app/TemplateWebsites
COPY ../apps/rockwind-vfs-aws/web.settings web.settings
RUN ["dotnet", "restore", "--configfile", "NuGet.Config"]
RUN ["dotnet", "build"]
EXPOSE 5000/tcp
ENV ASPNETCORE_URLS https://*:5000
ENTRYPOINT ["dotnet", "web.dll", "web.settings", "--server.urls", "http://*:5000"]
