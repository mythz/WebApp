RMDIR ..\..\..\WebAppStarter\app /s /q
XCOPY /E ..\apps\bare ..\..\..\WebAppStarter\app\

RMDIR ..\..\..\WebAppStarter\web /s /q
XCOPY /E ..\apps\web ..\..\..\WebAppStarter\web\


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
