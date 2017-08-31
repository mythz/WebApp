(Get-Content ..\apps\rockwind-vfs\web.postgres.settings) `
    -replace 'contentRoot ~/../rockwind-vfs', 'contentRoot ~/../../../../apps/rockwind-vfs' `
    -replace 'webRoot ~/../rockwind-vfs', 'webRoot ~/../../../../apps/rockwind-vfs' `
    | Set-Content bin\Debug\netcoreapp2.0\web.settings

dotnet run
