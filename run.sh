#!/bin/bash
if [ "$1" = "o" ]
then
    ./start.sh 
fi
export DISTLAB_CONTAINER_PGM_PATH=$PWD/.container/distlab.container &&
export DISTLAB_CONTAINERS_ASSEMBLY_ROOT=$PWD/.containers &&
export DISTLAB_DATA_PLAN_ROOT=$PWD/.dataplan &&
cd .controller &&
dotnet distlab.controller.dll inMemoryDBEventual