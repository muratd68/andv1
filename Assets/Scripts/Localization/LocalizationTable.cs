using System.Collections.Generic;

namespace WaterSort.Core
{
    public enum Language
    {
        Turkish = 0,
        English = 1
    }

    /// <summary>
    /// All player-facing text, indexed by key. Each entry is { Turkish, English }.
    /// Plain C# (no UnityEngine) so it can be checked by the logic tests.
    /// {0}, {1} are string.Format placeholders.
    /// </summary>
    public static class LocalizationTable
    {
        public static readonly Dictionary<string, string[]> Entries = new Dictionary<string, string[]>
        {
            // Main menu
            { "menu.play", new[] { "Oyna", "Play" } },
            { "menu.continue", new[] { "Devam Et", "Continue" } },
            { "menu.level_select", new[] { "Seviye Seç", "Select Level" } },
            { "menu.daily", new[] { "Günlük Ödül", "Daily Reward" } },
            { "menu.settings", new[] { "Ayarlar", "Settings" } },
            { "menu.no_ads", new[] { "Reklamları Kaldır", "Remove Ads" } },
            { "menu.restore", new[] { "Satın Alımları Geri Yükle", "Restore Purchases" } },

            // Common
            { "common.close", new[] { "Kapat", "Close" } },
            { "common.cancel", new[] { "Vazgeç", "Cancel" } },
            { "common.watch", new[] { "İzle", "Watch" } },

            // Settings
            { "settings.title", new[] { "Ayarlar", "Settings" } },
            { "settings.sound", new[] { "Ses Efektleri", "Sound Effects" } },
            { "settings.music", new[] { "Müzik", "Music" } },
            { "settings.haptics", new[] { "Titreşim", "Vibration" } },
            { "settings.language", new[] { "Dil: Türkçe", "Language: English" } },

            // HUD
            { "hud.level", new[] { "Seviye {0}", "Level {0}" } },
            { "hud.moves", new[] { "Hamle: {0}", "Moves: {0}" } },
            { "hud.undo_count", new[] { "Geri Al: {0}", "Undo: {0}" } },

            // Game buttons
            { "button.undo", new[] { "Geri Al", "Undo" } },
            { "button.undo_ad", new[] { "Geri Al (Reklam)", "Undo (Ad)" } },
            { "button.restart", new[] { "Yeniden", "Restart" } },
            { "button.hint", new[] { "İpucu", "Hint" } },
            { "button.extra_tube", new[] { "+ Tüp", "+ Tube" } },
            { "button.menu", new[] { "Menü", "Menu" } },
            { "button.next_level", new[] { "Sonraki Seviye", "Next Level" } },

            // Level complete
            { "complete.title", new[] { "Seviye {0} Tamamlandı!", "Level {0} Complete!" } },
            { "complete.moves", new[] { "{0} hamle ile tamamladınız!", "Completed in {0} moves!" } },

            // Rewarded ads
            { "reward.undo.title", new[] { "Geri Al Hakkı", "Extra Undos" } },
            { "reward.undo.message", new[] { "{0} geri alma hakkı kazanmak için reklam izle", "Watch an ad to get {0} more undos" } },
            { "reward.undo.granted", new[] { "+{0} Geri Alma Hakkı!", "+{0} Undos!" } },
            { "reward.extra_tube.title", new[] { "Ekstra Tüp", "Extra Tube" } },
            { "reward.extra_tube.message", new[] { "Geçici bir boş tüp eklemek için reklam izle", "Watch an ad to add an empty tube" } },
            { "reward.extra_tube.granted", new[] { "Ekstra tüp eklendi!", "Extra tube added!" } },
            { "reward.skip.title", new[] { "Seviyeyi Atla", "Skip Level" } },
            { "reward.skip.message", new[] { "Bu seviyeyi geçmek için reklam izle", "Watch an ad to skip this level" } },
            { "reward.skip.granted", new[] { "Seviye atlandı!", "Level skipped!" } },
            { "reward.hint.title", new[] { "İpucu", "Hint" } },
            { "reward.hint.message", new[] { "Bir sonraki hamle için ipucu almak için reklam izle", "Watch an ad to see the next move" } },
            { "reward.daily.title", new[] { "Günlük Ödül", "Daily Reward" } },
            { "reward.daily.message", new[] { "Günlük {0} coin kazanmak için reklam izle", "Watch an ad to get {0} coins" } },
            { "reward.daily.granted", new[] { "+{0} Coin!", "+{0} Coins!" } },

            // Hints
            { "hint.move", new[] { "İpucu: {0}. tüpten {1}. tüpe dök", "Hint: pour tube {0} into tube {1}" } },
            { "hint.unsolvable", new[] { "Bu durumdan çözüm yok - geri al veya yeniden başla", "No solution from here - undo or restart" } },

            // Ads and purchases
            { "ads.no_connection", new[] { "İnternet bağlantınızı kontrol edin", "Check your internet connection" } },
            { "ads.load_failed", new[] { "Reklam yüklenemedi", "The ad could not be loaded" } },
            { "ads.mock", new[] { "Test reklamı oynatılıyor...", "Playing a test ad..." } },
            { "iap.no_ads_done", new[] { "Reklamlar kaldırıldı. Teşekkürler!", "Ads removed. Thank you!" } },
            { "iap.failed", new[] { "Satın alma tamamlanamadı", "The purchase was not completed" } },
            { "iap.unavailable", new[] { "Satın alma şu an kullanılamıyor", "Purchases are not available right now" } },
            { "iap.restored", new[] { "Satın alımlar geri yüklendi", "Purchases restored" } },
        };
    }
}
