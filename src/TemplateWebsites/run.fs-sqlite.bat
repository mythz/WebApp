COPY ..\apps\fs-sqlite\web.settings ..\TemplateWebsites\bin\Debug\netcoreapp2.0\fs-sqlite.web.settings
COPY ..\apps\fs-sqlite\web.settings ..\TemplateWebsites\bin\Release\netcoreapp2.0\fs-sqlite.web.settings

dotnet run fs-sqlite.web.settings
