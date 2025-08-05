import requests
from bs4 import BeautifulSoup
import pyodbc
import time
import re
from urllib.parse import quote_plus

class IMDBAllPosterUpdater:
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
        """IMDB'de film arama"""
        try:
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
        """Film sayfasından poster URL'sini al"""
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
                        poster_url = re.sub(r'\._V1_.*\.', '._V1_QL75_UX1000_.', poster_url)
                    
                    print(f"Poster bulundu: {poster_url}")
                    return poster_url
            
            return None
            
        except Exception as e:
            print(f"Poster alma hatası: {e}")
            return None
    
    def update_all_posters(self):
        """Tüm filmlerin posterlerini güncelle"""
        try:
            conn = pyodbc.connect(self.connection_string)
            cursor = conn.cursor()
            
            # Tüm filmleri al
            cursor.execute("""
                SELECT Id, Title, Year 
                FROM Films 
                ORDER BY Title
            """)
            
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
                
                # Rate limiting
                time.sleep(1.5)
                
                # Her 50 filmde bir ilerleme raporu
                if i % 50 == 0:
                    print(f"\nİlerleme: {i}/{len(films)} (%{(i/len(films)*100):.1f})")
                    print(f"Başarılı: {updated_count}, Başarısız: {failed_count}")
            
            print(f"\nİşlem tamamlandı!")
            print(f"Başarılı: {updated_count}")
            print(f"Başarısız: {failed_count}")
            print(f"Toplam: {len(films)}")
            
            conn.close()
            
        except Exception as e:
            print(f"Veritabanı hatası: {e}")
    
    def get_film_stats(self):
        """Veritabanındaki film istatistiklerini al"""
        try:
            conn = pyodbc.connect(self.connection_string)
            cursor = conn.cursor()
            
            cursor.execute("SELECT COUNT(*) FROM Films")
            total_films = cursor.fetchone()[0]
            
            cursor.execute("SELECT COUNT(*) FROM Films WHERE PosterUrl IS NOT NULL AND PosterUrl != ''")
            films_with_poster = cursor.fetchone()[0]
            
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
    
    updater = IMDBAllPosterUpdater(connection_string)
    
    print("IMDB Tüm Poster Güncelleme")
    print("=" * 60)
    
    # İstatistikleri göster
    stats = updater.get_film_stats()
    if stats:
        print(f"\nMevcut Durum:")
        print(f"   Toplam Film: {stats['total']}")
        print(f"   Posterli Film: {stats['with_poster']}")
        print(f"   Postersiz Film: {stats['without_poster']}")
        print(f"   Poster Oranı: %{stats['percentage']}")
    
    print(f"\nDİKKAT: Bu işlem tüm filmlerin posterlerini yeniden güncelleyecek!")
    print(f"   Tahmini süre: {stats['total'] * 1.5 / 60:.1f} dakika")
    
    response = input(f"\nDevam etmek istiyor musunuz? (y/n): ")
    
    if response.lower() == 'y':
        print(f"\nTüm posterler güncelleniyor...")
        updater.update_all_posters()
        
        # Son istatistikleri göster
        final_stats = updater.get_film_stats()
        if final_stats:
            print(f"\nGüncelleme Sonrası:")
            print(f"   Toplam Film: {final_stats['total']}")
            print(f"   Posterli Film: {final_stats['with_poster']}")
            print(f"   Postersiz Film: {final_stats['without_poster']}")
            print(f"   Poster Oranı: %{final_stats['percentage']}")
    else:
        print("İşlem iptal edildi.")

if __name__ == "__main__":
    main() 