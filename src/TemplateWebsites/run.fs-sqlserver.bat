COPY ..\apps\fs-sqlserver\web.settings ..\TemplateWebsites\bin\Debug\netcoreapp2.0\fs-sqlserver.web.settings
COPY ..\apps\fs-sqlserver\web.settings ..\TemplateWebsites\bin\Release\netcoreapp2.0\fs-sqlserver.web.settings

dotnet run fs-sqlserver.web.settings
