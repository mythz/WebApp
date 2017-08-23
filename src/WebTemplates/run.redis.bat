COPY ..\apps\redis\web.settings ..\TemplateWebsites\bin\Debug\netcoreapp2.0\redis.web.settings
COPY ..\apps\redis\web.settings ..\TemplateWebsites\bin\Release\netcoreapp2.0\redis.web.settings

dotnet run redis.web.settings
