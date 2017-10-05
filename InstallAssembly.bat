@ECHO OFF
@SET GACUTIL="c:\Program Files\Microsoft Visual Studio 8\SDK\v2.0\Bin\gacutil.exe"

%GACUTIL% -if bin\debug\CodeTesterWebPart.dll

cscript c:\windows\system32\iisapp.vbs /a "SharePoint - 80" /r

