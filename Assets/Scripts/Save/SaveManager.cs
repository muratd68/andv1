using UnityEngine;
using System;
using System.Globalization;

namespace WaterSort.Core
{
    /// <summary>
    /// Manages player progress, coins, and persistent data.
    /// The whole save is one JSON record, AES-encrypted with an integrity hash
    /// (SecurityManager), stored in PlayerPrefs. Saves from older versions
    /// (plain PlayerPrefs keys) are migrated on first load.
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

        // Current save format
        private const string KEY_SAVE_DATA = "SaveData_v2";

        // Legacy (plain text) keys - only read for migration
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
            if (PlayerPrefs.HasKey(KEY_SAVE_DATA))
            {
                _playerData = LoadSecure();
            }
            else if (PlayerPrefs.HasKey(KEY_CURRENT_LEVEL) || PlayerPrefs.HasKey(KEY_COINS))
            {
                _playerData = LoadLegacy();
                SaveGame();
                DeleteLegacyKeys();
                Debug.Log("Migrated old save to the encrypted format");
            }
            else
            {
                _playerData = new PlayerData { LastDailyRewardDate = DateTime.MinValue };
            }

            Debug.Log($"Game loaded - Current Level: {_playerData.CurrentLevel}, Coins: {_playerData.Coins}");
        }

        public void SaveGame()
        {
            if (_playerData == null) return;

            var record = new SaveRecord
            {
                CurrentLevel = _playerData.CurrentLevel,
                HighestLevel = _playerData.HighestLevel,
                Coins = _playerData.Coins,
                TotalMoves = _playerData.TotalMoves,
                TotalPlaytime = _playerData.TotalPlaytime,
                LastDailyReward = GetStringFromDate(_playerData.LastDailyRewardDate),
                NoAdsPurchased = _playerData.NoAdsPurchased
            };

            // Encrypts, stores an integrity hash and calls PlayerPrefs.Save()
            SecurityManager.Instance.SecureSave(KEY_SAVE_DATA, JsonUtility.ToJson(record));
        }

        private PlayerData LoadSecure()
        {
            string json = SecurityManager.Instance.SecureLoad(KEY_SAVE_DATA, "");
            SaveRecord record = null;

            try
            {
                if (!string.IsNullOrEmpty(json))
                    record = JsonUtility.FromJson<SaveRecord>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Save data could not be read: {ex.Message}");
            }

            if (record == null)
            {
                // Tampered or corrupt: start fresh rather than trusting the data
                Debug.LogWarning("Save data invalid - starting a new save");
                return new PlayerData { LastDailyRewardDate = DateTime.MinValue };
            }

            return new PlayerData
            {
                CurrentLevel = Mathf.Max(0, record.CurrentLevel),
                HighestLevel = Mathf.Max(0, record.HighestLevel),
                Coins = Mathf.Max(0, record.Coins),
                TotalMoves = record.TotalMoves,
                TotalPlaytime = record.TotalPlaytime,
                LastDailyRewardDate = GetDateFromString(record.LastDailyReward),
                NoAdsPurchased = record.NoAdsPurchased
            };
        }

        private PlayerData LoadLegacy()
        {
            return new PlayerData
            {
                CurrentLevel = PlayerPrefs.GetInt(KEY_CURRENT_LEVEL, 0),
                HighestLevel = PlayerPrefs.GetInt(KEY_HIGHEST_LEVEL, 0),
                Coins = PlayerPrefs.GetInt(KEY_COINS, 0),
                TotalMoves = PlayerPrefs.GetInt(KEY_TOTAL_MOVES, 0),
                TotalPlaytime = PlayerPrefs.GetFloat(KEY_TOTAL_PLAYTIME, 0f),
                LastDailyRewardDate = GetDateFromString(PlayerPrefs.GetString(KEY_LAST_DAILY_REWARD, "")),
                NoAdsPurchased = PlayerPrefs.GetInt(KEY_NO_ADS_PURCHASED, 0) == 1
            };
        }

        private void DeleteLegacyKeys()
        {
            PlayerPrefs.DeleteKey(KEY_CURRENT_LEVEL);
            PlayerPrefs.DeleteKey(KEY_HIGHEST_LEVEL);
            PlayerPrefs.DeleteKey(KEY_COINS);
            PlayerPrefs.DeleteKey(KEY_TOTAL_MOVES);
            PlayerPrefs.DeleteKey(KEY_TOTAL_PLAYTIME);
            PlayerPrefs.DeleteKey(KEY_LAST_DAILY_REWARD);
            PlayerPrefs.DeleteKey(KEY_NO_ADS_PURCHASED);
            PlayerPrefs.Save();
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

            if (level > _playerData.HighestLevel)
            {
                _playerData.HighestLevel = level;
            }

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
                AdsManager.Instance.RemoveAds();
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
            // Round-trip format, independent of the device language (e.g. Turkish locale)
            return date.ToString("o", CultureInfo.InvariantCulture);
        }

        private DateTime GetDateFromString(string dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return DateTime.MinValue;

            try
            {
                return DateTime.Parse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
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

    /// <summary>
    /// Serialized form of PlayerData (JsonUtility cannot serialize DateTime)
    /// </summary>
    [System.Serializable]
    public class SaveRecord
    {
        public int CurrentLevel;
        public int HighestLevel;
        public int Coins;
        public int TotalMoves;
        public float TotalPlaytime;
        public string LastDailyReward;
        public bool NoAdsPurchased;
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
