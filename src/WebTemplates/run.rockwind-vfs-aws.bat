COPY ..\apps\rockwind-vfs\web.aws.settings bin\Debug\netcoreapp2.0\rockwind-aws.web.settings
COPY ..\apps\rockwind-vfs\web.aws.settings bin\Release\netcoreapp2.0\rockwind-aws.web.settings
COPY ..\apps\rockwind-vfs\web.aws.settings bin\Debug\netcoreapp2.0\web.settings

dotnet run rockwind-aws.web.settings
