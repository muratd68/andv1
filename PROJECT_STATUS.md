# Water Sort Puzzle - Proje Durumu

## Özet

Kod tarafı tamamlandı ve Unity olmadan yapılabilecek her yolla doğrulandı. Sahneler, prefablar ve UI tek bir editör menüsüyle oluşturuluyor. Kalan işler hesap açma, içerik (ses, ikon) ve gerçek cihazda test.

## ✅ Tamamlananlar

### Oynanış
- Tüp seçme/dökme, geçersiz hamlede sallanma, dökme sırasında giriş kilidi
- Seviye bitişi, sonraki seviye, yeniden başlatma, seviye atlama (kaydedilir)
- Geri alma (seviye başına 5 hak, sonra ödüllü reklam)
- İpucu: çözücü bir sonraki doğru hamleyi gösterir; çözüm yoksa reklam gösterilmez
- Ekstra tüp, tüpler otomatik ortalanır ve kamera tüm tüpleri sığdırır
- 3 elle tasarlanmış seviye (JSON) + 1000+ prosedürel seviye; hepsi çözücü ile doğrulanır ve her açılışta aynı gelir

### Arayüz
- Ana menü: Oyna (yeni oyun), Devam Et, Seviye Seç, Günlük Ödül, Ayarlar, Reklamları Kaldır, (iOS) Satın Alımları Geri Yükle
- Oyun ekranı: seviye, hamle, geri alma hakkı, coin; Geri Al / Yeniden / İpucu / + Tüp
- Ayarlar: ses efektleri, müzik, titreşim, dil
- Türkçe ve İngilizce (cihaz diline göre varsayılan)
- Çentikli ekranlar için güvenli alan desteği

### Para kazanma ve servisler
- AdMob interstitial / rewarded / banner (SDK kurulunca açılır, yoksa test modu)
- Reklamları Kaldır satın alması (Unity IAP 4.x)
- Firebase Analytics + Remote Config (SDK kurulunca açılır)
- Reklam sıklığı ve banner Remote Config ile ayarlanabilir

### Altyapı
- Şifreli kayıt (AES-256, rastgele IV, bütünlük kontrolü), eski kayıtlardan otomatik taşıma
- Yerleşik animasyon motoru (DOTween gerekmez)
- Nesne havuzu, performans ve cihaz uyumluluk yöneticileri
- Tek tıkla kurulum aracı: `Water Sort > Projeyi Kur`
- SDK algılama: `Water Sort > SDK Tanımlarını Güncelle`

### Doğrulama (Unity olmadan)
- `Tests/LogicTests`: 3 elle tasarlanmış + 997 üretilmiş seviye çözülebilir ve tekrarlanabilir; çözücü kuralları; şifreleme; her çeviri anahtarının iki dilde olması
- `Tests/CompileCheck`: tüm scriptler Unity 2021.3 API'sine karşı derleniyor (SDK'lı ve SDK'sız)
- `Tests/EditorCompileCheck`: editör scriptleri dahil derleniyor

## ⏳ Kalanlar (senin yapman gerekenler)

| İş | Neden kodla yapılamıyor |
|----|-------------------------|
| Unity'de `Water Sort > Projeyi Kur` çalıştırıp oynamak | Unity Editor gerekiyor |
| Gerçek Android/iOS cihazda test | Cihaz gerekiyor |
| AdMob hesabı, App ID, reklam birimi ID'leri | Senin hesabın |
| Firebase projesi ve yapılandırma dosyaları | Senin hesabın |
| Mağazada "Reklamları Kaldır" ürünü | Google Play / App Store hesabın |
| Ses efektleri, müzik, uygulama ikonu | İçerik |
| Şirket adı, paket adı (`com.yourcompany.watersort`) | Senin kararın |
| Gizlilik politikası sayfası | Mağazalar reklam/analitik için zorunlu tutar |

## Bilinen Sınırlar

- Unity içinde henüz hiç çalıştırılmadı; derleme ve mantık Unity dışında doğrulandı. İlk açılışta küçük UI ayarları (yazı boyutu, konum) gerekebilir.
- SDK kod yolları gerçek SDK'larla değil, aynı API'yi taklit eden dosyalarla derlendi. Desteklenen sürümler: Google Mobile Ads 8.0+, Unity IAP 4.x, güncel Firebase Unity SDK.
- Seviye 4 ve sonrası her cihazda aynıdır, ama web önizlemesindeki seviyelerle aynı değildir (farklı rastgele sayı üreteci).
