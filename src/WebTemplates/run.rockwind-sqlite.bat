COPY ..\apps\rockwind-fs\web.sqlite.settings ..\TemplateWebsites\bin\Debug\netcoreapp2.0\rockwind-sqlite.web.settings
COPY ..\apps\rockwind-fs\web.sqlite.settings ..\TemplateWebsites\bin\Release\netcoreapp2.0\rockwind-sqlite.web.settings

dotnet run rockwind-sqlite.web.settings
