# 🎬 MovieHouse

Modern ve kullanıcı dostu film yönetim sistemi. Filmleri keşfedin, puanlayın ve kişisel listelerinizi oluşturun.

## ✨ Özellikler

- 🎯 **Film Keşfi**: 1000 film arasından arama yapın
- ⭐ **Puanlama Sistemi**: 5 yıldızlık film puanlama
- 📋 **Kişisel Listeler**: İzlediklerim, izleyeceklerim ve özelleştirilmiş listeler 
- 🔍 **Gelişmiş Arama**: Film adı, yönetmen, oyuncu ve tür bazında arama
- 🎨 **Modern UI**: Responsive ve kullanıcı dostu arayüz
- 👤 **Kullanıcı Sistemi**: Kayıt olun ve kişisel profilinizi yönetin

## 🚀 Hızlı Başlangıç

### Gereksinimler

- .NET 8.0 SDK
- Python 3.8+
- SQL Server LocalDB

### Kurulum

1. **Projeyi klonlayın:**
   ```bash
   git clone https://github.com/kullaniciadi/MovieHouse.git
   cd MovieHouse
   ```

2. **Otomatik kurulum (Önerilen):**
   ```bash
   setup_and_run.bat
   ```

3. **Manuel kurulum:**
   ```bash
   # Python paketlerini yükleyin
   pip install -r requirements.txt
   
   # Veritabanını oluşturun
   dotnet ef database update
   
   # Verileri import edin
   python import_movies.py
   
   # Projeyi çalıştırın
   dotnet run
   ```

### Hızlı Çalıştırma

Proje kurulduktan sonra:
```bash
run.bat
```

## 📁 Proje Yapısı

```
MovieHouse/
├── Controllers/          # MVC Controller'ları
├── Models/              # Veri modelleri
├── Views/               # Razor view'ları
├── Data/                # Entity Framework context
├── wwwroot/             # Statik dosyalar (CSS, JS)
├── Migrations/          # Veritabanı migration'ları
├── Datasets2/           # Film verileri
├── import_movies.py     # Veri import scripti
├── setup_and_run.bat    # Otomatik kurulum
└── run.bat             # Hızlı çalıştırma
```

## 🛠️ Teknolojiler

- **Backend**: ASP.NET Core MVC
- **Veritabanı**: SQL Server LocalDB
- **ORM**: Entity Framework Core
- **Frontend**: HTML5, CSS3, JavaScript, Bootstrap
- **Veri İşleme**: Python (pandas, pyodbc)
- **Web Scraping**: BeautifulSoup, requests

## 📊 Veritabanı Şeması

- **Films**: Film bilgileri
- **Users**: Kullanıcı hesapları
- **UserLists**: Kullanıcı listeleri
- **UserRatings**: Film puanları
- **Categories**: Film kategorileri
- **Directors**: Yönetmenler
- **Actors**: Oyuncular

## 🎯 Kullanım

1. **Kayıt Olun**: Yeni hesap oluşturun
2. **Filmleri Keşfedin**: Ana sayfada filmleri görüntüleyin
3. **Arama Yapın**: Arama çubuğunu kullanın
4. **Puanlayın**: Filmleri 5 yıldızla puanlayın
5. **Liste Oluşturun**: Kişisel film listelerinizi yönetin

## 🔧 Geliştirme

### Yeni Migration Ekleme
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Veritabanını Sıfırlama
```bash
dotnet ef database drop --force
dotnet ef database update
```

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/AmazingFeature`)
3. Commit yapın (`git commit -m 'Add some AmazingFeature'`)
4. Push yapın (`git push origin feature/AmazingFeature`)
5. Pull Request oluşturun

## 📞 İletişim

Proje hakkında sorularınız için issue açabilirsiniz.

---

**MovieHouse** - Film tutkunları için modern platform 🎬 
