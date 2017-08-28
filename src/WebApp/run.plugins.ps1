dotnet build ..\example-plugins\FilterInfo
copy ..\example-plugins\FilterInfo\bin\Debug\netcoreapp2.0\FilterInfo.dll ..\apps\plugins\plugins

dotnet build ..\example-plugins\ServerInfo
copy ..\example-plugins\ServerInfo\bin\Debug\netcoreapp2.0\ServerInfo.dll ..\apps\plugins\plugins

(Get-Content ..\apps\plugins\web.settings) `
    -replace 'contentRoot ~/../plugins', 'contentRoot ~/../../../../apps/plugins' `
    -replace 'webRoot ~/../plugins/wwwroot', 'webRoot ~/../../../../apps/plugins/wwwroot' `
    | Set-Content bin\Debug\netcoreapp2.0\web.settings

dotnet run
