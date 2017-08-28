(Get-Content ..\apps\rockwind\web.sqlserver.settings) `
    -replace 'contentRoot ~/..', 'contentRoot ~/../../../../apps' `
    -replace 'webRoot ~/../rockwind', 'webRoot ~/../../../../apps/rockwind' `
    | Set-Content bin\Debug\netcoreapp2.0\web.settings

dotnet run
