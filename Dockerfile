FROM microsoft/dotnet:2.0-sdk
COPY src /app
COPY src/apps/rockwind-vfs-aws/web.release.settings /app/WebTemplates
WORKDIR /app/WebTemplates
RUN ["dotnet", "restore", "--configfile", "NuGet.Config"]
RUN ["dotnet", "build", "-c", "Release"]
EXPOSE 5000/tcp
ENV ASPNETCORE_URLS https://*:5000
ENTRYPOINT ["dotnet", "run"]
