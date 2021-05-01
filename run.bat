echo off
set DISTLAB_CONTAINER_PGM_PATH=%cd%/.container/distlab.container.exe
set DISTLAB_CONTAINERS_ASSEMBLY_ROOT=%cd%/.containers
set DISTLAB_DATA_PLAN_ROOT=%cd%/.dataplan
IF "%1"=="" goto :run
IF "%1"=="o" goto :obs
:obs
call start.bat
:run
cd .controller
dotnet distlab.controller.dll inMemoryDBEventual