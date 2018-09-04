(Get-Content ..\apps\bare\web.settings) `
    -replace 'contentRoot ~/../bare', 'contentRoot ~/../app' `
    -replace 'webRoot ~/../bare', 'webRoot ~/../app' `
    | Set-Content ..\..\..\..\NetCoreTemplates\bare-webapp\app\web.settings

(Get-Content ..\apps\bare\web.settings) `
    -replace 'contentRoot ~/../bare', 'contentRoot ~/../app' `
    -replace 'webRoot ~/../bare', 'webRoot ~/../app' `
    | Set-Content ..\..\..\WebAppStarter\app\web.settings

(Get-Content ..\apps\blog\web.settings) `
    -replace 'contentRoot ~/../blog', 'contentRoot ~/../app' `
    -replace 'webRoot ~/../blog', 'webRoot ~/../app' `
    | Set-Content ..\..\..\Blog\app\web.settings

(Get-Content ..\apps\bare\web.min.settings) `
    -replace 'contentRoot ~/../bare', 'contentRoot ~/../app' `
    | Set-Content ..\..\..\WebAppStarter\app\web.min.settings

(Get-Content ..\apps\chat\web.settings) `
    -replace 'contentRoot ~/../chat', 'contentRoot ~/../app' `
    -replace 'webRoot ~/../chat', 'webRoot ~/../app' `
    | Set-Content ..\..\..\Chat\app\web.settings

(Get-Content ..\apps\plugins\web.settings) `
    -replace 'contentRoot ~/../plugins', 'contentRoot ~/../app' `
    -replace 'webRoot ~/../plugins', 'webRoot ~/../app' `
    | Set-Content ..\..\..\Plugins\app\web.settings

(Get-Content ..\apps\redis\web.settings) `
    -replace 'contentRoot ~/../redis', 'contentRoot ~/../app' `
    -replace 'webRoot ~/../redis', 'webRoot ~/../app' `
    | Set-Content ..\..\..\Redis\app\web.settings

(Get-Content ..\apps\redis-html\web.settings) `
    -replace 'contentRoot ~/../redis-html', 'contentRoot ~/../app' `
    -replace 'webRoot ~/../redis-html', 'webRoot ~/../app' `
    | Set-Content ..\..\..\RedisHtml\app\web.settings

(Get-Content ..\apps\rockwind\web.sqlite.settings) `
    -replace 'contentRoot ~/..', 'contentRoot ~/../app' `
    -replace 'webRoot ~/../rockwind', 'webRoot ~/../app' `
    | Set-Content ..\..\..\Rockwind\app\web.settings

