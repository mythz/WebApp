dotnet build ..\example-plugins\FilterInfo
copy ..\example-plugins\FilterInfo\bin\Debug\netcoreapp2.0\FilterInfo.dll ..\apps\web-plugins\plugins

dotnet build ..\example-plugins\ServerInfo
copy ..\example-plugins\ServerInfo\bin\Debug\netcoreapp2.0\ServerInfo.dll ..\apps\web-plugins\plugins

(Get-Content ..\apps\web-plugins\web.settings) `
    -replace 'contentRoot ~/../web-plugins', 'contentRoot ~/../../../../apps/web-plugins' `
    -replace 'webRoot ~/../web-plugins/wwwroot', 'webRoot ~/../../../../apps/web-plugins/wwwroot' `
    | Set-Content bin\Debug\netcoreapp2.0\web.settings

dotnet run
