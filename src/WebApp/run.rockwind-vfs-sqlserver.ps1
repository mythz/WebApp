(Get-Content ..\apps\rockwind-vfs\web.sqlserver.settings) `
    -replace 'contentRoot ~/../rockwind-vfs', 'contentRoot ~/../../../../apps/rockwind-vfs' `
    -replace 'webRoot ~/../rockwind-vfs', 'webRoot ~/../../../../apps/rockwind-vfs' `
    | Set-Content bin\Debug\netcoreapp2.1\web.settings

dotnet run
