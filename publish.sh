#!/bin/bash

rm -r published/

# To run the app locally you can use the following steps:
# cd published/
# export ASPNETCORE_URLS=https://localhost:7093/;
# ./Keepi.Web 
dotnet publish -c Release src/Keepi.Web/ -o published/