COPY ..\apps\fs-redis\web.settings ..\TemplateWebsites\bin\Debug\netcoreapp2.0\fs-redis.web.settings
COPY ..\apps\fs-redis\web.settings ..\TemplateWebsites\bin\Release\netcoreapp2.0\fs-redis.web.settings

dotnet run fs-redis.web.settings
