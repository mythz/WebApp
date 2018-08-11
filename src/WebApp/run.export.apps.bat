DEL ..\apps\web\web.*.settings

RMDIR ..\..\..\..\NetCoreTemplates\templates-webapp\web /s /q
XCOPY /E ..\apps\web ..\..\..\..\NetCoreTemplates\templates-webapp\web\

RMDIR ..\..\..\..\NetCoreTemplates\parcel-webapp\web /s /q
XCOPY /E ..\apps\web ..\..\..\..\NetCoreTemplates\parcel-webapp\web\

RMDIR ..\..\..\WebAppStarter\app /s /q
XCOPY /E ..\apps\bare ..\..\..\WebAppStarter\app\

RMDIR ..\..\..\WebAppStarter\web /s /q
XCOPY /E ..\apps\web ..\..\..\WebAppStarter\web\


RMDIR ..\..\..\Chat\app /s /q
XCOPY /E ..\apps\chat ..\..\..\Chat\app\

RMDIR ..\..\..\Chat\web /s /q
XCOPY /E ..\apps\web ..\..\..\Chat\web\


RMDIR ..\..\..\Plugins\app /s /q
XCOPY /E ..\apps\plugins ..\..\..\Plugins\app\

RMDIR ..\..\..\Plugins\web /s /q
XCOPY /E ..\apps\web ..\..\..\Plugins\web\


RMDIR ..\..\..\Redis\app /s /q
XCOPY /E ..\apps\redis ..\..\..\Redis\app\

RMDIR ..\..\..\Redis\web /s /q
XCOPY /E ..\apps\web ..\..\..\Redis\web\


RMDIR ..\..\..\RedisHtml\app /s /q
XCOPY /E ..\apps\redis-html ..\..\..\RedisHtml\app\

RMDIR ..\..\..\RedisHtml\web /s /q
XCOPY /E ..\apps\web ..\..\..\RedisHtml\web\


RMDIR ..\..\..\Rockwind\app /s /q
XCOPY /E ..\apps\rockwind ..\..\..\Rockwind\app\

RMDIR ..\..\..\Rockwind\web /s /q
XCOPY /E ..\apps\web ..\..\..\Rockwind\web\
COPY ..\apps\northwind.sqlite ..\..\..\Rockwind\app\


RMDIR ..\..\..\Rockwind.Aws\app /s /q
XCOPY /E ..\apps\rockwind-vfs ..\..\..\Rockwind.Aws\app\
DEL ..\..\..\Rockwind.Aws\app\web.*.settings
COPY ..\apps\rockwind-vfs\web.aws.settings ..\..\..\Rockwind.Aws\app\web.settings

RMDIR ..\..\..\Rockwind.Aws\web /s /q
XCOPY /E ..\apps\web ..\..\..\Rockwind.Aws\web\


RMDIR ..\..\..\Rockwind.Azure\app /s /q
XCOPY /E ..\apps\rockwind-vfs ..\..\..\Rockwind.Azure\app\
DEL ..\..\..\Rockwind.Azure\app\web.*.settings
COPY ..\apps\rockwind-vfs\web.azure.settings ..\..\..\Rockwind.Azure\app\web.settings

RMDIR ..\..\..\Rockwind.Azure\web /s /q
XCOPY /E ..\apps\web ..\..\..\Rockwind.Azure\web\

Powershell.exe -executionpolicy remotesigned -File run.export.apps.ps1
