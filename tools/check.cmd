@echo off
rem Sprawdzenie komputera do warsztatu - uruchom dwuklikiem.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0check.ps1" %*
set CODE=%ERRORLEVEL%
pause
exit /b %CODE%
