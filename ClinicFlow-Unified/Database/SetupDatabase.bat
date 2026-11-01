@echo off
setlocal
title ClinicFlow Unified - Automated Database Setup
echo =================================================================
echo CLINICFLOW UNIFIED DATABASE BOOTSTRAPPER LAUNCHER
echo =================================================================
echo.
echo Launching automated SQL Server database setup and migration...
echo.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0SetupDatabase.ps1"
echo.
pause
