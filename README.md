# Water Sort Puzzle - Hyper Casual Mobile Game

![Unity](https://img.shields.io/badge/Unity-2021.3+-black.svg?style=flat&logo=unity)
![Platform](https://img.shields.io/badge/Platform-Android%20%7C%20iOS-blue.svg)
![Language](https://img.shields.io/badge/Language-C%23-green.svg)

**Water Sort Puzzle**, tek parmakla oynanan, reklam geliri odaklı hyper-casual bir mobil oyun projesidir. Oyuncular farklı renklerdeki sıvıları doğru tüplere sıralayarak bulmacaları çözer.

## 📋 İçindekiler

- [Özellikler](#-özellikler)
- [Kurulum](#-kurulum)
- [Proje Yapısı](#-proje-yapısı)
- [Reklam Entegrasyonu](#-reklam-entegrasyonu)
- [Level Sistemi](#-level-sistemi)
- [Monetization Stratejisi](#-monetization-stratejisi)
- [Test Etme](#-test-etme)
- [Build Alma](#-build-alma)
- [Yayın Öncesi Yapılacaklar](#-yayın-öncesi-yapılacaklar)

---

## ✨ Özellikler

### Oynanış
- **Tek parmak kontrolü** - Dokun, seç, dök
- **1000+ seviye** - Prosedürel üretim; her seviye çözücü ile doğrulanır, her açılışta aynı gelir
- **Zorluk eğrisi** - 2 renkten 12 renge kademeli geçiş
- **Geri alma** - Seviye başına 5 hak, sonrası ödüllü reklamla
- **İpucu** - Çözücü bir sonraki doğru hamleyi gösterir
- **Ekstra tüp** - Zor seviyelerde ödüllü reklamla boş tüp
- **Seviye seçme, ayarlar, günlük ödül**
- **Türkçe ve İngilizce** - Cihaz diline göre otomatik, ayarlardan değiştirilebilir

### Monetization (Para Kazanma)
- **Interstitial** - Her 2 seviyede bir, en az 30 sn arayla (Remote Config ile ayarlanır)
- **Rewarded** - Geri alma hakkı, ekstra tüp, ipucu, seviye atlama, günlük ödül
- **Banner** - Ekranın altında
- **Reklamları Kaldır (IAP)** - Interstitial ve banner kalkar, ödüllü reklamlar isteğe bağlı kalır

### Teknik Özellikler
- **Tek tıkla kurulum** - `Water Sort > Projeyi Kur` sahneleri, prefabları ve UI'ı oluşturur
- **Şifreli kayıt** - AES-256 + bütünlük kontrolü, eski kayıtlar otomatik taşınır
- **SDK'sız çalışır** - AdMob, Firebase ve IAP kurulu değilse test (mock) modunda çalışır
- **Harici animasyon paketi gerekmez** - Yerleşik `SimpleTween`
- **Testler** - Level/çözücü/şifreleme/çeviri testleri ve Unity'siz derleme kontrolü (`Tests/`)

---

## 🚀 Kurulum

### Gereksinimler

- **Unity 2021.3 LTS** (veya daha yeni)
- **Android Build Support** (Android için) / **iOS Build Support** + Xcode (iOS için)

### Adım 1: Projeyi Aç

1. Unity Hub > "Add" > "Add project from disk" > bu klasörü seç
2. Unity ilk açılışta paketleri (TextMeshPro, UI) kendisi indirir

### Adım 2: Projeyi Kur (tek tık)

Menüden **Water Sort > Projeyi Kur (Sahneler + Prefablar)** seç.

- İlk çalıştırmada TextMeshPro Essential Resources'ı içe aktarır; bitince menüyü **bir kez daha** çalıştır.
- Oluşturulanlar:
  - `Assets/Scenes/` - Bootstrap, MainMenu, Game (Build Settings'e eklenir)
  - `Assets/Prefabs/Tube.prefab`, `Assets/Prefabs/UI/LevelButton.prefab`
  - `Assets/Resources/Prefabs/LiquidSegment.prefab`
  - `Assets/Art/Generated/` - tüp ve sıvı sprite'ları
- Tüm UI referansları bağlanır; elle sürükle-bırak gerekmez.

### Adım 3: Oyna

**Water Sort > Oyunu Başlat** (Bootstrap sahnesini açıp Play'e basar).

Reklamlar ve satın alma bu aşamada test modunda çalışır (ekranda "Test reklamı oynatılıyor..." görünür).

### Adım 4 (Opsiyonel): SDK'lar

Her SDK bağımsızdır. Kurduktan sonra **Water Sort > SDK Tanımlarını Güncelle** çalıştır; araç kurulu SDK'yı algılar ve ilgili kodu açar (`WATERSORT_ADMOB`, `WATERSORT_FIREBASE`, `WATERSORT_IAP`).

| SDK | Nasıl kurulur | Senin yapman gereken |
|-----|---------------|----------------------|
| AdMob (Google Mobile Ads Unity 8.0+) | [Releases](https://github.com/googleads/googleads-mobile-unity/releases) sayfasından `.unitypackage` içe aktar | AdMob hesabı, App ID (`Assets > Google Mobile Ads > Settings`), `AdsManager` üzerindeki ad unit ID'leri |
| Firebase (Analytics + Remote Config) | Firebase Unity SDK'dan `FirebaseAnalytics` ve `FirebaseRemoteConfig` paketleri | Firebase projesi, `google-services.json` / `GoogleService-Info.plist` dosyalarını `Assets/` içine koy |
| Unity IAP 4.x | Package Manager > In-App Purchasing | Google Play Console / App Store Connect'te `com.yourcompany.watersort.noads` (veya `IAPManager.NoAdsProductId`) ürünü |

---

## 📁 Proje Yapısı

```
Assets/
├── Editor/
│   ├── ProjectSetupWizard.cs       # Water Sort menüsü: kurulum, oyunu başlat, SDK tanımları
│   └── BuildOptimizer.cs           # Build ayarları optimizasyonu
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs          # Oyun durumu, sahne geçişi, ses/müzik
│   │   ├── LevelManager.cs         # Level yükleme, hamle, geri alma, kazanma, kamera
│   │   ├── InputManager.cs         # Dokunma/tıklama
│   │   ├── SimpleTween.cs          # Yerleşik animasyon motoru
│   │   ├── ObjectPoolManager.cs    # Nesne havuzu
│   │   ├── PerformanceManager.cs   # FPS/kalite ayarı
│   │   ├── SecurityManager.cs      # Şifreleme, anti-cheat
│   │   └── DeviceCompatibilityManager.cs
│   ├── Gameplay/
│   │   ├── TubeController.cs       # Tüp mantığı ve animasyonları
│   │   ├── LevelData.cs            # Level veri yapısı (JSON)
│   │   ├── LevelDataGenerator.cs   # Prosedürel level üretimi
│   │   ├── LevelSolver.cs          # Çözücü (doğrulama + ipucu)
│   │   ├── LiquidColor.cs          # Renkler
│   │   └── MoveValidator.cs        # Hamle kuralları
│   ├── Localization/               # Türkçe/İngilizce metinler (Loc, LocalizedText)
│   ├── UI/                         # UIManager, MainMenuUI, Toast, SafeAreaPanel
│   ├── Services/                   # Ads, Reward, IAP, Analytics, RemoteConfig, FirebaseInit
│   └── Save/                       # SaveManager (şifreli), SaveCrypto
└── Resources/
    └── Levels/                     # Elle tasarlanmış seviyeler (Level_0..2.json)
Tests/
├── LogicTests/                     # dotnet run: level, çözücü, şifreleme, çeviri testleri
├── CompileCheck/                   # dotnet build: tüm scriptler Unity 2021.3 API'sine karşı
└── EditorCompileCheck/             # dotnet build: editör scriptleri dahil
```

---

## 📱 Reklam Entegrasyonu

### AdMob Kurulumu

1. [AdMob Console](https://apps.admob.com/)'da uygulama ve 3 reklam birimi oluştur (Interstitial, Rewarded, Banner)
2. Google Mobile Ads Unity eklentisini içe aktar, `Assets > Google Mobile Ads > Settings`'e App ID'leri gir
3. **Water Sort > SDK Tanımlarını Güncelle** çalıştır
4. `AdsManager.cs` içindeki varsayılan reklam birimi ID'lerini kendi ID'lerinle değiştir (Android ve iOS ayrı alanlar)

### Test Reklamları

Koddaki varsayılan ID'ler Google'ın **test** ID'leridir. Yayından önce kendi ID'lerinle değiştir.

### Reklam Stratejisi

**Interstitial:** Her 2 seviyede bir, en az 30 sn arayla (`interstitial_frequency`, `min_time_between_interstitials`)

**Rewarded:**
- Geri alma: +3 hak
- Ekstra tüp: +1 boş tüp
- İpucu: bir sonraki doğru hamle (çözüm yoksa reklam gösterilmez)
- Günlük ödül: +50 coin

**Banner:** Ekranın altında; "Reklamları Kaldır" satın alınınca kalkar.

---

## 🎮 Level Sistemi

### Prosedürel Level Üretimi

Oyun, `LevelDataGenerator.cs` kullanarak otomatik olarak seviyeler üretir:

```csharp
// Zorluk eğrisi
Level 0-4:    2 renk, 4-5 tüp
Level 5-9:    3 renk, 5-6 tüp
Level 10-19:  4 renk, 6-7 tüp
Level 20-39:  5 renk, 7-8 tüp
// ...ve devam eder
```

### Elle Tasarlanmış Seviyeler

`Assets/Resources/Levels/` klasörüne JSON dosyaları ekleyerek özel seviyeler oluşturabilirsiniz:

```json
{
  "LevelIndex": 0,
  "TubeCount": 4,
  "ColorCount": 2,
  "TubeCapacity": 4,
  "TubeConfigurations": [
    { "Colors": [1, 2, 1, 2] },
    { "Colors": [2, 1, 2, 1] },
    { "Colors": [] },
    { "Colors": [] }
  ]
}
```

Her tüp alttan üste renk listesidir. Her renk tam `TubeCapacity` kez geçmelidir; geçersiz dosyalar yerine prosedürel seviye üretilir.

**Renk Kodları:**
- 0: None (Boş)
- 1: Red (Kırmızı)
- 2: Blue (Mavi)
- 3: Green (Yeşil)
- 4: Yellow (Sarı)
- 5: Purple (Mor)
- 6: Orange (Turuncu)
- 7: Pink (Pembe)
- 8: Cyan (Camgöbeği)
- 9: Brown (Kahverengi)
- 10: Gray (Gri)
- 11: Lime (Limon Yeşili)
- 12: Teal (Deniz Mavisi)

### Yeni Seviye Ekleme

1. `Assets/Resources/Levels/Level_X.json` dosyası oluştur
2. JSON formatında level verisini yaz
3. Oyun otomatik olarak bu seviyeyi yükleyecek
4. Eğer JSON dosyası yoksa prosedürel üretim devreye girer
5. `dotnet run --project Tests/LogicTests` ile seviyenin çözülebilir olduğunu doğrula

---

## 💰 Monetization Stratejisi

### Gelir Maksimizasyonu

Bu oyun **reklam geliri** odaklı tasarlanmıştır:

**Interstitial Ads (Ana gelir kaynağı):**
- Her 2-3 seviyede gösteriliyor
- Oyuncular hızlı level bitirdiğinden yüksek impression
- Remote Config ile frekans optimize edilebilir

**Rewarded Ads (Engagement artırıcı):**
- Oyuncuya fayda sağlar (undo, extra tube, skip)
- Completion rate yüksek
- eCPM genelde interstitial'dan yüksek

**Banner Ads (Pasif gelir):**
- Sürekli görünür
- Oyun oynandığı sürece impression
- Düşük eCPM ama stabil

### A/B Testing

Remote Config kullanarak test edebilirsiniz:

```csharp
// Farklı kullanıcı gruplarına farklı ayarlar
Group A: interstitial_frequency = 2
Group B: interstitial_frequency = 3

// En yüksek retention + revenue kombinasyonunu bul
```

### Retention Stratejileri

- **İlk 5 seviye çok kolay** - Oyuncuyu alıştır
- **Günlük ödül** - Her gün geri dönme nedeni
- **Progression sistemi** - Level sayısı oyuncuya ilerleme hissi verir
- **Tatmin edici animasyonlar** - Juicy gameplay

---

## 🧪 Test Etme

### Unity Editor'de Test

1. **Water Sort > Oyunu Başlat**
2. Reklamlar test modunda: ekranda "Test reklamı oynatılıyor..." mesajı, konsolda `[MOCK]` logları

### Unity Olmadan Test (.NET 8 SDK)

```bash
dotnet run --project Tests/LogicTests                 # level, çözücü, şifreleme, çeviri testleri
dotnet build Tests/CompileCheck                       # tüm scriptler Unity 2021.3 API'sine karşı
dotnet build Tests/CompileCheck -p:WithSdks=true      # AdMob/Firebase/IAP kod yolları dahil
dotnet build Tests/EditorCompileCheck                 # editör scriptleri dahil
```

### Test Checklist

- [ ] Bootstrap'tan ana menü açılıyor mu?
- [ ] Tüp seçme ve dökme çalışıyor mu? Geçersiz hamlede tüp sallanıyor mu?
- [ ] Level bitince popup geliyor, sonraki level oynanabiliyor mu?
- [ ] Geri al, yeniden başla, ipucu, ekstra tüp çalışıyor mu?
- [ ] Dil değiştirince tüm yazılar değişiyor mu?
- [ ] Oyunu kapatıp açınca ilerleme ve coinler duruyor mu?
- [ ] Çentikli (notch) telefonda UI kesilmiyor mu?

---

## 📦 Build Alma

### Android Build

1. **Build Settings:**
   - File > Build Settings
   - Platform: Android
   - Switch Platform

2. **Player Settings:**
   - Company Name: Şirket adınız
   - Product Name: Water Sort Puzzle
   - Package Name: `com.yourcompany.watersort`
   - Version: 1.0
   - Bundle Version Code: 1
   - Minimum API Level: 22 (Android 5.1)
   - Target API Level: 33 (Android 13)

3. **Keystore oluştur** (Release build için):
   - Publishing Settings > Create New Keystore

4. **Build:**
   - Build (APK) veya Build App Bundle (AAB - Google Play için)

### iOS Build

1. **Build Settings:**
   - Platform: iOS
   - Switch Platform

2. **Player Settings:**
   - Bundle Identifier: `com.yourcompany.watersort`
   - Target minimum iOS Version: 11.0
   - Camera Usage Description: (Gerekirse)

3. **Build:**
   - Build > Xcode projesi oluşturulur
   - Xcode'da proje aç
   - Signing ayarlarını yap
   - Archive > Distribute

---

## 📝 Yayın Öncesi Yapılacaklar

Kod tarafı hazır. Kalanlar hesap, içerik ve test işleri:

- [ ] Unity'de **Water Sort > Projeyi Kur** çalıştır ve gerçek cihazda test et
- [ ] AdMob hesabı + gerçek reklam ID'leri
- [ ] (Opsiyonel) Firebase projesi ve yapılandırma dosyaları
- [ ] (Opsiyonel) Unity IAP + mağazada "Reklamları Kaldır" ürünü
- [ ] Ses efektleri ve müzik (`InputManager` ve `GameManager` üzerindeki AudioClip alanları)
- [ ] Uygulama ikonu, mağaza görselleri, şirket adı ve paket adı (`com.yourcompany.watersort`)
- [ ] Gizlilik politikası (reklam ve analitik için mağazalar zorunlu tutar)

### Sonraki Fikirler

- [ ] Level bitince konfeti efekti
- [ ] Cloud save
- [ ] Günlük görevler, tema sistemi
- [ ] "Bizi değerlendir" penceresi

---

## 🎨 Görsel Tasarım Önerileri

### Tüp Sprite
- Basit cam tüp görseli
- Şeffaf PNG
- Boyut: 128x256 pixels

### Sıvı Segment
- Düz renkli kare/dikdörtgen
- Hafif gradient (üstte açık, altta koyu)

### Renkler
- Parlak, canlı, kontrast yüksek renkler
- Renk körlüğü için: Her renk tonsal olarak farklı

### UI
- Büyük, dokunulabilir butonlar (minimum 60x60 pixel)
- Sans-serif font
- Minimal, temiz tasarım

---

## 🤝 Katkıda Bulunma

Bu proje bir template/starter kit olarak tasarlanmıştır. Geliştirmek için:

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit edin (`git commit -m 'Add amazing feature'`)
4. Push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın

---

## 📄 Lisans

Bu proje açık kaynak bir starter kit olarak sunulmaktadır. İsterseniz ticari projelerde kullanabilirsiniz.

---

## 📞 Destek

Sorularınız için:
- GitHub Issues kullanın
- Dokümantasyonu okuyun
- Unity forumlarına başvurun

---

## 🎯 Başarı Metrikleri (KPI)

Oyununuzu optimize etmek için takip etmeniz gerekenler:

**Retention:**
- D1 (1 günlük): %40+
- D7 (7 günlük): %15+
- D30 (30 günlük): %5+

**Monetization:**
- ARPDAU (Kullanıcı başı günlük gelir): $0.05 - $0.15
- Ad Impressions/DAU: 10-20
- Rewarded Ad Completion Rate: %80+

**Engagement:**
- Avg. Session Length: 5-10 dakika
- Avg. Sessions/Day: 2-3
- Avg. Levels Completed/Session: 3-5

---

**Başarılar! 🎉**

Hyper-casual oyun geliştirmede başarılar dilerim. Oyununuz milyonlarca kullanıcıya ulaşsın!
