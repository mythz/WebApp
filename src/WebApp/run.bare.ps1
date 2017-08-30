(Get-Content ..\apps\bare\web.settings) `
    -replace 'contentRoot ~/../bare', 'contentRoot ~/../../../../apps/bare' `
    -replace 'webRoot ~/../bare', 'webRoot ~/../../../../apps/bare' `
    | Set-Content bin\Debug\netcoreapp2.0\web.settings

dotnet run
