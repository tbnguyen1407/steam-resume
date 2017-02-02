@ECHO OFF
SETLOCAL

FOR %%a in ("%~f0\..") DO SET prjName=%%~nxa
SET dirCompile=%~dp0\out\compile\%prjName%
SET dirPublish=%~dp0\out\publish\%prjName%
SET exclude=*.dll *.pdb *.vshost *.xml

ROBOCOPY %dirCompile% %dirPublish% /S /E /XF %exclude%
ROBOCOPY %dirCompile% %dirPublish%\lib *.dll /XF %prjName%.*.dll
ROBOCOPY %dirCompile% %dirPublish% %prjName%.*.dll

ENDLOCAL
EXIT /b