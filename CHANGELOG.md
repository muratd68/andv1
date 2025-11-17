# Changelog

Projedeki tüm önemli değişiklikler bu dosyada belgelenecektir.

## [Unreleased]

### Yapılacaklar
- DOTween paketi entegrasyonu
- AdMob SDK entegrasyonu
- Firebase SDK entegrasyonu
- Prefab'ların oluşturulması
- Sahnelerin kurulması
- Görsellerin eklenmesi
- Ses efektlerinin eklenmesi

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
