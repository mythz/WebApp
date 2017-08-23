COPY ..\apps\rockwind-vfs-aws\web.settings bin\Debug\netcoreapp2.0\rockwind-vfs-aws.web.settings
COPY ..\apps\rockwind-vfs-aws\web.settings bin\Release\netcoreapp2.0\rockwind-vfs-aws.web.settings

dotnet run rockwind-vfs-aws.web.settings
