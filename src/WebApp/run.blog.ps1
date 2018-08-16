(Get-Content ..\apps\blog\web.settings) `
    -replace 'contentRoot ~/../blog', 'contentRoot ~/../../../../apps/blog' `
    -replace 'webRoot ~/../blog', 'webRoot ~/../../../../apps/blog' `
    | Set-Content bin\Debug\netcoreapp2.1\web.settings

dotnet run
