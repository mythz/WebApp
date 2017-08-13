COPY ..\apps\rockwind-vfs\web.sqlserver.settings ..\TemplateWebsites\bin\Debug\netcoreapp2.0\rockwind-vfs-sqlserver.web.settings
COPY ..\apps\rockwind-vfs\web.sqlserver.settings ..\TemplateWebsites\bin\Release\netcoreapp2.0\rockwind-vfs-sqlserver.web.settings

dotnet run rockwind-vfs-sqlserver.web.settings
