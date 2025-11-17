using UnityEngine;
using System;

namespace WaterSort.Core
{
    /// <summary>
    /// Manages player progress, coins, and persistent data
    /// Uses PlayerPrefs for simple storage (can be upgraded to JSON/Cloud save)
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager _instance;
        public static SaveManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SaveManager");
                    _instance = go.AddComponent<SaveManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Save keys
        private const string KEY_CURRENT_LEVEL = "CurrentLevel";
        private const string KEY_HIGHEST_LEVEL = "HighestLevel";
        private const string KEY_COINS = "Coins";
        private const string KEY_TOTAL_MOVES = "TotalMoves";
        private const string KEY_TOTAL_PLAYTIME = "TotalPlaytime";
        private const string KEY_LAST_DAILY_REWARD = "LastDailyReward";
        private const string KEY_NO_ADS_PURCHASED = "NoAdsPurchased";

        // Runtime data
        private PlayerData _playerData;

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

        public void LoadGame()
        {
            _playerData = new PlayerData
            {
                CurrentLevel = PlayerPrefs.GetInt(KEY_CURRENT_LEVEL, 0),
                HighestLevel = PlayerPrefs.GetInt(KEY_HIGHEST_LEVEL, 0),
                Coins = PlayerPrefs.GetInt(KEY_COINS, 0),
                TotalMoves = PlayerPrefs.GetInt(KEY_TOTAL_MOVES, 0),
                TotalPlaytime = PlayerPrefs.GetFloat(KEY_TOTAL_PLAYTIME, 0f),
                LastDailyRewardDate = GetDateFromString(PlayerPrefs.GetString(KEY_LAST_DAILY_REWARD, "")),
                NoAdsPurchased = PlayerPrefs.GetInt(KEY_NO_ADS_PURCHASED, 0) == 1
            };

            Debug.Log($"Game loaded - Current Level: {_playerData.CurrentLevel}, Coins: {_playerData.Coins}");
        }

        public void SaveGame()
        {
            if (_playerData == null) return;

            PlayerPrefs.SetInt(KEY_CURRENT_LEVEL, _playerData.CurrentLevel);
            PlayerPrefs.SetInt(KEY_HIGHEST_LEVEL, _playerData.HighestLevel);
            PlayerPrefs.SetInt(KEY_COINS, _playerData.Coins);
            PlayerPrefs.SetInt(KEY_TOTAL_MOVES, _playerData.TotalMoves);
            PlayerPrefs.SetFloat(KEY_TOTAL_PLAYTIME, _playerData.TotalPlaytime);
            PlayerPrefs.SetString(KEY_LAST_DAILY_REWARD, GetStringFromDate(_playerData.LastDailyRewardDate));
            PlayerPrefs.SetInt(KEY_NO_ADS_PURCHASED, _playerData.NoAdsPurchased ? 1 : 0);

            PlayerPrefs.Save();

            Debug.Log("Game saved");
        }

        #region Level Progress

        public int GetCurrentLevel()
        {
            return _playerData?.CurrentLevel ?? 0;
        }

        public int GetHighestLevel()
        {
            return _playerData?.HighestLevel ?? 0;
        }

        public void CompleteLevel(int levelIndex, int moveCount)
        {
            if (_playerData == null) return;

            // Move to next level
            _playerData.CurrentLevel = levelIndex + 1;

            // Update highest level reached
            if (_playerData.CurrentLevel > _playerData.HighestLevel)
            {
                _playerData.HighestLevel = _playerData.CurrentLevel;
            }

            // Track total moves
            _playerData.TotalMoves += moveCount;

            // Award coins based on performance
            int coinsEarned = CalculateCoinsForLevel(moveCount);
            AddCoins(coinsEarned);

            SaveGame();

            Debug.Log($"Level {levelIndex} completed! Earned {coinsEarned} coins");
        }

        private int CalculateCoinsForLevel(int moveCount)
        {
            // Give more coins for fewer moves
            if (moveCount <= 5) return 10;
            if (moveCount <= 10) return 7;
            if (moveCount <= 15) return 5;
            return 3;
        }

        public void SetCurrentLevel(int level)
        {
            if (_playerData == null) return;

            _playerData.CurrentLevel = level;
            SaveGame();
        }

        #endregion

        #region Coins

        public int GetCoins()
        {
            return _playerData?.Coins ?? 0;
        }

        public void AddCoins(int amount)
        {
            if (_playerData == null) return;

            _playerData.Coins += amount;
            SaveGame();

            // Notify UI
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateCoinCount();
            }
        }

        public bool SpendCoins(int amount)
        {
            if (_playerData == null) return false;

            if (_playerData.Coins >= amount)
            {
                _playerData.Coins -= amount;
                SaveGame();

                // Notify UI
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateCoinCount();
                }

                return true;
            }

            return false;
        }

        #endregion

        #region Daily Reward

        public DateTime GetLastDailyRewardDate()
        {
            return _playerData?.LastDailyRewardDate ?? DateTime.MinValue;
        }

        public void SetLastDailyRewardDate(DateTime date)
        {
            if (_playerData == null) return;

            _playerData.LastDailyRewardDate = date;
            SaveGame();
        }

        #endregion

        #region IAP

        public bool HasNoAdsPurchased()
        {
            return _playerData?.NoAdsPurchased ?? false;
        }

        public void SetNoAdsPurchased(bool purchased)
        {
            if (_playerData == null) return;

            _playerData.NoAdsPurchased = purchased;
            SaveGame();

            // Update ads manager
            if (purchased)
            {
                AdsManager.Instance.EnableAds = false;
                AdsManager.Instance.DestroyBannerAd();
            }
        }

        #endregion

        #region Statistics

        public void AddPlaytime(float seconds)
        {
            if (_playerData == null) return;

            _playerData.TotalPlaytime += seconds;
        }

        public float GetTotalPlaytime()
        {
            return _playerData?.TotalPlaytime ?? 0f;
        }

        public int GetTotalMoves()
        {
            return _playerData?.TotalMoves ?? 0;
        }

        #endregion

        #region Helpers

        private string GetStringFromDate(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private DateTime GetDateFromString(string dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return DateTime.MinValue;

            try
            {
                return DateTime.Parse(dateString);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        #endregion

        #region Debug

        public void ResetAllProgress()
        {
            PlayerPrefs.DeleteAll();
            LoadGame();
            Debug.Log("All progress reset!");
        }

        public void UnlockAllLevels(int levelCount)
        {
            if (_playerData == null) return;

            _playerData.HighestLevel = levelCount;
            SaveGame();
            Debug.Log($"Unlocked {levelCount} levels!");
        }

        public void AddDebugCoins(int amount)
        {
            AddCoins(amount);
            Debug.Log($"Added {amount} debug coins");
        }

        #endregion
    }

    [System.Serializable]
    public class PlayerData
    {
        public int CurrentLevel;
        public int HighestLevel;
        public int Coins;
        public int TotalMoves;
        public float TotalPlaytime;
        public DateTime LastDailyRewardDate;
        public bool NoAdsPurchased;
    }
}
