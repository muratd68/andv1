# Hızlı Başlangıç Kılavuzu

Bu kılavuz projeyi Unity'de açıp oynanır hale getirmeyi anlatır. Toplam süre yaklaşık 10 dakika.

## 1. Unity'de Projeyi Aç

1. **Unity 2021.3 LTS** kur (Unity Hub > Installs). Android için "Android Build Support", iOS için "iOS Build Support" modüllerini ekle.
2. Unity Hub > **Add** > **Add project from disk** > bu klasörü seç.
3. İlk açılışta Unity paketleri indirir ve scriptleri derler. Console'da kırmızı hata olmamalı.

## 2. Projeyi Kur

Üst menüden **Water Sort > Projeyi Kur (Sahneler + Prefablar)** seç.

1. İlk seferde "TextMeshPro Essential Resources içe aktarıldı" mesajı çıkar. İçe aktarma bitince menüyü **bir kez daha** çalıştır.
2. İkinci çalıştırmada şunlar oluşturulur:
   - **Sahneler:** `Assets/Scenes/Bootstrap.unity`, `MainMenu.unity`, `Game.unity` (Build Settings'e sırayla eklenir)
   - **Prefablar:** `Assets/Prefabs/Tube.prefab`, `Assets/Prefabs/UI/LevelButton.prefab`, `Assets/Resources/Prefabs/LiquidSegment.prefab`
   - **Görseller:** `Assets/Art/Generated/Tube.png`, `LiquidSegment.png`
   - Ana menü, oyun ekranı, ayarlar, seviye seçme, level tamamlama, reklam onayı pencereleri ve tüm buton bağlantıları
3. "Hazır!" mesajı çıkınca kurulum bitti.

Menüyü tekrar çalıştırırsan her şey baştan oluşturulur (sahnelerde elle yaptığın değişiklikler silinir, önce sorar).

## 3. Oyna

**Water Sort > Oyunu Başlat** seç. Bootstrap sahnesi açılır ve oyun başlar.

- Ana menü > **Oyna** ile seviye 1 başlar.
- Bir tüpe dokun (seçilir, büyür), sonra başka bir tüpe dokun (sıvı dökülür).
- Reklam gerektiren butonlar (ipucu, ekstra tüp, 5 hakkı bitince geri al) test modunda çalışır: ekranda "Test reklamı oynatılıyor..." yazar, birkaç saniye sonra ödül verilir.
- Ayarlar > **Dil** butonu Türkçe/İngilizce arasında geçiş yapar.

## 4. Kendi Görsellerini ve Seslerini Ekle (Opsiyonel)

- **Tüp görseli:** `Assets/Art/Generated/Tube.png` dosyasını kendi görselinle değiştir (pivot alt-orta, 100 piksel = 1 birim). Ya da `Tube.prefab` içindeki SpriteRenderer'a başka sprite ver.
- **Sesler:** Game sahnesinde `InputManager` üzerinde Select/Pour/Error Sound alanları var. Arka plan müziği için Bootstrap sahnesindeki `GameManager` > Background Music.
- **Renkler ve fontlar:** Sahnelerdeki UI nesneleri normal Unity UI nesneleridir, Inspector'dan değiştirilebilir.

## 5. Reklamlar, Analitik ve Satın Alma (Opsiyonel)

Bunlar kurulmadan da oyun eksiksiz çalışır (test modu). Kurmak istediğinde:

### AdMob
1. [Google Mobile Ads Unity eklentisini](https://github.com/googleads/googleads-mobile-unity/releases) (8.0 veya üstü) indir ve `Assets > Import Package > Custom Package` ile içe aktar.
2. `Assets > Google Mobile Ads > Settings` içine AdMob App ID'lerini gir.
3. `Assets/Scripts/Services/AdsManager.cs` içindeki reklam birimi ID'lerini kendi ID'lerinle değiştir (şu an Google test ID'leri var).

### Firebase (Analytics + Remote Config)
1. [Firebase Console](https://console.firebase.google.com/)'da proje oluştur, Android/iOS uygulamasını ekle.
2. `google-services.json` ve/veya `GoogleService-Info.plist` dosyasını `Assets/` içine koy.
3. Firebase Unity SDK'dan **FirebaseAnalytics** ve **FirebaseRemoteConfig** paketlerini içe aktar.

### Reklamları Kaldır (IAP)
1. Window > Package Manager > **In-App Purchasing** (4.x) kur.
2. Google Play Console / App Store Connect'te `com.yourcompany.watersort.noads` kimlikli "non-consumable" ürün oluştur (farklı kimlik kullanırsan `IAPManager.NoAdsProductId`'yi değiştir).

### Son adım
Herhangi birini kurduktan sonra **Water Sort > SDK Tanımlarını Güncelle** çalıştır. Araç kurulu SDK'ları algılar ve ilgili kodu açar. Bir SDK'yı kaldırırsan yine bu menüyü çalıştır, aksi halde proje derlenmez.

## 6. Android Build

1. File > Build Settings > **Android** > Switch Platform.
2. Player Settings: Company Name, Product Name, Package Name (`com.yourcompany.watersort` yerine kendi adın).
3. Publishing Settings > Keystore oluştur (Google Play için zorunlu).
4. **Build** (APK, test için) veya **Build App Bundle** (AAB, Google Play için).

## Sorun Giderme

### Menüde "Water Sort" görünmüyor
Console'da derleme hatası var demektir. Hatayı düzelt ya da bir SDK'yı kaldırdıysan `Assets/Scripts` derlenemiyor olabilir: Project Settings > Player > Scripting Define Symbols'tan `WATERSORT_*` tanımlarını sil.

### Yazılar görünmüyor
TextMeshPro Essential Resources eksik. Window > TextMeshPro > Import TMP Essential Resources, sonra kurulumu tekrar çalıştır.

### Tüpler ekrana sığmıyor
Game sahnesindeki `LevelManager` > Fit Camera To Tubes açık olmalı. Board Screen Share değerini düşürerek tüpleri küçültebilirsin.

### Tüplere dokunma çalışmıyor
Tube prefab'ında BoxCollider2D olmalı ve sahnede EventSystem bulunmalı. Kurulum aracı ikisini de ekler; elle değiştirdiysen kurulumu tekrar çalıştır.

## Unity Olmadan Kontrol

.NET 8 SDK kuruluysa:

```bash
dotnet run --project Tests/LogicTests        # seviyeler, çözücü, şifreleme, çeviriler
dotnet build Tests/CompileCheck              # scriptler Unity 2021.3 API'sine karşı derleniyor mu
dotnet build Tests/EditorCompileCheck        # editör scriptleri dahil
```
