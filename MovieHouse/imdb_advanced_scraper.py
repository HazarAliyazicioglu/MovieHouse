import requests
from bs4 import BeautifulSoup
import pyodbc
import time
import re
from urllib.parse import quote_plus
import json

class IMDBAdvancedScraper:
    def __init__(self, connection_string):
        self.connection_string = connection_string
        self.session = requests.Session()
        self.session.headers.update({
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
            'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
            'Accept-Language': 'en-US,en;q=0.5',
            'Accept-Encoding': 'gzip, deflate',
            'Connection': 'keep-alive',
            'Upgrade-Insecure-Requests': '1',
        })
    
    def search_imdb_movie(self, title, year):
        """IMDB'de film arama - gelişmiş versiyon"""
        try:
            # IMDB arama URL'si
            search_query = f"{title} {year}"
            search_url = f"https://www.imdb.com/find?q={quote_plus(search_query)}&s=tt&ttype=ft"
            
            print(f"Aranıyor: {search_query}")
            
            response = self.session.get(search_url, timeout=10)
            response.raise_for_status()
            
            soup = BeautifulSoup(response.content, 'html.parser')
            
            # Film sonuçlarını bul
            result_items = soup.find_all('li', class_='find-result-item')
            
            if result_items:
                for item in result_items[:3]:  # İlk 3 sonucu kontrol et
                    # Film linkini al
                    link_element = item.find('a')
                    if link_element and 'href' in link_element.attrs:
                        movie_url = "https://www.imdb.com" + link_element['href']
                        
                        # Film başlığını kontrol et
                        title_element = item.find('a')
                        if title_element:
                            movie_title = title_element.get_text(strip=True)
                            
                            # Yıl bilgisini kontrol et
                            year_element = item.find('span', class_='find-result-item-year')
                            if year_element:
                                movie_year = year_element.get_text(strip=True)
                                if str(year) in movie_year:
                                    print(f"Eşleşme bulundu: {movie_title} ({movie_year})")
                                    return self.get_movie_poster(movie_url)
            
            # Eşleşme bulunamazsa ilk sonucu dene
            if result_items:
                link_element = result_items[0].find('a')
                if link_element and 'href' in link_element.attrs:
                    movie_url = "https://www.imdb.com" + link_element['href']
                    print(f"Tam eşleşme bulunamadı, ilk sonuç deneniyor...")
                    return self.get_movie_poster(movie_url)
            
            return None
            
        except Exception as e:
            print(f"Arama hatası: {e}")
            return None
    
    def get_movie_poster(self, movie_url):
        """Film sayfasından poster URL'sini al - gelişmiş versiyon"""
        try:
            response = self.session.get(movie_url, timeout=10)
            response.raise_for_status()
            
            soup = BeautifulSoup(response.content, 'html.parser')
            
            # Farklı poster seçicileri dene
            poster_selectors = [
                'img[data-testid="hero-media__poster"]',
                'img.ipc-image',
                'img[class*="poster"]',
                'img[src*="images-amazon.com"]',
                'img[src*="._V1_"]'
            ]
            
            for selector in poster_selectors:
                poster_img = soup.select_one(selector)
                if poster_img and 'src' in poster_img.attrs:
                    poster_url = poster_img['src']
                    
                    # Yüksek kaliteli poster URL'sine dönüştür
                    if '._V1_' in poster_url:
                        # IMDB poster boyutlarını değiştir
                        poster_url = re.sub(r'\._V1_.*\.', '._V1_QL75_UX1000_.', poster_url)
                    
                    print(f"Poster bulundu: {poster_url}")
                    return poster_url
            
            return None
            
        except Exception as e:
            print(f"Poster alma hatası: {e}")
            return None
    
    def update_database_posters(self, limit=None):
        """Veritabanındaki filmlerin posterlerini güncelle"""
        try:
            conn = pyodbc.connect(self.connection_string)
            cursor = conn.cursor()
            
            # Poster URL'si olmayan filmleri al
            query = """
                SELECT Id, Title, Year 
                FROM Films 
                WHERE PosterUrl IS NULL OR PosterUrl = '' OR PosterUrl LIKE '%movieposters.com%'
                ORDER BY Title
            """
            
            if limit:
                query += f" OFFSET 0 ROWS FETCH NEXT {limit} ROWS ONLY"
            
            cursor.execute(query)
            films = cursor.fetchall()
            
            print(f"Toplam {len(films)} film güncellenecek")
            
            updated_count = 0
            failed_count = 0
            
            for i, film in enumerate(films, 1):
                film_id, title, year = film
                
                print(f"\n[{i}/{len(films)}] Film: {title} ({year})")
                
                # IMDB'de ara
                poster_url = self.search_imdb_movie(title, year)
                
                if poster_url:
                    # Veritabanını güncelle
                    cursor.execute("""
                        UPDATE Films 
                        SET PosterUrl = ? 
                        WHERE Id = ?
                    """, (poster_url, film_id))
                    
                    conn.commit()
                    updated_count += 1
                    print(f"Poster güncellendi!")
                else:
                    failed_count += 1
                    print("Poster bulunamadı")
                
                # Rate limiting - IMDB'yi çok hızlı isteklerle yormayalım
                time.sleep(1.5)
            
            print(f"\nİşlem tamamlandı!")
            print(f"Başarılı: {updated_count}")
            print(f"Başarısız: {failed_count}")
            print(f"Toplam: {len(films)}")
            
            conn.close()
            
        except Exception as e:
            print(f"Veritabanı hatası: {e}")
    
    def test_search(self, title, year):
        """Test arama fonksiyonu"""
        print(f"Test: {title} ({year})")
        poster_url = self.search_imdb_movie(title, year)
        if poster_url:
            print(f"Test başarılı: {poster_url}")
        else:
            print("Test başarısız")
        return poster_url
    
    def get_film_stats(self):
        """Veritabanındaki film istatistiklerini al"""
        try:
            conn = pyodbc.connect(self.connection_string)
            cursor = conn.cursor()
            
            # Toplam film sayısı
            cursor.execute("SELECT COUNT(*) FROM Films")
            total_films = cursor.fetchone()[0]
            
            # Poster URL'si olan film sayısı
            cursor.execute("SELECT COUNT(*) FROM Films WHERE PosterUrl IS NOT NULL AND PosterUrl != ''")
            films_with_poster = cursor.fetchone()[0]
            
            # Poster URL'si olmayan film sayısı
            cursor.execute("SELECT COUNT(*) FROM Films WHERE PosterUrl IS NULL OR PosterUrl = ''")
            films_without_poster = cursor.fetchone()[0]
            
            conn.close()
            
            return {
                'total': total_films,
                'with_poster': films_with_poster,
                'without_poster': films_without_poster,
                'percentage': round((films_with_poster / total_films) * 100, 2) if total_films > 0 else 0
            }
            
        except Exception as e:
            print(f"İstatistik hatası: {e}")
            return None

def main():
    # SQL Server bağlantı bilgileri
    connection_string = (
        "DRIVER={ODBC Driver 17 for SQL Server};"
        "SERVER=(localdb)\\mssqllocaldb;"
        "DATABASE=MovieHouseDb;"
        "Trusted_Connection=yes;"
    )
    
    scraper = IMDBAdvancedScraper(connection_string)
    
    print("IMDB Advanced Poster Scraper")
    print("=" * 60)
    
    # İstatistikleri göster
    stats = scraper.get_film_stats()
    if stats:
        print(f"\nVeritabanı İstatistikleri:")
        print(f"   Toplam Film: {stats['total']}")
        print(f"   Posterli Film: {stats['with_poster']}")
        print(f"   Postersiz Film: {stats['without_poster']}")
        print(f"   Poster Oranı: %{stats['percentage']}")
    
    # Test arama
    print(f"\nTest arama yapılıyor...")
    test_poster = scraper.test_search("The Shawshank Redemption", 1994)
    
    if test_poster:
        print(f"\nTest başarılı! Seçenekler:")
        print(f"1. Tüm filmleri güncelle")
        print(f"2. İlk 10 filmi güncelle")
        print(f"3. İlk 20 filmi güncelle")
        print(f"4. Çıkış")
        
        choice = input(f"\nSeçiminiz (1-4): ")
        
        if choice == '1':
            scraper.update_database_posters()
        elif choice == '2':
            scraper.update_database_posters(limit=10)
        elif choice == '3':
            scraper.update_database_posters(limit=20)
        elif choice == '4':
            print("İşlem iptal edildi.")
        else:
            print("Geçersiz seçim!")
    else:
        print("Test başarısız! Lütfen internet bağlantınızı kontrol edin.")

if __name__ == "__main__":
    main() 