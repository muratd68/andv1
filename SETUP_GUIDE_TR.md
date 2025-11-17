# Hızlı Başlangıç Kılavuzu

Bu doküman, projeyi ilk kez açan birinin adım adım ne yapması gerektiğini açıklar.

## 1. Unity'de Projeyi Aç

```
1. Unity Hub'ı aç
2. "Add" butonuna tıkla
3. Bu klasörü seç
4. Unity 2021.3 LTS veya üzeri seç
5. Projeyi aç
```

İlk açılışta birkaç dakika sürebilir (script'leri compile ediyor).

## 2. Gerekli Paketleri İçe Aktar

### TextMeshPro (Zorunlu)
```
Window > TextMeshPro > Import TMP Essential Resources
```

### DOTween (Zorunlu - Animasyonlar)
```
1. Asset Store'da ara: "DOTween"
2. Download + Import
3. Tools > Demigiant > DOTween Utility Panel > Setup DOTween
```

### Google Mobile Ads SDK (Reklamlar için)
```
1. https://github.com/googleads/googleads-mobile-unity/releases
2. Son sürümü indir (GoogleMobileAds-v*.unitypackage)
3. Unity'de Assets > Import Package > Custom Package
4. Import et
```

İmport ettikten sonra:
```
Assets/Scripts/Services/AdsManager.cs dosyasını aç
Başındaki // using GoogleMobileAds.Api; yorumunu kaldır
Dosya içindeki diğer yorum satırlarını da kaldır
```

## 3. Sahneleri Oluştur

Şu sahneleri oluşturman gerekiyor:

### Bootstrap Scene
```
File > New Scene
Boş GameObject oluştur, adını "GameManager" yap
GameManager scriptini attach et
File > Save As > Scenes/Bootstrap
```

### MainMenu Scene
```
File > New Scene
Canvas oluştur (UI > Canvas)
Canvas'a MainMenuUI scriptini attach et
Play, Settings, Level Select butonları ekle
File > Save As > Scenes/MainMenu
```

### Game Scene
```
File > New Scene
Canvas oluştur (UI > Canvas)
Canvas'a UIManager scriptini attach et
Empty GameObject: "TubesContainer"
Empty GameObject: "LevelManager" (LevelManager scriptini attach et)
Empty GameObject: "InputManager" (InputManager scriptini attach et)
File > Save As > Scenes/Game
```

Build Settings'e sahneleri ekle:
```
File > Build Settings
Add Open Scenes (her 3 sahneyi de ekle)
Bootstrap en üstte olmalı
```

## 4. Prefab'ları Oluştur

### Tube Prefab
```
Hierarchy'de Create Empty > "Tube"

Tube altında:
1. Sprite Renderer ekle
   - Sprite: Basit bir dikdörtgen (Create > Sprites > Square)
   - Color: Beyaz/Açık Gri
   - Sorting Layer: Default

2. Box Collider 2D ekle
   - Is Trigger: false
   - Size: (1, 2)

3. TubeController script ekle
   - Max Capacity: 4

4. Create Empty child: "LiquidContainer"
   - Position: (0, -0.8, 0)

Prefabs klasörüne sürükle
```

### LiquidSegment Prefab
```
Hierarchy'de Sprite > Square
Adını "LiquidSegment" yap
Scale: (0.4, 0.2, 1)
Color: Red (test için)

Prefabs klasörüne sürükle
```

### TubeController'da Prefab Referansını Ayarla
```
Tube prefab'ını seç
TubeController > Liquid Segment Prefab: LiquidSegment prefab'ını sürükle
Apply changes
```

## 5. UI Prefabları (Basit Versiyon)

### LevelCompletePopup
```
UI > Panel (adını "LevelCompletePopup" yap)
Panel altında:
  - UI > Text - TextMeshPro: "Level Complete!" (title)
  - UI > Text - TextMeshPro: "Moves: X" (moves text)
  - UI > Button: "Next Level" (next button)

Prefabs/UI/ klasörüne kaydet
```

Diğer popup'lar da benzer şekilde oluşturulur (isteğe bağlı).

## 6. Game Scene'de Referansları Bağla

### LevelManager
```
LevelManager GameObject'i seç

LevelManager script:
  - Tubes Container: Hierarchy'deki TubesContainer'ı sürükle
  - Tube Prefab: Prefabs/Tube prefab'ını sürükle
```

### UIManager
```
Canvas'ı seç

UIManager script:
  - Level Text: Level text UI elementini sürükle
  - Move Count Text: Move count text UI elementini sürükle
  - Undo Button: Undo button'ını sürükle
  - Restart Button: Restart button'ını sürükle
  - Level Complete Popup: LevelCompletePopup prefab'ını sürükle
  (diğer referanslar opsiyonel)
```

## 7. Kamera Ayarı

```
Main Camera:
  - Position: (0, 0, -10)
  - Projection: Orthographic
  - Size: 5
  - Background: Mavi/Gri bir renk
```

## 8. İlk Test

```
1. Bootstrap sahnesini aç
2. Play butonuna bas
3. Console'da hata olmamalı
4. MainMenu sahnesine geçmeli
5. Play butonuna basınca Game sahnesine geçmeli
6. Tüpler görünmeli
7. Tüplere tıklayınca seçilmeli
```

## 9. Reklam Test (Opsiyonel)

Eğer AdMob SDK import ettiysen:

```
AdsManager GameObject'i oluştur (Bootstrap sahnesinde)
AdsManager script ekle

Test Ad Unit ID'leri (varsayılan olarak zaten var):
  - App ID: ca-app-pub-3940256099942544~3347511713
  - Interstitial: ca-app-pub-3940256099942544/1033173712
  - Rewarded: ca-app-pub-3940256099942544/5224354917
  - Banner: ca-app-pub-3940256099942544/6300978111
```

Play modunda Console'da "[MOCK] Ads..." mesajları göreceksin.

## 10. Android Build Test

```
File > Build Settings
Platform: Android > Switch Platform

Build Settings'de "Development Build" işaretle
Build and Run

APK cihazda çalışmalı
```

## Sorun Giderme

### "DOTween not found" hatası
- DOTween paketini import et
- Tools > Demigiant > DOTween Utility Panel > Setup DOTween

### Tüpler görünmüyor
- Tube prefab'ın Sprite Renderer'ının olduğundan emin ol
- LevelManager'da Tube Prefab referansının olduğundan emin ol
- Camera'nın Orthographic olduğundan emin ol

### Tıklama çalışmıyor
- InputManager GameObject'inin olduğundan emin ol
- Tube'larda Box Collider 2D olduğundan emin ol
- Event System'in olduğundan emin ol (Canvas ile otomatik gelir)

### Seviye yüklenmiyor
- Resources/Levels/ klasöründe örnek level JSON'ları olduğundan emin ol
- LevelDataGenerator prosedürel üretiyor, JSON yoksa sıkıntı yok

## Sonraki Adımlar

Temel çalışır hale geldiğinde:

1. Görselleri iyileştir (tüp sprite'ı, arkaplan, buttonlar)
2. Ses efektleri ekle
3. Animasyonları iyileştir (DOTween ile)
4. Daha fazla level tasarla
5. Gerçek AdMob Unit ID'lerini ekle
6. Firebase ekle (Analytics + Remote Config)

---

Herhangi bir sorun yaşarsan README.md'yi kontrol et veya GitHub Issues'a yaz!
