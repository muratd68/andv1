using UnityEngine;
using System;

namespace WaterSort.Core
{
    /// <summary>
    /// Manages rewarded ad placements and rewards
    /// Tracks what rewards to give after watching ads
    /// </summary>
    public class RewardManager : MonoBehaviour
    {
        private static RewardManager _instance;
        public static RewardManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("RewardManager");
                    _instance = go.AddComponent<RewardManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Reward Amounts - Can be overridden by Remote Config")]
        public int UndoMovesPerAd = 3;
        public int CoinsPerDailyReward = 50;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #region Prompt Helpers

        /// <summary>
        /// Asks for confirmation with the in-game popup. Scenes without a UIManager
        /// (e.g. the main menu) go straight to the ad.
        /// </summary>
        private void ShowPrompt(string title, string message, Action onWatchAd)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowRewardedAdPrompt(title, message, onWatchAd);
            else
                onWatchAd?.Invoke();
        }

        private void ShowMessage(string message)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage(message);
            else
                Debug.Log(message);
        }

        #endregion

        #region Undo Moves Reward

        /// <summary>
        /// Show rewarded ad to get more undo moves
        /// </summary>
        public void ShowRewardedAdForUndo()
        {
            ShowPrompt(
                "Geri Al Hakkı",
                $"{UndoMovesPerAd} geri alma hakkı kazanmak için reklam izle",
                () =>
                {
                    // User accepted - show ad
                    AdsManager.Instance.ShowRewardedAd(() =>
                    {
                        // Reward earned
                        LevelManager.Instance.AddUndoMoves(UndoMovesPerAd);

                        ShowMessage($"+{UndoMovesPerAd} Geri Alma Hakkı!");

                        AnalyticsManager.Instance.LogEvent("rewarded_ad_undo", new System.Collections.Generic.Dictionary<string, object>
                        {
                            { "amount", UndoMovesPerAd }
                        });
                    });
                }
            );
        }

        #endregion

        #region Extra Tube Reward

        /// <summary>
        /// Show rewarded ad to add an extra empty tube to the level
        /// </summary>
        public void ShowRewardedAdForExtraTube()
        {
            ShowPrompt(
                "Ekstra Tüp",
                "Geçici bir boş tüp eklemek için reklam izle",
                () =>
                {
                    // User accepted - show ad
                    AdsManager.Instance.ShowRewardedAd(() =>
                    {
                        // Reward earned
                        LevelManager.Instance.AddExtraTube();

                        ShowMessage("Ekstra tüp eklendi!");

                        AnalyticsManager.Instance.LogEvent("rewarded_ad_extra_tube", null);
                    });
                }
            );
        }

        #endregion

        #region Skip Level Reward

        /// <summary>
        /// Show rewarded ad to skip current level
        /// </summary>
        public void ShowRewardedAdForSkip(Action onSkipped = null)
        {
            ShowPrompt(
                "Seviyeyi Atla",
                "Bu seviyeyi geçmek için reklam izle",
                () =>
                {
                    // User accepted - show ad
                    AdsManager.Instance.ShowRewardedAd(() =>
                    {
                        // Reward earned
                        ShowMessage("Seviye atlandı!");

                        AnalyticsManager.Instance.LogEvent("rewarded_ad_skip", new System.Collections.Generic.Dictionary<string, object>
                        {
                            { "level", LevelManager.Instance.CurrentLevelIndex }
                        });

                        onSkipped?.Invoke();
                    });
                }
            );
        }

        #endregion

        #region Hint Reward (Optional Feature)

        /// <summary>
        /// Show rewarded ad to get a hint for the next move
        /// </summary>
        public void ShowRewardedAdForHint()
        {
            // Don't make the player watch an ad for a hint that cannot exist
            if (LevelManager.Instance != null && !LevelManager.Instance.HasSolution())
            {
                ShowMessage("Bu durumdan çözüm yok - geri al veya yeniden başla");
                return;
            }

            ShowPrompt(
                "İpucu",
                "Bir sonraki hamle için ipucu almak için reklam izle",
                () =>
                {
                    // User accepted - show ad
                    AdsManager.Instance.ShowRewardedAd(() =>
                    {
                        ShowHint();

                        AnalyticsManager.Instance.LogEvent("rewarded_ad_hint", null);
                    });
                }
            );
        }

        private void ShowHint()
        {
            if (LevelManager.Instance == null) return;

            if (!LevelManager.Instance.ShowHint())
            {
                ShowMessage("Bu durumdan çözüm yok - geri al veya yeniden başla");
            }
        }

        #endregion

        #region Daily Reward (Optional Feature)

        /// <summary>
        /// Show rewarded ad for daily bonus coins
        /// </summary>
        public void ShowDailyRewardAd(Action onClaimed = null)
        {
            ShowPrompt(
                "Günlük Ödül",
                $"Günlük {CoinsPerDailyReward} coin kazanmak için reklam izle",
                () =>
                {
                    // User accepted - show ad
                    AdsManager.Instance.ShowRewardedAd(() =>
                    {
                        // Reward earned
                        SaveManager.Instance.AddCoins(CoinsPerDailyReward);
                        SaveManager.Instance.SetLastDailyRewardDate(System.DateTime.Now);

                        ShowMessage($"+{CoinsPerDailyReward} Coin!");
                        onClaimed?.Invoke();

                        AnalyticsManager.Instance.LogEvent("rewarded_ad_daily", new System.Collections.Generic.Dictionary<string, object>
                        {
                            { "amount", CoinsPerDailyReward }
                        });
                    });
                }
            );
        }

        public bool CanClaimDailyReward()
        {
            System.DateTime lastClaim = SaveManager.Instance.GetLastDailyRewardDate();
            return (System.DateTime.Now - lastClaim).TotalHours >= 24;
        }

        #endregion
    }
}
