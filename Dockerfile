FROM microsoft/dotnet:2.0-sdk
COPY src/apps /apps
WORKDIR /apps
RUN cp /apps/rockwind-vfs/web.aws.settings /apps/bin/web.settings
EXPOSE 5000/tcp
ENV ASPNETCORE_URLS https://*:5000
ENTRYPOINT ["dotnet", "/apps/bin/web.dll"]
