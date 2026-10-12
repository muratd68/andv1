using UnityEngine;
using System;
using System.Collections.Generic;
#if WATERSORT_FIREBASE
using Firebase.Extensions;
using Firebase.RemoteConfig;
#endif

namespace WaterSort.Core
{
    /// <summary>
    /// Remote Config for A/B testing and live tuning.
    /// Allows changing game parameters without an app update. Uses the defaults below
    /// until Firebase Remote Config is available (Firebase SDK + WATERSORT_FIREBASE define).
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

        /// <summary>
        /// Raised after fetched values are activated
        /// </summary>
        public event Action ConfigUpdated;
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

            FirebaseInit.WhenReady(ok =>
            {
                if (ok)
                    InitializeFirebaseRemoteConfig();
                else
                    Debug.Log("Remote Config: Firebase not available - using defaults");
            });

            _isInitialized = true;
        }

        private void InitializeFirebaseRemoteConfig()
        {
#if WATERSORT_FIREBASE
            var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
            remoteConfig.SetDefaultsAsync(_defaults).ContinueWithOnMainThread(_ => FetchRemoteConfig());
#endif
        }

        public void FetchRemoteConfig()
        {
#if WATERSORT_FIREBASE
            var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
            remoteConfig.FetchAsync(TimeSpan.FromHours(12)).ContinueWithOnMainThread(fetchTask =>
            {
                if (fetchTask.IsCanceled || fetchTask.IsFaulted)
                {
                    Debug.LogWarning("Remote Config fetch failed - using cached/default values");
                    LoadConfigValues();
                    return;
                }

                remoteConfig.ActivateAsync().ContinueWithOnMainThread(_ => LoadConfigValues());
            });
#endif
        }

        private void LoadConfigValues()
        {
#if WATERSORT_FIREBASE
            var remoteConfig = FirebaseRemoteConfig.DefaultInstance;

            foreach (var kvp in _defaults)
            {
                ConfigValue value = remoteConfig.GetValue(kvp.Key);

                // Keep the type of the default value
                if (kvp.Value is int)
                    _currentConfig[kvp.Key] = (int)value.LongValue;
                else if (kvp.Value is float)
                    _currentConfig[kvp.Key] = (float)value.DoubleValue;
                else if (kvp.Value is bool)
                    _currentConfig[kvp.Key] = value.BooleanValue;
                else
                    _currentConfig[kvp.Key] = value.StringValue;
            }
#endif

            if (DebugMode)
            {
                Debug.Log("Remote Config Values:");
                foreach (var kvp in _currentConfig)
                {
                    Debug.Log($"  {kvp.Key} = {kvp.Value}");
                }
            }

            ConfigUpdated?.Invoke();
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
