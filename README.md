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
- [TODO / Geliştirme Planı](#-todo--geliştirme-planı)

---

## ✨ Özellikler

### Oynanış
- **Tek parmak kontrolü** - Tap to pour mekanikli basit kontrol
- **1000+ seviye** - Prosedürel seviye üretimi ile sonsuz içerik
- **Zorluk eğrisi** - Kolay başlangıçtan zor seviyelere kademeli geçiş
- **Undo sistemi** - Geri alma haklarıyla kullanıcı dostu deneyim
- **Extra tüp** - Zor seviyelerde yardımcı mekanikler

### Monetization (Para Kazanma)
- **Interstitial Ads** - Her 2-3 seviyede bir gösterilen tam ekran reklamlar
- **Rewarded Ads** - Ödül karşılığı izlenen reklamlar:
  - Geri alma hakkı kazanma
  - Ekstra tüp ekleme
  - Seviye atlama
  - Günlük ödül alma
- **Banner Ads** - Oyun ekranı altında sürekli banner
- **IAP Ready** - Reklamsız sürüm satın alma altyapısı (TODO)

### Teknik Özellikler
- **Modüler mimari** - Kolay geliştirme ve bakım
- **Save sistem** - PlayerPrefs tabanlı (cloud save için genişletilebilir)
- **Analytics** - Firebase Analytics entegrasyonu için hazır (TODO: Firebase SDK)
- **Remote Config** - A/B test ve canlı ayar değişikliği desteği (TODO: Firebase SDK)
- **DOTween** - Akıcı animasyonlar için (TODO: Import package)

---

## 🚀 Kurulum

### Gereksinimler

- **Unity 2021.3** veya üzeri (LTS sürümü önerilir)
- **Android SDK** (Android build için)
- **Xcode** (iOS build için - sadece macOS)

### Adım 1: Projeyi Aç

1. Unity Hub'ı aç
2. "Add" > "Add project from disk" seç
3. Bu klasörü seç

### Adım 2: Gerekli Paketleri İçe Aktar

#### DOTween (Animasyon)
1. Asset Store'dan [DOTween (HOTween v2)](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676) indir
2. Import et
3. Tools > Demigiant > DOTween Utility Panel > Setup DOTween

#### Google Mobile Ads SDK (AdMob)
1. [Google Mobile Ads Unity Plugin](https://github.com/googleads/googleads-mobile-unity/releases) indir
2. `.unitypackage` dosyasını Unity'de import et
3. `Assets/Scripts/Services/AdsManager.cs` dosyasındaki yorumları kaldır
4. AdMob App ID ve Ad Unit ID'lerini ayarla

#### Firebase SDK (Opsiyonel - Analytics & Remote Config)
1. [Firebase Console](https://console.firebase.google.com/) üzerinden proje oluştur
2. `google-services.json` (Android) ve `GoogleService-Info.plist` (iOS) dosyalarını indir
3. Firebase Unity SDK'yı import et
4. Analytics ve RemoteConfig modüllerini ekle
5. `Assets/Scripts/Services/AnalyticsManager.cs` ve `RemoteConfigManager.cs` dosyalarındaki yorumları kaldır

#### TextMeshPro
1. Window > TextMeshPro > Import TMP Essential Resources

### Adım 3: Sahneleri Ayarla

1. `File > Build Settings` aç
2. Şu sahneleri ekle (oluşturmanız gerekecek):
   - `Scenes/Bootstrap`
   - `Scenes/MainMenu`
   - `Scenes/Game`

### Adım 4: Prefab'ları Oluştur

Aşağıdaki prefab'ları oluşturmanız gerekiyor:

**Tube Prefab:**
- Sprite Renderer (tüp görseli)
- Box Collider 2D
- TubeController script
- Liquid Container (child Transform)

**LiquidSegment Prefab:**
- Sprite Renderer (kare/dikdörtgen)
- Scale: 0.5x0.25

**UI Popups:**
- LevelCompletePopup
- SettingsPopup
- RewardedAdPromptPopup
- MessagePopup (toast)

---

## 📁 Proje Yapısı

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs          # Global oyun yöneticisi
│   │   ├── LevelManager.cs         # Level yükleme ve mantığı
│   │   └── InputManager.cs         # Touch/click kontrolü
│   ├── Gameplay/
│   │   ├── TubeController.cs       # Tüp mantığı
│   │   ├── LevelData.cs           # Level veri yapısı
│   │   ├── LevelDataGenerator.cs   # Prosedürel level üretimi
│   │   └── MoveValidator.cs        # Hamle doğrulama
│   ├── UI/
│   │   ├── UIManager.cs            # UI yönetimi
│   │   └── MainMenuUI.cs           # Ana menü
│   ├── Services/
│   │   ├── AdsManager.cs           # Reklam yönetimi
│   │   ├── RewardManager.cs        # Ödüllü reklam mantığı
│   │   ├── AnalyticsManager.cs     # Analytics tracking
│   │   └── RemoteConfigManager.cs  # Remote config
│   └── Save/
│       └── SaveManager.cs          # Kayıt sistemi
├── Scenes/
│   ├── Bootstrap.unity             # İlk yüklenen sahne
│   ├── MainMenu.unity              # Ana menü
│   └── Game.unity                  # Oyun sahnesi
├── Prefabs/
│   ├── Tube.prefab                 # Tüp prefab'ı
│   ├── LiquidSegment.prefab        # Sıvı segmenti
│   └── UI/                         # UI popup'ları
├── Resources/
│   └── Levels/                     # Önceden hazırlanmış seviyeler
│       ├── Level_0.json
│       ├── Level_1.json
│       └── Level_2.json
└── Materials/
    └── Sprites/                    # Sprite'lar
```

---

## 📱 Reklam Entegrasyonu

### AdMob Kurulumu

1. **AdMob hesabı oluştur**: [AdMob Console](https://apps.admob.com/)

2. **Uygulama ve Ad Unit'leri oluştur:**
   - Interstitial Ad Unit
   - Rewarded Ad Unit
   - Banner Ad Unit

3. **Ad Unit ID'leri kopyala ve `AdsManager.cs`'e yapıştır:**

```csharp
// AdsManager.cs içinde bu ID'leri değiştir:
public string AppId = "ca-app-pub-XXXXXX~YYYYYY";
public string InterstitialAdUnitId = "ca-app-pub-XXXXXX/YYYYYY";
public string RewardedAdUnitId = "ca-app-pub-XXXXXX/YYYYYY";
public string BannerAdUnitId = "ca-app-pub-XXXXXX/YYYYYY";
```

### Test Reklamları

Geliştirme sırasında **test reklam ID'leri** kullanın (yukarıdaki kod zaten test ID'leri içeriyor).

**ÖNEMLİ:** Canlı yayına çıkmadan önce gerçek Ad Unit ID'lerinizi kullanın!

### Reklam Stratejisi

**Interstitial (Geçiş Reklamları):**
- Varsayılan: Her 2 seviyede bir
- Remote Config ile ayarlanabilir: `interstitial_frequency`
- Minimum 30 saniye aralık

**Rewarded (Ödüllü Reklamlar):**
- Undo hakkı: 3 hamle/reklam
- Extra tüp: 1 tüp/reklam
- Seviye atlama: Direkt geçiş
- Günlük ödül: 50 coin/reklam

**Banner:**
- Ekranın altında sürekli gösteriliyor
- Settings'de kapatma opsiyonu (TODO)

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
    [1, 2, 1, 2],
    [2, 1, 2, 1],
    [],
    []
  ]
}
```

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

1. `Bootstrap` sahnesini aç
2. Play butonuna bas
3. Konsol loglarına dikkat et (reklam mock mesajları göreceksin)

### Android'de Test

```bash
# Build settings
File > Build Settings
Platform: Android
Switch Platform

# Build APK
Build (veya Build and Run)
```

### Test Checklist

- [ ] Oyun başlıyor mu?
- [ ] Tüplere tıklama çalışıyor mu?
- [ ] Sıvı transfer animasyonu oynanıyor mu?
- [ ] Level tamamlanınca popup geliyor mu?
- [ ] Reklam gösterim logları görünüyor mu?
- [ ] Save/Load çalışıyor mu?
- [ ] Undo butonu çalışıyor mu?

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

## 📝 TODO / Geliştirme Planı

### Kritik (Oyun Yayınlanmadan Önce)

- [ ] **DOTween paketi import et** (animasyonlar için)
- [ ] **AdMob SDK import et** ve reklam kodlarının yorumlarını kaldır
- [ ] **Gerçek AdMob Unit ID'lerini ekle**
- [ ] **Prefab'ları oluştur** (Tube, LiquidSegment, UI popups)
- [ ] **Sahneleri oluştur** (Bootstrap, MainMenu, Game)
- [ ] **Görselleri ekle** (tüp sprite, background, button icons)
- [ ] **Ses efektleri ekle** (pour, complete, button click)
- [ ] **Arka plan müziği ekle**
- [ ] **Uygulama ikonu tasarla**
- [ ] **Test cihazda çalıştır**

### Orta Öncelik

- [ ] **Firebase SDK import et** (Analytics + Remote Config)
- [ ] **Firebase Analytics kodlarının yorumlarını kaldır**
- [ ] **Remote Config aktif et**
- [ ] **IAP sistemi tamamla** (No Ads satın alma)
- [ ] **Level Select ekranı ekle**
- [ ] **Settings popup tasarımı yap**
- [ ] **Hint sistemi implement et**
- [ ] **Particle efektleri ekle** (level complete confetti)
- [ ] **Haptic feedback test et**

### Düşük Öncelik / Nice-to-Have

- [ ] Cloud save (Firebase/PlayFab)
- [ ] Leaderboard
- [ ] Daily challenges
- [ ] Tema sistemi (farklı renk paletleri)
- [ ] Ek tüp şekilleri
- [ ] Power-up'lar (mix renkleri, freeze time vb.)
- [ ] Social share (level completion screenshot)
- [ ] Rate us popup
- [ ] Birden fazla dil desteği

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
