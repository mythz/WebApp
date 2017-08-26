dotnet build ..\example-plugins\FiltersPlugin
copy ..\example-plugins\FiltersPlugin\bin\Debug\netcoreapp2.0\FiltersPlugin.dll ..\apps\web-plugins\plugins

dotnet build ..\example-plugins\ServicesPlugin
copy ..\example-plugins\ServicesPlugin\bin\Debug\netcoreapp2.0\ServicesPlugin.dll ..\apps\web-plugins\plugins

(Get-Content ..\apps\web-plugins\web.settings) `
    -replace 'contentRoot ~/', 'contentRoot ~/../../../../apps/web-plugins' `
    -replace 'webRoot ~/wwwroot', 'webRoot ~/../../../../apps/web-plugins/wwwroot' `
    | Set-Content bin\Debug\netcoreapp2.0\web.settings

dotnet run
