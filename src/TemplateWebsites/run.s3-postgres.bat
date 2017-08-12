if exist ..\apps\s3-postgres\web.settings.prod.meta (
    COPY ..\apps\s3-postgres\web.settings.prod.meta ..\TemplateWebsites\bin\Debug\netcoreapp2.0\s3-postgres.web.settings
    COPY ..\apps\s3-postgres\web.settings.prod.meta ..\TemplateWebsites\bin\Release\netcoreapp2.0\s3-postgres.web.settings
) else (
    COPY ..\apps\s3-postgres\web.settings ..\TemplateWebsites\bin\Debug\netcoreapp2.0\s3-postgres.web.settings
    COPY ..\apps\s3-postgres\web.settings ..\TemplateWebsites\bin\Release\netcoreapp2.0\s3-postgres.web.settings
)

dotnet run s3-postgres.web.settings
