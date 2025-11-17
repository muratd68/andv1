using UnityEngine;
using System.Collections.Generic;
// using Firebase.RemoteConfig; // Uncomment when Firebase is imported

namespace WaterSort.Core
{
    /// <summary>
    /// Remote Config for A/B testing and live tuning
    /// Allows changing game parameters without app update
    /// TODO: Import Firebase SDK and uncomment Firebase code
    /// </summary>
    public class RemoteConfigManager : MonoBehaviour
    {
        private static RemoteConfigManager _instance;
        public static RemoteConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("RemoteConfigManager");
                    _instance = go.AddComponent<RemoteConfigManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Settings")]
        public bool EnableRemoteConfig = true;
        public bool DebugMode = true;

        // Default values (fallback)
        private Dictionary<string, object> _defaults = new Dictionary<string, object>
        {
            // Ad Settings
            { "interstitial_frequency", 2 },
            { "min_time_between_interstitials", 30f },
            { "show_banner", true },

            // Reward Settings
            { "undo_moves_per_ad", 3 },
            { "daily_reward_coins", 50 },
            { "coins_per_level_easy", 10 },
            { "coins_per_level_medium", 7 },
            { "coins_per_level_hard", 5 },

            // Difficulty Settings
            { "starting_undo_moves", 5 },
            { "max_colors", 12 },

            // Feature Flags
            { "enable_hints", true },
            { "enable_daily_rewards", true },
            { "enable_level_select", false },

            // Economy
            { "hint_cost_coins", 20 },
            { "extra_tube_cost_coins", 30 },
            { "skip_level_cost_coins", 50 }
        };

        private Dictionary<string, object> _currentConfig;
        private bool _isInitialized = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _currentConfig = new Dictionary<string, object>(_defaults);
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("Initializing Remote Config Manager...");

            if (!EnableRemoteConfig)
            {
                Debug.Log("Remote Config is disabled - using defaults");
                return;
            }

            // TODO: Uncomment when Firebase is imported
            /*
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    InitializeFirebaseRemoteConfig();
                }
                else
                {
                    Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
                }
            });
            */

            _isInitialized = true;
        }

        private void InitializeFirebaseRemoteConfig()
        {
            // TODO: Uncomment when Firebase is imported
            /*
            var remoteConfig = FirebaseRemoteConfig.DefaultInstance;

            // Set default values
            var defaults = new Dictionary<string, object>();
            foreach (var kvp in _defaults)
            {
                defaults[kvp.Key] = kvp.Value;
            }
            remoteConfig.SetDefaultsAsync(defaults);

            // Fetch remote values
            FetchRemoteConfig();
            */

            Debug.Log("[MOCK] Firebase Remote Config would be initialized here");
        }

        public void FetchRemoteConfig()
        {
            // TODO: Uncomment when Firebase is imported
            /*
            var remoteConfig = FirebaseRemoteConfig.DefaultInstance;

            // Set cache expiration
            System.TimeSpan cacheExpiration = System.TimeSpan.FromHours(12);

            remoteConfig.FetchAsync(cacheExpiration).ContinueWith(fetchTask =>
            {
                if (fetchTask.IsCanceled)
                {
                    Debug.Log("Remote Config fetch was cancelled");
                    return;
                }
                if (fetchTask.IsFaulted)
                {
                    Debug.LogError("Remote Config fetch failed");
                    return;
                }

                Debug.Log("Remote Config fetched successfully");

                remoteConfig.ActivateAsync().ContinueWith(activateTask =>
                {
                    Debug.Log("Remote Config activated");
                    LoadConfigValues();
                });
            });
            */

            Debug.Log("[MOCK] Fetching remote config...");
            LoadConfigValues();
        }

        private void LoadConfigValues()
        {
            // TODO: Uncomment when Firebase is imported
            /*
            var remoteConfig = FirebaseRemoteConfig.DefaultInstance;

            foreach (var key in _defaults.Keys)
            {
                var value = remoteConfig.GetValue(key);
                _currentConfig[key] = ConvertValue(value);
            }
            */

            if (DebugMode)
            {
                Debug.Log("Remote Config Values:");
                foreach (var kvp in _currentConfig)
                {
                    Debug.Log($"  {kvp.Key} = {kvp.Value}");
                }
            }
        }

        #region Get Config Values

        public int GetInt(string key, int defaultValue = 0)
        {
            if (_currentConfig.ContainsKey(key))
            {
                return System.Convert.ToInt32(_currentConfig[key]);
            }
            return defaultValue;
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            if (_currentConfig.ContainsKey(key))
            {
                return System.Convert.ToSingle(_currentConfig[key]);
            }
            return defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            if (_currentConfig.ContainsKey(key))
            {
                return System.Convert.ToBoolean(_currentConfig[key]);
            }
            return defaultValue;
        }

        public string GetString(string key, string defaultValue = "")
        {
            if (_currentConfig.ContainsKey(key))
            {
                return _currentConfig[key].ToString();
            }
            return defaultValue;
        }

        #endregion

        #region Testing / Debug

        /// <summary>
        /// Override a config value locally for testing
        /// </summary>
        public void SetTestValue(string key, object value)
        {
            _currentConfig[key] = value;
            Debug.Log($"[Test] Set {key} = {value}");
        }

        /// <summary>
        /// Reset all values to defaults
        /// </summary>
        public void ResetToDefaults()
        {
            _currentConfig = new Dictionary<string, object>(_defaults);
            Debug.Log("Remote Config reset to defaults");
        }

        #endregion
    }
}
