# Changelog

Projedeki tüm önemli değişiklikler bu dosyada belgelenecektir.

## [0.2.0] - 2026-10-12

### Eklenen
- Tek tıkla kurulum: `Water Sort > Projeyi Kur` sahneleri, prefabları, UI'ı oluşturur ve bağlar
- Türkçe ve İngilizce dil desteği
- Çözücü (LevelSolver): her seviye doğrulanır, ipucu gerçek hamleyi gösterir
- Ana menüde ayarlar ve seviye seçme
- Reklamları Kaldır satın alması (Unity IAP), iOS için geri yükleme
- AdMob, Firebase ve IAP kodu SDK kurulunca otomatik açılır (`WATERSORT_*` tanımları)
- Şifreli kayıt ve eski kayıtlardan otomatik taşıma
- Müzik ve ses efekti desteği, güvenli alan (notch) desteği, kamera otomatik sığdırma
- Unity olmadan çalışan testler ve derleme kontrolleri (`Tests/`)

### Değişen
- DOTween kaldırıldı, yerine yerleşik `SimpleTween`
- Level JSON formatı: her tüp `{ "Colors": [...] }`
- Reklamları Kaldır yalnızca interstitial ve banner'ı kaldırır; ödüllü reklamlar isteğe bağlı kalır
- Prosedürel seviyeler her açılışta aynı (seviye numarasına bağlı seed)

### Düzeltilen
- Derleme hataları (PlayerData tekrarı, property üzerinde [Header], olmayan metotlar, BuildOptimizer)
- İkinci seviyede dokunmaların çalışmaması
- Elle tasarlanmış JSON seviyelerinin hiç yüklenmemesi
- Çözülemeyen seviye üretilebilmesi
- Tüp kapasitesi, ekstra tüp yerleşimi, nesne havuzu, geri alma hakkı sıfırlama
- Atlanan seviyenin kaydedilmemesi, seviye yazısının güncellenmemesi
- Ana menüde günlük ödülün çökmesi, Türkçe cihazlarda tarih kaydı
- Reklam sıklığı sayacı, şifrelemede sabit IV

## [0.1.0] - 2024-11-17

### Eklenen
- Temel proje yapısı oluşturuldu
- Core sistem scriptleri (GameManager, LevelManager, InputManager)
- Gameplay scriptleri (TubeController, LevelData, MoveValidator)
- Reklam sistemi (AdsManager, RewardManager)
- UI sistemi (UIManager, MainMenuUI)
- Save/Progress sistemi
- Analytics ve Remote Config altyapısı
- Prosedürel level üretim algoritması
- Örnek level dosyaları (Level 0-2)
- Kapsamlı dokümantasyon (README, SETUP_GUIDE)
- Git yapılandırması (.gitignore)

### Özellikler
- 1000+ seviye desteği (prosedürel üretim)
- Zorluk eğrisi sistemi
- Undo/Redo sistemi
- Interstitial, Rewarded, Banner reklam desteği
- Remote Config ile canlı ayar değişikliği
- Analytics event tracking
- Save/Load sistemi
- Coin sistemi
- Daily reward altyapısı

### Teknik
- Modüler ve ölçeklenebilir mimari
- Singleton pattern ile manager'lar
- ScriptableObject tabanlı level data
- JSON tabanlı level storage
- PlayerPrefs ile basit save sistemi
