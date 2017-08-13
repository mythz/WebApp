COPY ..\apps\fs-mysql\web.settings ..\TemplateWebsites\bin\Debug\netcoreapp2.0\fs-mysql.web.settings
COPY ..\apps\fs-mysql\web.settings ..\TemplateWebsites\bin\Release\netcoreapp2.0\fs-mysql.web.settings

dotnet run fs-mysql.web.settings
