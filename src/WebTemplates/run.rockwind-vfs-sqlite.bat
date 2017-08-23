COPY ..\apps\rockwind-vfs\web.sqlite.settings ..\TemplateWebsites\bin\Debug\netcoreapp2.0\rockwind-vfs-sqlite.web.settings
COPY ..\apps\rockwind-vfs\web.sqlite.settings ..\TemplateWebsites\bin\Release\netcoreapp2.0\rockwind-vfs-sqlite.web.settings

dotnet run rockwind-vfs-sqlite.web.settings
