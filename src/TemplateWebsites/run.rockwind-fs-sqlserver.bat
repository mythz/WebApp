COPY ..\apps\rockwind-fs\web.sqlserver.settings ..\TemplateWebsites\bin\Debug\netcoreapp2.0\rockwind-fs-sqlserver.web.settings
COPY ..\apps\rockwind-fs\web.sqlserver.settings ..\TemplateWebsites\bin\Release\netcoreapp2.0\rockwind-fs-sqlserver.web.settings

dotnet run rockwind-fs-sqlserver.web.settings
