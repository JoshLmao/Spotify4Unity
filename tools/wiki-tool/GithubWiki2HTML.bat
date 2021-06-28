@echo off

echo Updating Wiki repository

rem Step into folder
cd ".\Spotify4Unity.wiki"

rem Pull git repo fore latest wiki
git submodule update --init --recursive

rem Go up a folder
cd ".."

rem Ask for version name
set /p version="Enter Version: "

echo Running GWTC to create Wiki file

rem Convert wiki to html
gwtc ".\Spotify4Unity.wiki" --file-name ".\Spotify4Unity.wiki\Spotify4Unity_v%version%_Documentation"