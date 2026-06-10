@echo off
setlocal

cd /d "%~dp0"

start "AG Viewer Server" cmd /k python viewer_server.py

timeout /t 2 /nobreak > nul

start "" chrome "http://127.0.0.1:8765/viewer/main"

endlocal
