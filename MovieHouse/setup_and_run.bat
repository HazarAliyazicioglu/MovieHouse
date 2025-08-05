@echo off
chcp 65001 >nul
echo ========================================
echo    MovieHouse Projesi Kurulum ve Çalıştırma
echo ========================================
echo.

:: Renk kodları
set "GREEN=[92m"
set "YELLOW=[93m"
set "RED=[91m"
set "BLUE=[94m"
set "RESET=[0m"

echo %BLUE%1. Gerekli araçların kontrolü...%RESET%
echo.

:: .NET SDK kontrolü
echo %YELLOW%   .NET SDK kontrol ediliyor...%RESET%
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo %RED%   ❌ .NET SDK bulunamadı!%RESET%
    echo %YELLOW%   Lütfen https://dotnet.microsoft.com/download adresinden .NET 8.0 SDK'yı indirin.%RESET%
    pause
    exit /b 1
) else (
    echo %GREEN%   ✅ .NET SDK mevcut%RESET%
)

:: Python kontrolü
echo %YELLOW%   Python kontrol ediliyor...%RESET%
python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo %RED%   ❌ Python bulunamadı!%RESET%
    echo %YELLOW%   Lütfen https://python.org/downloads adresinden Python 3.8+ indirin.%RESET%
    pause
    exit /b 1
) else (
    echo %GREEN%   ✅ Python mevcut%RESET%
)

:: SQL Server LocalDB kontrolü
echo %YELLOW%   SQL Server LocalDB kontrol ediliyor...%RESET%
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT @@VERSION" >nul 2>&1
if %errorlevel% neq 0 (
    echo %RED%   ❌ SQL Server LocalDB bulunamadı!%RESET%
    echo %YELLOW%   Lütfen SQL Server Express LocalDB'yi yükleyin.%RESET%
    pause
    exit /b 1
) else (
    echo %GREEN%   ✅ SQL Server LocalDB mevcut%RESET%
)

echo.
echo %BLUE%2. Python paketlerinin yüklenmesi...%RESET%
echo %YELLOW%   Gerekli Python paketleri yükleniyor...%RESET%
pip install -r requirements.txt
if %errorlevel% neq 0 (
    echo %RED%   ❌ Python paketleri yüklenemedi!%RESET%
    pause
    exit /b 1
) else (
    echo %GREEN%   ✅ Python paketleri yüklendi%RESET%
)

echo.
echo %BLUE%3. Veritabanı kurulumu...%RESET%
echo %YELLOW%   Veritabanı migration'ları uygulanıyor...%RESET%
dotnet ef database update
if %errorlevel% neq 0 (
    echo %RED%   ❌ Veritabanı migration'ları uygulanamadı!%RESET%
    echo %YELLOW%   Veritabanını sıfırlamayı deneyin: dotnet ef database drop --force%RESET%
    pause
    exit /b 1
) else (
    echo %GREEN%   ✅ Veritabanı hazır%RESET%
)

echo.
echo %BLUE%4. Veri import işlemi...%RESET%
echo %YELLOW%   Film verileri import ediliyor...%RESET%
echo %YELLOW%   Bu işlem birkaç dakika sürebilir...%RESET%
python import_movies.py
if %errorlevel% neq 0 (
    echo %RED%   ❌ Veri import işlemi başarısız!%RESET%
    echo %YELLOW%   Manuel olarak tekrar deneyin: python import_movies.py%RESET%
    pause
    exit /b 1
) else (
    echo %GREEN%   ✅ Veri import tamamlandı%RESET%
)

echo.
echo %BLUE%5. Film posterlerinin güncellenmesi...%RESET%
echo %YELLOW%   Film posterleri IMDB'den güncelleniyor...%RESET%
echo %YELLOW%   Bu işlem uzun sürebilir (10-20 dakika)...%RESET%
echo %YELLOW%   İsterseniz bu adımı atlayabilirsiniz (Ctrl+C)%RESET%
timeout /t 5 /nobreak >nul

python imdb_advanced_scraper.py
if %errorlevel% neq 0 (
    echo %YELLOW%   ⚠️ Poster güncelleme atlandı veya hata oluştu%RESET%
) else (
    echo %GREEN%   ✅ Poster güncelleme tamamlandı%RESET%
)

echo.
echo %BLUE%6. Proje derleme...%RESET%
echo %YELLOW%   Proje derleniyor...%RESET%
dotnet build
if %errorlevel% neq 0 (
    echo %RED%   ❌ Proje derlenemedi!%RESET%
    pause
    exit /b 1
) else (
    echo %GREEN%   ✅ Proje başarıyla derlendi%RESET%
)

echo.
echo %GREEN%========================================%RESET%
echo %GREEN%🎉 MovieHouse projesi başarıyla kuruldu!%RESET%
echo %GREEN%========================================%RESET%
echo.
echo %BLUE%Proje çalıştırılıyor...%RESET%
echo %YELLOW%Tarayıcınızda http://localhost:5000 adresini açın%RESET%
echo %YELLOW%Durdurmak için Ctrl+C tuşlayın%RESET%
echo.

:: Projeyi çalıştır
dotnet run

pause 