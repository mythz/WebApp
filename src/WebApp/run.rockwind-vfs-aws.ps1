(Get-Content ..\apps\rockwind-vfs\web.aws.settings) `
    | Set-Content bin\Debug\netcoreapp2.1\web.settings

dotnet run
