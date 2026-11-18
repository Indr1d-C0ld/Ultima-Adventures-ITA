@echo off
rem Ultima Adventures - collegamento: Internet
set "BASE=%~dp0"
start "" "%BASE%Client-Windows\TazUO.exe" -settings "%BASE%Profili\internet.json" -uopath "%BASE%Dati" -profilespath "%BASE%Profili\Personaggi"
