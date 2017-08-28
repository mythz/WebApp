(Get-Content ..\apps\rockwind-vfs\web.postgres.settings) `
    -replace 'contentRoot ~/..', 'contentRoot ~/../../../../apps' `
    -replace 'webRoot ~/../rockwind', 'webRoot ~/../../../../apps/rockwind-vfs' `
    | Set-Content bin\Debug\netcoreapp2.0\web.settings

dotnet run
