(Get-Content ..\apps\redis\web.settings) `
    -replace 'contentRoot ~/../redis', 'contentRoot ~/../../../../apps/redis' `
    -replace 'webRoot ~/../redis', 'webRoot ~/../../../../apps/redis' `
    | Set-Content bin\Debug\netcoreapp2.0\web.settings

dotnet run
