@echo off
rem Ultima Adventures - collegamento: Rete di casa (LAN)
set "BASE=%~dp0"
start "" "%BASE%Client-Windows\TazUO.exe" -settings "%BASE%Profili\lan.json" -uopath "%BASE%Dati" -profilespath "%BASE%Profili\Personaggi"
