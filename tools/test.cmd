@echo off


@setlocal 
color 09

set SLNDIR=C:\root\V2\coapp.powershell
set TOOLSDIR=%SLNDIR%\tools
set NODE_PATH=%TOOLSDIR%\node

path = %TOOLSDIR%;%NODE_PATH%\node_modules\.bin;%PATH%

set NPM=call npm
set REPLACE=replace.cmd

node -v
%NPM% -v

node %SLNDIR%\tools\incver.js %SLNDIR%\version.txt

for /F %%v in (version.txt) do set NEWVER=%%v
echo %NEWVER%

call replace.cmd "\[assembly: AssemblyVersion.*" "[assembly: AssemblyVersion(""%NEWVER%"")]" %SLNDIR%\CoApp.Powershell\assemblystrongname.cs
call replace.cmd "\[assembly: AssemblyFileVersion.*" "[assembly: AssemblyFileVersion(""%NEWVER%"")]" %SLNDIR%\CoApp.Powershell\assemblystrongname.cs

call replace.cmd ".Product.Id=""\*"".Name=""CoApp.Powershell"".*" "<Product Id=""*"" Name=""CoApp.Powershell"" Language=""1033"" Version=""%NEWVER%"" Manufacturer=""Outercurve Foundation, CoApp Project"" UpgradeCode=""a43c25d7-b22a-411c-b780-3a83479b1d26"">" %SLNDIR%\Installer\Product.wxs

call replace.cmd ".UpgradeVersion.OnlyDetect=""no"".*" "<UpgradeVersion OnlyDetect=""no"" Property=""PREVIOUSFOUND"" Minimum=""1.0.0"" IncludeMinimum=""yes"" Maximum=""%NEWVER%"" IncludeMaximum=""no""></UpgradeVersion>"

cmd.exe 

color 0E