COPY ..\apps\rockwind-vfs\web.postgres.settings ..\TemplateWebsites\bin\Debug\netcoreapp2.0\rockwind-vfs-postgres.web.settings
COPY ..\apps\rockwind-vfs\web.postgres.settings ..\TemplateWebsites\bin\Release\netcoreapp2.0\rockwind-vfs-postgres.web.settings

dotnet run rockwind-vfs-postgres.web.settings
