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

(Get-Content ..\apps\rockwind\web.sqlite.settings) `
    -replace 'contentRoot ~/..', 'contentRoot ~/../app' `
    -replace 'webRoot ~/../rockwind', 'webRoot ~/../app' `
    | Set-Content ..\..\..\Rockwind\app\web.settings

