# Katkıda Bulunma Rehberi

Bu projeye katkıda bulunmak istediğiniz için teşekkürler!

## Geliştirme Süreci

1. **Fork edin**: Projeyi kendi hesabınıza fork edin
2. **Branch oluşturun**: `git checkout -b feature/amazing-feature`
3. **Değişikliklerinizi yapın**: Kodunuzu yazın, test edin
4. **Commit edin**: `git commit -m 'feat: Add amazing feature'`
5. **Push edin**: `git push origin feature/amazing-feature`
6. **Pull Request açın**: GitHub'da PR oluşturun

## Commit Mesajları

Conventional Commits formatını kullanın:

```
feat: Yeni özellik ekleme
fix: Bug düzeltme
docs: Dokümantasyon değişikliği
style: Kod formatı değişikliği
refactor: Kod yeniden yapılandırma
test: Test ekleme/düzeltme
chore: Build/tooling değişiklikleri
```

Örnek:
```
feat: Add hint system for difficult levels
fix: Tube selection bug on Android
docs: Update README with Firebase setup
```

## Kod Standartları

### C# Kodlama Standartları
- PascalCase: Class, method, property isimleri
- camelCase: Local variable, parameter isimleri
- _camelCase: Private field isimleri
- UPPER_CASE: Constant isimleri

Örnek:
```csharp
public class TubeController : MonoBehaviour
{
    private const int MAX_CAPACITY = 4;
    private List<LiquidSegment> _liquidStack;

    public void AddLiquid(LiquidColor color, int amount)
    {
        int targetIndex = _liquidStack.Count;
        // ...
    }
}
```

### Dokümantasyon
- Public method/property'lere XML comment ekleyin
- Karmaşık algoritmaları açıklayın
- TODO yorumları kullanın

```csharp
/// <summary>
/// Validates if liquid can be poured from source to target tube
/// </summary>
/// <param name="sourceTube">Tube to pour from</param>
/// <param name="targetTube">Tube to pour into</param>
/// <returns>True if move is valid</returns>
public static bool IsValidMove(TubeController sourceTube, TubeController targetTube)
{
    // TODO: Add color matching optimization
    // Implementation...
}
```

## Test Etme

Değişikliklerinizi test edin:

- [ ] Unity Editor'de çalışıyor mu?
- [ ] Android cihazda test ettiniz mi?
- [ ] Console'da error/warning var mı?
- [ ] Performans etkilendi mi?

## Pull Request Süreci

1. **Güncel kalın**: Main branch'ten pull edin
2. **Çakışmaları çözün**: Merge conflict varsa düzeltin
3. **Açıklayıcı olun**: PR açıklamasında ne değiştirdiğinizi yazın
4. **Screenshot ekleyin**: UI değişiklikleri varsa görsel ekleyin

## Proje Yapısına Ekleme

Yeni dosya eklerken:

```
Assets/Scripts/
├── Core/           # Temel sistem scriptleri
├── Gameplay/       # Oyun mekanikleri
├── UI/             # UI scriptleri
├── Services/       # Harici servis entegrasyonları
└── Save/           # Save/load sistemi
```

Her klasöre uygun yere ekleyin.

## Sorular?

- GitHub Issues kullanın
- Tartışmalar için Discussions kullanın
- Kodla ilgili sorular için code review'da yorum yapın

Teşekkürler! 🙏
