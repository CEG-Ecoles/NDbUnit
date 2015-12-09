@echo off

set repos=http://vdnnet1.vdn.ne.ch/nuget/api/v2/package/
set apikey=9RerAspawrAch9

GOTO :ALL

:MOVEOUTPUT
if NOT EXIST OutputOld md OutputOld
for /f %%F in ('dir /b Output\*.nupkg') DO move Output\%%F "OutputOld"\%%F
GOTO :EOF

:ENSUREOUTPUTPATHEXISTS
if NOT EXIST Output md Output
GOTO :EOF

:INVOKEPACK
..\.nuget\nuget.exe pack %1 -o "Output"
GOTO :EOF

:INVOKEPUSH
..\.nuget\nuget.exe push Output\%1 -source %repos% -apikey %apikey%
GOTO :EOF

:ALL
CALL :ENSUREOUTPUTPATHEXISTS
CALL :MOVEOUTPUT
for /f %%F in ('dir /b *.nuspec') DO CALL :INVOKEPACK %%F
GOTO :PUSHTOSERVER

:PUSHTOSERVER
for /f %%F in ('dir /b Output\*.nupkg') DO CALL :INVOKEPUSH %%F
GOTO :EOF
