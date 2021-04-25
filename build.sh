#!/bin/bash
dotnet build -o .containers &&
cp "./bin/Debug/net5.0/services.dll" ".containers/services.dll"