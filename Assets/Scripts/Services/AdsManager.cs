using UnityEngine;
using System;
using System.Collections;
// using GoogleMobileAds.Api; // Uncomment when AdMob SDK is imported

namespace WaterSort.Core
{
    /// <summary>
    /// Central ad management system
    /// Handles interstitial, rewarded, and banner ads
    /// TODO: Import Google Mobile Ads SDK and uncomment AdMob code
    /// </summary>
    public class AdsManager : MonoBehaviour
    {
        private static AdsManager _instance;
        public static AdsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("AdsManager");
                    _instance = go.AddComponent<AdsManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Ad Unit IDs - REPLACE WITH YOUR IDS")]
        [Tooltip("AdMob App ID")]
        public string AppId = "ca-app-pub-3940256099942544~3347511713"; // Test ID

        [Tooltip("Interstitial Ad Unit ID")]
        public string InterstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712"; // Test ID

        [Tooltip("Rewarded Ad Unit ID")]
        public string RewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; // Test ID

        [Tooltip("Banner Ad Unit ID")]
        public string BannerAdUnitId = "ca-app-pub-3940256099942544/6300978111"; // Test ID

        [Header("Ad Settings")]
        public bool EnableAds = true;
        public bool ShowBanner = true;
        public int InterstitialFrequency = 2; // Show after every N levels
        public float MinTimeBetweenInterstitials = 30f; // Minimum seconds between interstitial ads

        [Header("Remote Config Override")]
        public bool UseRemoteConfig = true;

        // Ad instances (uncomment when AdMob is imported)
        // private InterstitialAd _interstitialAd;
        // private RewardedAd _rewardedAd;
        // private BannerView _bannerView;

        private float _lastInterstitialTime = 0f;
        private int _levelsSinceLastInterstitial = 0;
        private Action _onInterstitialClosed;
        private Action _onRewardedCompleted;

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

            Debug.Log("Initializing Ads Manager...");

            // Respect a "No Ads" purchase from a previous session
            if (SaveManager.Instance.HasNoAdsPurchased())
            {
                EnableAds = false;
            }

            if (!EnableAds)
            {
                Debug.Log("Ads are disabled");
                return;
            }

            // Load settings from Remote Config
            if (UseRemoteConfig)
            {
                LoadRemoteConfig();
            }

            // Initialize AdMob
            InitializeAdMob();

            _isInitialized = true;
        }

        private void InitializeAdMob()
        {
            // TODO: Uncomment when AdMob SDK is imported
            /*
            MobileAds.Initialize(initStatus =>
            {
                Debug.Log("AdMob initialized");
                LoadInterstitialAd();
                LoadRewardedAd();

                if (ShowBanner)
                {
                    ShowBannerAd();
                }
            });
            */

            Debug.Log("[MOCK] AdMob would be initialized here");
        }

        private void LoadRemoteConfig()
        {
            // Load from RemoteConfigManager
            InterstitialFrequency = RemoteConfigManager.Instance.GetInt("interstitial_frequency", 2);
            MinTimeBetweenInterstitials = RemoteConfigManager.Instance.GetFloat("min_time_between_interstitials", 30f);
            ShowBanner = RemoteConfigManager.Instance.GetBool("show_banner", true);
        }

        #region Interstitial Ads

        private void LoadInterstitialAd()
        {
            // TODO: Uncomment when AdMob SDK is imported
            /*
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
            }

            AdRequest request = new AdRequest.Builder().Build();

            InterstitialAd.Load(InterstitialAdUnitId, request, (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Interstitial ad failed to load: " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded");
                _interstitialAd = ad;

                // Register events
                _interstitialAd.OnAdFullScreenContentClosed += OnInterstitialClosed;
                _interstitialAd.OnAdFullScreenContentFailed += OnInterstitialFailed;
            });
            */

            Debug.Log("[MOCK] Loading interstitial ad");
        }

        public void ShowInterstitial(Action onClosed = null)
        {
            _onInterstitialClosed = onClosed;

            if (!EnableAds || !CanShowInterstitial())
            {
                Debug.Log("Cannot show interstitial - conditions not met");
                onClosed?.Invoke();
                return;
            }

            // TODO: Uncomment when AdMob SDK is imported
            /*
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                _interstitialAd.Show();
                _lastInterstitialTime = Time.time;
                _levelsSinceLastInterstitial = 0;

                AnalyticsManager.Instance.LogEvent("ad_interstitial_shown");
            }
            else
            {
                Debug.Log("Interstitial ad not ready");
                LoadInterstitialAd(); // Try to load for next time
                onClosed?.Invoke();
            }
            */

            Debug.Log("[MOCK] Showing interstitial ad");
            _lastInterstitialTime = Time.time;
            _levelsSinceLastInterstitial = 0;

            AnalyticsManager.Instance.LogEvent("ad_interstitial_shown", null);

            // Simulate ad closing after 2 seconds
            StartCoroutine(SimulateAdClose(2f, onClosed));
        }

        /// <summary>
        /// Called once per finished level. Every level counts towards the frequency,
        /// even when the minimum time has not passed yet.
        /// </summary>
        private bool CanShowInterstitial()
        {
            _levelsSinceLastInterstitial++;

            // Check level frequency constraint
            if (_levelsSinceLastInterstitial < InterstitialFrequency)
            {
                return false;
            }

            // Check time constraint (first ad no earlier than MinTimeBetweenInterstitials after launch)
            if (Time.time - _lastInterstitialTime < MinTimeBetweenInterstitials)
            {
                return false;
            }

            return true;
        }

        private void OnInterstitialClosed()
        {
            Debug.Log("Interstitial ad closed");
            _onInterstitialClosed?.Invoke();
            _onInterstitialClosed = null;

            // Load next ad
            LoadInterstitialAd();
        }

        private void OnInterstitialFailed(/* AdError error */)
        {
            Debug.LogError("Interstitial ad failed to show");
            _onInterstitialClosed?.Invoke();
            _onInterstitialClosed = null;

            // Load next ad
            LoadInterstitialAd();
        }

        #endregion

        #region Rewarded Ads

        private void LoadRewardedAd()
        {
            // TODO: Uncomment when AdMob SDK is imported
            /*
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
            }

            AdRequest request = new AdRequest.Builder().Build();

            RewardedAd.Load(RewardedAdUnitId, request, (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load: " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded");
                _rewardedAd = ad;

                // Register events
                _rewardedAd.OnAdFullScreenContentClosed += OnRewardedClosed;
                _rewardedAd.OnAdFullScreenContentFailed += OnRewardedFailed;
            });
            */

            Debug.Log("[MOCK] Loading rewarded ad");
        }

        public void ShowRewardedAd(Action onRewardEarned)
        {
            _onRewardedCompleted = onRewardEarned;

            if (!EnableAds)
            {
                Debug.Log("Ads disabled - granting reward anyway");
                onRewardEarned?.Invoke();
                return;
            }

            // TODO: Uncomment when AdMob SDK is imported
            /*
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                _rewardedAd.Show((Reward reward) =>
                {
                    Debug.Log("User earned reward: " + reward.Amount);
                    _onRewardedCompleted?.Invoke();
                    _onRewardedCompleted = null;
                });

                AnalyticsManager.Instance.LogEvent("ad_rewarded_shown");
            }
            else
            {
                Debug.Log("Rewarded ad not ready");
                LoadRewardedAd(); // Try to load for next time

                // Show error to user
                UIManager.Instance.ShowMessage("İnternet bağlantınızı kontrol edin");
            }
            */

            Debug.Log("[MOCK] Showing rewarded ad");
            AnalyticsManager.Instance.LogEvent("ad_rewarded_shown", new System.Collections.Generic.Dictionary<string, object>
            {
                { "placement", "generic" }
            });

            // Simulate ad completion after 3 seconds
            StartCoroutine(SimulateAdClose(3f, onRewardEarned));
        }

        private void OnRewardedClosed()
        {
            Debug.Log("Rewarded ad closed");

            // Load next ad
            LoadRewardedAd();
        }

        private void OnRewardedFailed(/* AdError error */)
        {
            Debug.LogError("Rewarded ad failed to show");
            _onRewardedCompleted = null;

            // Load next ad
            LoadRewardedAd();

            UIManager.Instance.ShowMessage("Reklam yüklenemedi");
        }

        #endregion

        #region Banner Ads

        private void ShowBannerAd()
        {
            if (!ShowBanner) return;

            // TODO: Uncomment when AdMob SDK is imported
            /*
            if (_bannerView != null)
            {
                _bannerView.Destroy();
            }

            _bannerView = new BannerView(BannerAdUnitId, AdSize.Banner, AdPosition.Bottom);

            AdRequest request = new AdRequest.Builder().Build();
            _bannerView.LoadAd(request);

            Debug.Log("Banner ad loaded");
            */

            Debug.Log("[MOCK] Banner ad would be shown at bottom");
        }

        public void HideBannerAd()
        {
            // TODO: Uncomment when AdMob SDK is imported
            /*
            if (_bannerView != null)
            {
                _bannerView.Hide();
            }
            */

            Debug.Log("[MOCK] Hiding banner ad");
        }

        public void DestroyBannerAd()
        {
            // TODO: Uncomment when AdMob SDK is imported
            /*
            if (_bannerView != null)
            {
                _bannerView.Destroy();
                _bannerView = null;
            }
            */

            Debug.Log("[MOCK] Destroying banner ad");
        }

        #endregion

        #region Mock/Testing

        private IEnumerator SimulateAdClose(float delay, Action callback)
        {
            yield return new WaitForSeconds(delay);
            callback?.Invoke();
        }

        #endregion

        private void OnDestroy()
        {
            DestroyBannerAd();
        }
    }
}
