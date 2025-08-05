@echo off
chcp 65001 >nul
echo ========================================
echo    MovieHouse Projesi Çalıştırma
echo ========================================
echo.

echo %BLUE%Proje çalıştırılıyor...%RESET%
echo %YELLOW%Tarayıcınızda http://localhost:5000 adresini açın%RESET%
echo %YELLOW%Durdurmak için Ctrl+C tuşlayın%RESET%
echo.

dotnet run

pause 