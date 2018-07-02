(Get-Content ..\apps\redis-html\web.settings) `
    -replace 'contentRoot ~/../redis-html', 'contentRoot ~/../../../../apps/redis-html' `
    -replace 'webRoot ~/../redis-html', 'webRoot ~/../../../../apps/redis-html' `
    | Set-Content bin\Debug\netcoreapp2.1\web.settings

dotnet run
