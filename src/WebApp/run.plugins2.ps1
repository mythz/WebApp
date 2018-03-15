dotnet build ..\example-plugins\FilterInfo
copy ..\example-plugins\FilterInfo\bin\Debug\netcoreapp2.0\FilterInfo.dll ..\apps\plugins2\plugins

dotnet build ..\example-plugins\ServerInfo
copy ..\example-plugins\ServerInfo\bin\Debug\netcoreapp2.0\ServerInfo.dll ..\apps\plugins2\plugins

(Get-Content ..\apps\plugins2\web.settings) `
    -replace 'contentRoot ~/../plugins2', 'contentRoot ~/../../../../apps/plugins2' `
    -replace 'webRoot ~/../plugins2', 'webRoot ~/../../../../apps/plugins2' `
    | Set-Content bin\Debug\netcoreapp2.0\web.settings

dotnet run
