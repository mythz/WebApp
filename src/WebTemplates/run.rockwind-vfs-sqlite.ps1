(Get-Content ..\apps\rockwind-vfs\web.sqlite.settings) `
    -replace 'contentRoot ~/..', 'contentRoot ~/../../../../apps' `
    -replace 'webRoot ~/../rockwind-vfs', 'webRoot ~/../../../../apps/rockwind-vfs' `
    | Set-Content bin\Debug\netcoreapp2.0\web.settings

dotnet run
