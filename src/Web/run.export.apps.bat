RMDIR ..\..\..\..\NetCoreTemplates\bare-webapp\web /s /q
XCOPY /E /Y ..\apps\web ..\..\..\..\NetCoreTemplates\bare-webapp\web\
RMDIR ..\..\..\..\NetCoreTemplates\bare-webapp\app /s /q
XCOPY /E /Y ..\apps\bare ..\..\..\..\NetCoreTemplates\bare-webapp\app\
DEL ..\..\..\..\NetCoreTemplates\bare-webapp\app\app.min.settings

RMDIR ..\..\..\..\NetCoreTemplates\parcel-webapp\web /s /q
XCOPY /E /Y ..\apps\web ..\..\..\..\NetCoreTemplates\parcel-webapp\web\

RMDIR ..\..\..\..\NetCoreTemplates\rockwind-webapp\web /s /q
XCOPY /E /Y ..\apps\web ..\..\..\..\NetCoreTemplates\rockwind-webapp\web\
RMDIR ..\..\..\..\NetCoreTemplates\rockwind-webapp\app /s /q
XCOPY /E /Y ..\apps\rockwind ..\..\..\..\NetCoreTemplates\rockwind-webapp\app\
DEL ..\..\..\..\NetCoreTemplates\rockwind-webapp\app\app.*
COPY ..\apps\rockwind\app.template.settings ..\..\..\..\NetCoreTemplates\rockwind-webapp\app\app.settings
COPY ..\apps\northwind.sqlite ..\..\..\..\NetCoreTemplates\rockwind-webapp\app\

RMDIR ..\..\..\WebAppStarter\app /s /q
XCOPY /E /Y ..\apps\bare ..\..\..\WebAppStarter\app\
RMDIR ..\..\..\WebAppStarter\web /s /q
XCOPY /E /Y ..\apps\web ..\..\..\WebAppStarter\web\

RMDIR ..\..\..\blog\app /s /q
RMDIR ..\..\..\blog\web /s /q
RMDIR ..\..\..\chat\web /s /q
RMDIR ..\..\..\chat\app /s /q
RMDIR ..\..\..\plugins\app /s /q
RMDIR ..\..\..\plugins\web /s /q
RMDIR ..\..\..\redis\app /s /q
RMDIR ..\..\..\redis\web /s /q
RMDIR ..\..\..\redis-html\app /s /q
RMDIR ..\..\..\redis-html\web /s /q
RMDIR ..\..\..\rockwind\app /s /q
RMDIR ..\..\..\rockwind\web /s /q
RMDIR ..\..\..\rockwind-aws\app /s /q
RMDIR ..\..\..\rockwind-aws\web /s /q
RMDIR ..\..\..\rockwind-azure\app /s /q
RMDIR ..\..\..\rockwind-azure\web /s /q

XCOPY /E /Y ..\apps\blog ..\..\..\blog\
DEL ..\..\..\blog\app.release.settings

XCOPY /E /Y ..\apps\chat ..\..\..\chat\

XCOPY /E /Y ..\apps\plugins ..\..\..\plugins\

XCOPY /E /Y ..\apps\redis ..\..\..\redis\

XCOPY /E /Y ..\apps\redis-html ..\..\..\redis-html\

XCOPY /E /Y ..\apps\rockwind ..\..\..\rockwind\

COPY ..\apps\northwind.sqlite ..\..\..\rockwind\
COPY ..\apps\northwind.sqlite ..\..\..\rockwind-aws\
COPY ..\apps\northwind.sqlite ..\..\..\rockwind-azure\

XCOPY /E /Y ..\apps\rockwind-vfs ..\..\..\rockwind-aws\
DEL ..\..\..\rockwind-aws\app.*.settings
COPY ..\apps\rockwind-vfs\app.aws.settings ..\..\..\rockwind-aws\app.settings

XCOPY /E /Y ..\apps\rockwind-vfs ..\..\..\rockwind-azure\
DEL ..\..\..\rockwind-azure\app.*.settings
COPY ..\apps\rockwind-vfs\app.azure.settings ..\..\..\rockwind-azure\app.settings

REM Powershell.exe -executionpolicy remotesigned -File run.export.apps.ps1
