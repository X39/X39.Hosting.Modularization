@echo off
call :CD_PROJECT
echo Starting local publish of nuget packages...

:: Restore all projects
dotnet restore
if NOT ERRORLEVEL 0 call :ERROR restore

:: Build nuget packages
dotnet build -c Release --no-restore
if NOT ERRORLEVEL 0 call :ERROR build

:: Pack nuget packages
dotnet pack -c Release --no-build --no-restore
if NOT ERRORLEVEL 0 call :ERROR build

:: Upload nuget packages
call :UPLOAD_NUGET X39.Hosting.Modularization
call :UPLOAD_NUGET X39.Hosting.Modularization.Abstraction

:: Quit the script
call :QUIT


:UPLOAD_NUGET
echo Moving nuget package of %1
IF NOT EXIST ..\.packages mkdir ..\.packages
FOR /f "delims=" %%i in ('dir /b /a-d /s "source\%1\bin\Release\%1.*nupkg"') DO (
    IF NOT EXIST ..\.packages\%%~nxi (
        move %%i ..\.packages\%%~nxi
    ) ELSE (
        del %%i
    )
)
exit /b

:ERROR
echo The task %1 failed with errorcode %ERRORLEVEL%
pause
exit ERRORLEVEL



:QUIT
exit 0

:CD_PROJECT
IF NOT EXIST shared.sln cd ..
exit /b