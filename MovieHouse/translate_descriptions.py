import pyodbc
import time
from googletrans import Translator
import re

# Veritabanı bağlantısı
def connect_to_database():
    conn_str = (
        "DRIVER={ODBC Driver 17 for SQL Server};"
        "SERVER=(localdb)\\MSSQLLocalDB;"
        "DATABASE=MovieHouseDb;"
        "Trusted_Connection=yes;"
    )
    return pyodbc.connect(conn_str)

# Film açıklamalarını çevir
def translate_descriptions():
    conn = connect_to_database()
    cursor = conn.cursor()
    translator = Translator()
    
    # Tüm filmleri al
    cursor.execute("SELECT Id, Title, Description FROM Films WHERE Description IS NOT NULL AND Description != ''")
    films = cursor.fetchall()
    
    print(f"Toplam {len(films)} film bulundu. Çeviri işlemi başlıyor...")
    
    translated_count = 0
    error_count = 0
    
    for i, film in enumerate(films, 1):
        film_id, title, description = film
        
        try:
            # Açıklamayı temizle (HTML etiketleri varsa)
            clean_description = re.sub(r'<[^>]+>', '', description)
            clean_description = clean_description.strip()
            
            if not clean_description:
                print(f"{i}. '{title}' - Açıklama boş, atlanıyor")
                continue
            
            print(f"{i}. '{title}' çevriliyor...")
            
            # Çeviri yap
            translated = translator.translate(clean_description, dest='tr')
            turkish_description = translated.text
            
            # Veritabanını güncelle
            cursor.execute(
                "UPDATE Films SET Description = ? WHERE Id = ?",
                (turkish_description, film_id)
            )
            
            translated_count += 1
            print(f"   Çevrildi: {turkish_description[:100]}...")
            
            # Rate limiting - API'yi yormamak için (0.5 saniye)
            time.sleep(0.5)
            
        except Exception as e:
            error_count += 1
            print(f"   Hata: {e}")
            continue
    
    conn.commit()
    conn.close()
    
    print(f"\nÇeviri tamamlandı!")
    print(f"Başarıyla çevrilen: {translated_count}")
    print(f"Hata alan: {error_count}")

# Test çevirisi
def test_translation():
    translator = Translator()
    test_text = "A young boy discovers a magical world and embarks on an adventure."
    
    try:
        translated = translator.translate(test_text, dest='tr')
        print(f"Test çevirisi:")
        print(f"Orijinal: {test_text}")
        print(f"Türkçe: {translated.text}")
        return True
    except Exception as e:
        print(f"Test çevirisi başarısız: {e}")
        return False

if __name__ == "__main__":
    print("Film Açıklamaları Türkçe Çeviri Aracı")
    print("=" * 50)
    
    # Test çevirisi yap
    if test_translation():
        print("\nTest başarılı! Ana çeviri işlemi başlıyor...")
        translate_descriptions()
    else:
        print("Test başarısız! Lütfen internet bağlantınızı kontrol edin.") 