@echo off

set command=%~1
if {%command%}=={} (
	call :printHelp "Command is required"
	goto :eof
)

set packageVersion=%~2
if {%packageVersion%}=={} (
	call :printHelp "Package version is required"
	goto :eof
)

set baseDir=%~dp0

call :joinPath %baseDir% "Dist"
set distDir=%result%

call :joinPath %distDir% %packageVersion%
set outputDir=%result%

if {%command%}=={pack} (
	call :pack
	goto :eof
)

if {%command%}=={push} (
	set apiKey=%~3
	call :push %apiKey%
	goto :eof
)

call :printHelp "Unsupported command: %command%"
goto :eof

:pack
	set projects="Source\Platibus\Platibus.csproj"
	set projects=%projects% "Source\Platibus.AspNetCore\Platibus.AspNetCore.csproj"
	set projects=%projects% "Source\Platibus.IIS\Platibus.IIS.csproj"
	set projects=%projects% "Source\Platibus.MongoDB\Platibus.MongoDB.csproj"
	set projects=%projects% "Source\Platibus.Owin\Platibus.Owin.csproj"
	set projects=%projects% "Source\Platibus.RabbitMQ\Platibus.RabbitMQ.csproj"
	set projects=%projects% "Source\Platibus.SQLite\Platibus.SQLite.csproj"

	if not exist "%outputDir%" (
		mkdir "%outputDir%"
	)
	
	if exist %outputDir%\*.nupkg (
		del %outputDir%\*.nupkg
	)

	for %%p in (%projects%) do (
		call :packProject %%p
	)
	goto :eof

:packProject
	set project=%~f1
	dotnet pack "%project%" -c Release --include-source --include-symbols /p:PackageVersion=%packageVersion% -o "%outputDir%"
	goto :eof

:push
	for %%p in (%outputDir%\*%packageVersion%.nupkg) do (
		call :pushPackage %%p %apikey%
	)
	goto :eof

:pushPackage
	set package=%~f1
	set pushArgs=%package% -s https://api.nuget.org/v3/index.json
	if "%apiKey%" neq "" (
		set pushArgs=%pushArgs% -k %apiKey% 
	)
	dotnet nuget push %pushArgs%
	goto :eof

:pushSymbolsPackage
	set symbolsPackage=%~f1
	set pushArgs=%symbolsPackage% -ss https://nuget.smbsrc.net/
	if "%apiKey%" neq "" (
		set pushArgs=%pushArgs% -sk %apiKey% 
	)
	dotnet nuget push %pushArgs%
	goto :eof

:joinPath
	set basePath=%~1
	set subPath=%~2
	if {%basePath:~-1,1%}=={\} (
		set result=%basePath%%subPath%
	) else (
		set result=%basePath%\%subPath%
	)
	goto :eof

:printHelp
	set message=%~1
	if "%message%" neq "" (
		echo %message%
		echo.
	)
	echo Usage:
	echo     Publish.cmd command packageVersion [apiKey]
	echo.
	echo Arguments:
	echo.
	echo command            The command to execute ("pack" or "push")
	echo.
	echo packageVersion     The version number to assign to the package (e.g. "5.0.0-beta")
	echo.
	echo apiKey             The package server API key for the "push" command
	goto :eof