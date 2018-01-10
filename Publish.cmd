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

call :joinpath %baseDir% "Dist"
set distDir=%result%

call :joinpath %distDir% %packageVersion%
set outputDir=%result%

if {%command%}=={pack} (
	call :pack %%p
	goto :eof
)

if {%command%}=={publish} (
	call :publish %%p
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

:publish
	for %%p in (%outputDir%\*.nupkg) do (
		call :publishPackage %%p
	)
	goto :eof

:publishPackage
	set package=%~f1
	echo %package%
	goto :eof

:joinpath
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
	echo command            The command to execute ("pack" or "publish")
	echo.
	echo packageVersion     The version number to assign to the package (e.g. "5.0.0-beta")
	echo.
	echo apiKey             The package server API key for the "publish" command
	goto :eof