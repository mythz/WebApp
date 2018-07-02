dotnet build ..\example-plugins\Chat
copy ..\example-plugins\Chat\bin\Debug\netcoreapp2.0\Chat.dll ..\apps\chat\plugins

(Get-Content ..\apps\chat\web.settings) `
    -replace 'contentRoot ~/../chat', 'contentRoot ~/../../../../apps/chat' `
    -replace 'webRoot ~/../chat', 'webRoot ~/../../../../apps/chat' `
    | Set-Content bin\Debug\netcoreapp2.1\web.settings

dotnet run
