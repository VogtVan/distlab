#!/bin/bash
export THESIS_CONTAINER_PGM_PATH=$PWD/.container/distlab.container &&
export THESIS_CONTAINERS_ASSEMBLY_ROOT=$PWD/.containers &&
export THESIS_DATA_PLAN_ROOT=$PWD/.dataplan &&
cd .controller &&
dotnet distlab.controller.dll inMemoryDBEventual