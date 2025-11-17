using UnityEngine;
using System.Collections.Generic;
// using Firebase.Analytics; // Uncomment when Firebase is imported

namespace WaterSort.Core
{
    /// <summary>
    /// Analytics event tracking
    /// Integrates with Firebase Analytics (or other analytics services)
    /// TODO: Import Firebase SDK and uncomment Firebase code
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        private static AnalyticsManager _instance;
        public static AnalyticsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("AnalyticsManager");
                    _instance = go.AddComponent<AnalyticsManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Settings")]
        public bool EnableAnalytics = true;
        public bool DebugMode = true;

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
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("Initializing Analytics Manager...");

            if (!EnableAnalytics)
            {
                Debug.Log("Analytics are disabled");
                return;
            }

            // TODO: Uncomment when Firebase is imported
            /*
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                    Debug.Log("Firebase Analytics initialized");
                }
                else
                {
                    Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
                }
            });
            */

            _isInitialized = true;
        }

        /// <summary>
        /// Log a custom event with optional parameters
        /// </summary>
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!EnableAnalytics) return;

            if (DebugMode)
            {
                string paramsStr = "";
                if (parameters != null)
                {
                    foreach (var kvp in parameters)
                    {
                        paramsStr += $"{kvp.Key}={kvp.Value}, ";
                    }
                }
                Debug.Log($"[Analytics] Event: {eventName} | Params: {paramsStr}");
            }

            // TODO: Uncomment when Firebase is imported
            /*
            if (parameters != null && parameters.Count > 0)
            {
                Parameter[] firebaseParams = new Parameter[parameters.Count];
                int i = 0;
                foreach (var kvp in parameters)
                {
                    if (kvp.Value is int)
                        firebaseParams[i] = new Parameter(kvp.Key, (int)kvp.Value);
                    else if (kvp.Value is long)
                        firebaseParams[i] = new Parameter(kvp.Key, (long)kvp.Value);
                    else if (kvp.Value is double)
                        firebaseParams[i] = new Parameter(kvp.Key, (double)kvp.Value);
                    else if (kvp.Value is float)
                        firebaseParams[i] = new Parameter(kvp.Key, (double)(float)kvp.Value);
                    else
                        firebaseParams[i] = new Parameter(kvp.Key, kvp.Value.ToString());
                    i++;
                }
                FirebaseAnalytics.LogEvent(eventName, firebaseParams);
            }
            else
            {
                FirebaseAnalytics.LogEvent(eventName);
            }
            */
        }

        #region Common Events

        /// <summary>
        /// Track when a level starts
        /// </summary>
        public void LogLevelStart(int levelIndex)
        {
            LogEvent("level_start", new Dictionary<string, object>
            {
                { "level_id", levelIndex },
                { "level_name", $"Level_{levelIndex}" }
            });
        }

        /// <summary>
        /// Track when a level is completed
        /// </summary>
        public void LogLevelComplete(int levelIndex, int moveCount, float timeSpent)
        {
            LogEvent("level_complete", new Dictionary<string, object>
            {
                { "level_id", levelIndex },
                { "move_count", moveCount },
                { "time_spent", timeSpent }
            });
        }

        /// <summary>
        /// Track when a level is failed or player gets stuck
        /// </summary>
        public void LogLevelFailed(int levelIndex, int moveCount, string reason)
        {
            LogEvent("level_failed", new Dictionary<string, object>
            {
                { "level_id", levelIndex },
                { "move_count", moveCount },
                { "reason", reason }
            });
        }

        /// <summary>
        /// Track ad impressions
        /// </summary>
        public void LogAdImpression(string adType, string placement)
        {
            LogEvent("ad_impression", new Dictionary<string, object>
            {
                { "ad_type", adType },
                { "placement", placement }
            });
        }

        /// <summary>
        /// Track rewarded ad completions
        /// </summary>
        public void LogRewardedAdCompleted(string rewardType, int rewardAmount)
        {
            LogEvent("rewarded_ad_completed", new Dictionary<string, object>
            {
                { "reward_type", rewardType },
                { "reward_amount", rewardAmount }
            });
        }

        /// <summary>
        /// Track IAP purchases
        /// </summary>
        public void LogPurchase(string productId, string currency, double value)
        {
            LogEvent("purchase", new Dictionary<string, object>
            {
                { "product_id", productId },
                { "currency", currency },
                { "value", value }
            });
        }

        /// <summary>
        /// Track game sessions
        /// </summary>
        public void LogSessionStart()
        {
            LogEvent("session_start", new Dictionary<string, object>
            {
                { "timestamp", System.DateTime.Now.ToString() }
            });
        }

        public void LogSessionEnd(float duration)
        {
            LogEvent("session_end", new Dictionary<string, object>
            {
                { "duration", duration }
            });
        }

        #endregion

        #region User Properties

        /// <summary>
        /// Set user properties for segmentation
        /// </summary>
        public void SetUserProperty(string propertyName, string value)
        {
            if (!EnableAnalytics) return;

            // TODO: Uncomment when Firebase is imported
            // FirebaseAnalytics.SetUserProperty(propertyName, value);

            if (DebugMode)
            {
                Debug.Log($"[Analytics] User Property: {propertyName} = {value}");
            }
        }

        public void SetUserLevel(int level)
        {
            SetUserProperty("user_level", level.ToString());
        }

        public void SetTotalPlaytime(float hours)
        {
            SetUserProperty("total_playtime", hours.ToString("F2"));
        }

        #endregion
    }
}
