(Get-Content ..\apps\rockwind\web.sqlserver.settings) `
    -replace 'contentRoot ~/../rockwind', 'contentRoot ~/../../../../apps/rockwind' `
    -replace 'webRoot ~/../rockwind', 'webRoot ~/../../../../apps/rockwind' `
    | Set-Content bin\Debug\netcoreapp2.1\web.settings

dotnet run
