import pandas as pd
import pyodbc
import re
from datetime import datetime

# Veritabanı bilgileri
connection_string = (
    "DRIVER={ODBC Driver 17 for SQL Server};"
    "SERVER=(localdb)\\MSSQLLocalDB;"
    "DATABASE=MovieHouseDb;"
    "Trusted_Connection=yes;"
)

def clean_text(text):
    """Metni temizle ve None değerleri kontrol et"""
    if pd.isna(text) or text == '':
        return None
    return str(text).strip()

def extract_year(year_str):
    """Yıl bilgisini çıkar"""
    if pd.isna(year_str):
        return None
    try:
        return int(str(year_str).strip())
    except:
        return None

def extract_rating(rating_str):
    """IMDB puanını çıkar"""
    if pd.isna(rating_str):
        return 0.0
    try:
        return float(str(rating_str).strip())
    except:
        return 0.0



def main():
    print("Film Verilerini Yükleme Başlıyor...")
    
    try:
        # CSV dosyasını oku
        print("CSV dosyası okunuyor...")
        df = pd.read_csv('Datasets2/imdb_top_1000.csv')
        print(f"{len(df)} film bulundu")
        
        # Veritabanına bağlan
        print("Veritabanına bağlanılıyor...")
        conn = pyodbc.connect(connection_string)
        cursor = conn.cursor()
        
        # Mevcut verileri temizle
        print("Mevcut veriler temizleniyor...")
        cursor.execute("DELETE FROM FilmActors")
        cursor.execute("DELETE FROM FilmCategories")
        cursor.execute("DELETE FROM Films")
        cursor.execute("DELETE FROM Actors")
        cursor.execute("DELETE FROM Directors")
        cursor.execute("DELETE FROM Categories")
        conn.commit()
        
        # Kategorileri işle
        print("Kategoriler işleniyor...")
        all_genres = set()
        for genre_str in df['Genre'].dropna():
            genres = [g.strip() for g in str(genre_str).split(',')]
            all_genres.update(genres)
        
        # Kategorileri veritabanına ekle
        category_ids = {}
        for genre in sorted(all_genres):
            cursor.execute(
                "INSERT INTO Categories (Name) VALUES (?)",
                (genre,)
            )
            category_ids[genre] = cursor.execute("SELECT @@IDENTITY").fetchone()[0]
        
        print(f"{len(all_genres)} kategori eklendi")
        
        # Yönetmenleri işle
        print("Yönetmenler işleniyor...")
        director_ids = {}
        for director_name in df['Director'].dropna().unique():
            director_name = clean_text(director_name)
            if director_name:
                cursor.execute(
                    "INSERT INTO Directors (Name) VALUES (?)",
                    (director_name,)
                )
                director_ids[director_name] = cursor.execute("SELECT @@IDENTITY").fetchone()[0]
        
        print(f"{len(director_ids)} yönetmen eklendi")
        
        # Oyuncuları işle
        print("Oyuncular işleniyor...")
        actor_ids = {}
        star_columns = ['Star1', 'Star2', 'Star3', 'Star4']
        
        for col in star_columns:
            for actor_name in df[col].dropna().unique():
                actor_name = clean_text(actor_name)
                if actor_name and actor_name not in actor_ids:
                    cursor.execute(
                        "INSERT INTO Actors (Name) VALUES (?)",
                        (actor_name,)
                    )
                    actor_ids[actor_name] = cursor.execute("SELECT @@IDENTITY").fetchone()[0]
        
        print(f"{len(actor_ids)} oyuncu eklendi")
        
        # Filmleri işle
        print("Filmler işleniyor...")
        film_count = 0
        
        for index, row in df.iterrows():
            try:
                # Film bilgilerini hazırla
                title = clean_text(row['Series_Title'])
                year = extract_year(row['Released_Year'])
                runtime = clean_text(row['Runtime'])
                genre = clean_text(row['Genre'])
                rating = extract_rating(row['IMDB_Rating'])
                overview = clean_text(row['Overview'])
                poster_url = clean_text(row['Poster_Link'])
                director_name = clean_text(row['Director'])
                # Null kontrolleri
                if not title or not year:
                    print(f"Satır {index + 1} atlandı: Başlık veya yıl eksik")
                    continue
                
                # Yönetmen ID'sini al
                director_id = director_ids.get(director_name, 1)  # Varsayılan 1
                
                # Film ekle
                cursor.execute("""
                    INSERT INTO Films (Title, Year, Runtime, Genre, Rating, Description, PosterUrl, DirectorId)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?)
                """, (title, year, runtime, genre, rating, overview, poster_url, director_id))
                
                film_id = cursor.execute("SELECT @@IDENTITY").fetchone()[0]
                
                # Film-Kategori ilişkilerini ekle
                if genre:
                    genres = [g.strip() for g in genre.split(',')]
                    for g in genres:
                        if g in category_ids:
                            cursor.execute(
                                "INSERT INTO FilmCategories (FilmId, CategoryId) VALUES (?, ?)",
                                (film_id, category_ids[g])
                            )
                
                # Film-Oyuncu ilişkilerini ekle
                for col in star_columns:
                    actor_name = clean_text(row[col])
                    if actor_name and actor_name in actor_ids:
                        cursor.execute(
                            "INSERT INTO FilmActors (FilmId, ActorId) VALUES (?, ?)",
                            (film_id, actor_ids[actor_name])
                        )
                
                film_count += 1
                if film_count % 50 == 0:
                    print(f"{film_count} film işlendi...")
                    conn.commit()
                    
            except Exception as e:
                print(f"Satır {index + 1} hatası: {str(e)}")
                continue
        
        # Son commit
        conn.commit()
        
        print(f"Toplam {film_count} film başarıyla yüklendi!")
        
        # İstatistikler
        cursor.execute("SELECT COUNT(*) FROM Films")
        total_films = cursor.fetchone()[0]
        
        cursor.execute("SELECT COUNT(*) FROM Directors")
        total_directors = cursor.fetchone()[0]
        
        cursor.execute("SELECT COUNT(*) FROM Actors")
        total_actors = cursor.fetchone()[0]
        
        cursor.execute("SELECT COUNT(*) FROM Categories")
        total_categories = cursor.fetchone()[0]
        
        print("\nİstatistikler:")
        print(f"   Filmler: {total_films}")
        print(f"   Yönetmenler: {total_directors}")
        print(f"   Oyuncular: {total_actors}")
        print(f"   Kategoriler: {total_categories}")
        
        conn.close()
        print("\nVeri yükleme tamamlandı!")
        
    except Exception as e:
        print(f"Hata: {str(e)}")
        if 'conn' in locals():
            conn.close()

if __name__ == "__main__":
    main() 