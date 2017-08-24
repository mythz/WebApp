COPY ..\apps\rockwind-fs\web.mysql.settings ..\TemplateWebsites\bin\Debug\netcoreapp2.0\rockwind-mysql.web.settings
COPY ..\apps\rockwind-fs\web.mysql.settings ..\TemplateWebsites\bin\Release\netcoreapp2.0\rockwind-mysql.web.settings

dotnet run rockwind-mysql.web.settings
