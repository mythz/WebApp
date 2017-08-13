COPY ..\apps\rockwind-vfs-aws\web.settings ..\TemplateWebsites\bin\Debug\netcoreapp2.0\rockwind-vfs-aws.web.settings
COPY ..\apps\rockwind-vfs-aws\web.settings ..\TemplateWebsites\bin\Release\netcoreapp2.0\rockwind-vfs-aws.web.settings

dotnet run rockwind-vfs-aws.web.settings
