using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if WATERSORT_ADMOB
using GoogleMobileAds.Api;
#endif

namespace WaterSort.Core
{
    /// <summary>
    /// Central ad management system: interstitial, rewarded and banner ads.
    ///
    /// Without the Google Mobile Ads SDK the ads are simulated (mock) so the game
    /// flow can be tested. With the SDK imported, the scripting define symbol
    /// WATERSORT_ADMOB enables real AdMob ads (Water Sort > Projeyi Kur adds it
    /// automatically when the SDK is detected). Requires Google Mobile Ads Unity plugin 8.0+.
    ///
    /// "Remove Ads" (IAP) removes interstitials and the banner; rewarded ads stay,
    /// because the player chooses to watch them for a reward.
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

        [Header("Ad Unit IDs - Google test IDs, REPLACE WITH YOUR IDS before release")]
        [Tooltip("Android interstitial ad unit")]
        public string AndroidInterstitialId = "ca-app-pub-3940256099942544/1033173712";
        [Tooltip("Android rewarded ad unit")]
        public string AndroidRewardedId = "ca-app-pub-3940256099942544/5224354917";
        [Tooltip("Android banner ad unit")]
        public string AndroidBannerId = "ca-app-pub-3940256099942544/6300978111";

        [Tooltip("iOS interstitial ad unit")]
        public string IosInterstitialId = "ca-app-pub-3940256099942544/4411468910";
        [Tooltip("iOS rewarded ad unit")]
        public string IosRewardedId = "ca-app-pub-3940256099942544/1712485313";
        [Tooltip("iOS banner ad unit")]
        public string IosBannerId = "ca-app-pub-3940256099942544/2934735716";

        [Header("Ad Settings")]
        [Tooltip("Master switch. When off, rewarded actions are granted without an ad (for testing).")]
        public bool EnableAds = true;
        public bool ShowBanner = true;
        public int InterstitialFrequency = 2; // Show after every N levels
        public float MinTimeBetweenInterstitials = 30f; // Minimum seconds between interstitial ads

        [Header("Remote Config Override")]
        public bool UseRemoteConfig = true;

        [Header("Mock (no SDK)")]
        public float MockInterstitialSeconds = 2f;
        public float MockRewardedSeconds = 3f;

        /// <summary>
        /// True after the "Remove Ads" purchase: no interstitials, no banner
        /// </summary>
        public bool AdsRemoved { get; private set; }

        private float _lastInterstitialTime = 0f;
        private int _levelsSinceLastInterstitial = 0;
        private bool _isInitialized = false;

        // SDK callbacks can arrive on a background thread; run them in Update instead
        private readonly Queue<Action> _mainThreadActions = new Queue<Action>();

#if WATERSORT_ADMOB
        private InterstitialAd _interstitialAd;
        private RewardedAd _rewardedAd;
        private BannerView _bannerView;
        private Action _onInterstitialClosed;
#endif

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

        private void Update()
        {
            while (true)
            {
                Action action;
                lock (_mainThreadActions)
                {
                    if (_mainThreadActions.Count == 0) break;
                    action = _mainThreadActions.Dequeue();
                }
                action?.Invoke();
            }
        }

        private void RunOnMainThread(Action action)
        {
            lock (_mainThreadActions)
            {
                _mainThreadActions.Enqueue(action);
            }
        }

        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            Debug.Log("Initializing Ads Manager...");

            // Respect a "Remove Ads" purchase from a previous session
            AdsRemoved = SaveManager.Instance.HasNoAdsPurchased();

            if (!EnableAds)
            {
                Debug.Log("Ads are disabled");
                return;
            }

            if (UseRemoteConfig)
            {
                LoadRemoteConfig();
                // Pick up fetched values when Remote Config finishes
                RemoteConfigManager.Instance.ConfigUpdated += LoadRemoteConfig;
            }

            InitializeSdk();
        }

        private void LoadRemoteConfig()
        {
            InterstitialFrequency = RemoteConfigManager.Instance.GetInt("interstitial_frequency", InterstitialFrequency);
            MinTimeBetweenInterstitials = RemoteConfigManager.Instance.GetFloat("min_time_between_interstitials", MinTimeBetweenInterstitials);
            ShowBanner = RemoteConfigManager.Instance.GetBool("show_banner", ShowBanner);
        }

        /// <summary>
        /// Called after a successful "Remove Ads" purchase
        /// </summary>
        public void RemoveAds()
        {
            AdsRemoved = true;
            DestroyBannerAd();
        }

        #region Interstitial Ads

        public void ShowInterstitial(Action onClosed = null)
        {
            if (!EnableAds || AdsRemoved || !CanShowInterstitial())
            {
                onClosed?.Invoke();
                return;
            }

            _lastInterstitialTime = Time.time;
            _levelsSinceLastInterstitial = 0;

#if WATERSORT_ADMOB
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                _onInterstitialClosed = onClosed;
                _interstitialAd.Show();
                AnalyticsManager.Instance.LogEvent("ad_interstitial_shown", null);
            }
            else
            {
                Debug.Log("Interstitial ad not ready");
                LoadInterstitialAd();
                onClosed?.Invoke();
            }
#else
            Debug.Log("[MOCK] Showing interstitial ad");
            Toast.Show(Loc.Get("ads.mock"), MockInterstitialSeconds);
            AnalyticsManager.Instance.LogEvent("ad_interstitial_shown", null);
            StartCoroutine(InvokeAfter(MockInterstitialSeconds, onClosed));
#endif
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

        #endregion

        #region Rewarded Ads

        /// <summary>
        /// Shows a rewarded ad; onRewardEarned runs only if the player watched it to the end
        /// </summary>
        public void ShowRewardedAd(Action onRewardEarned)
        {
            if (!EnableAds)
            {
                Debug.Log("Ads disabled - granting reward anyway");
                onRewardEarned?.Invoke();
                return;
            }

#if WATERSORT_ADMOB
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                AnalyticsManager.Instance.LogEvent("ad_rewarded_shown", null);
                _rewardedAd.Show((Reward reward) => RunOnMainThread(onRewardEarned));
            }
            else
            {
                Debug.Log("Rewarded ad not ready");
                LoadRewardedAd();
                Toast.Show(Loc.Get("ads.no_connection"));
            }
#else
            Debug.Log("[MOCK] Showing rewarded ad");
            Toast.Show(Loc.Get("ads.mock"), MockRewardedSeconds);
            AnalyticsManager.Instance.LogEvent("ad_rewarded_shown", null);
            StartCoroutine(InvokeAfter(MockRewardedSeconds, onRewardEarned));
#endif
        }

        #endregion

        #region Banner Ads

        public void HideBannerAd()
        {
#if WATERSORT_ADMOB
            _bannerView?.Hide();
#endif
        }

        public void DestroyBannerAd()
        {
#if WATERSORT_ADMOB
            if (_bannerView != null)
            {
                _bannerView.Destroy();
                _bannerView = null;
            }
#endif
        }

        #endregion

        #region SDK

#if WATERSORT_ADMOB
        private string InterstitialId => Application.platform == RuntimePlatform.IPhonePlayer ? IosInterstitialId : AndroidInterstitialId;
        private string RewardedId => Application.platform == RuntimePlatform.IPhonePlayer ? IosRewardedId : AndroidRewardedId;
        private string BannerId => Application.platform == RuntimePlatform.IPhonePlayer ? IosBannerId : AndroidBannerId;

        private void InitializeSdk()
        {
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            MobileAds.Initialize(initStatus => RunOnMainThread(() =>
            {
                Debug.Log("AdMob initialized");
                LoadInterstitialAd();
                LoadRewardedAd();

                if (ShowBanner && !AdsRemoved)
                    ShowBannerAd();
            }));
        }

        private void LoadInterstitialAd()
        {
            if (AdsRemoved) return;

            _interstitialAd?.Destroy();
            _interstitialAd = null;

            InterstitialAd.Load(InterstitialId, new AdRequest(), (InterstitialAd ad, LoadAdError error) => RunOnMainThread(() =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning("Interstitial ad failed to load: " + error);
                    return;
                }

                _interstitialAd = ad;
                ad.OnAdFullScreenContentClosed += () => RunOnMainThread(OnInterstitialFinished);
                ad.OnAdFullScreenContentFailed += adError => RunOnMainThread(OnInterstitialFinished);
            }));
        }

        private void OnInterstitialFinished()
        {
            Action callback = _onInterstitialClosed;
            _onInterstitialClosed = null;
            callback?.Invoke();

            LoadInterstitialAd();
        }

        private void LoadRewardedAd()
        {
            _rewardedAd?.Destroy();
            _rewardedAd = null;

            RewardedAd.Load(RewardedId, new AdRequest(), (RewardedAd ad, LoadAdError error) => RunOnMainThread(() =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning("Rewarded ad failed to load: " + error);
                    return;
                }

                _rewardedAd = ad;
                ad.OnAdFullScreenContentClosed += () => RunOnMainThread(LoadRewardedAd);
                ad.OnAdFullScreenContentFailed += adError => RunOnMainThread(() =>
                {
                    Toast.Show(Loc.Get("ads.load_failed"));
                    LoadRewardedAd();
                });
            }));
        }

        private void ShowBannerAd()
        {
            DestroyBannerAd();
            _bannerView = new BannerView(BannerId, AdSize.Banner, AdPosition.Bottom);
            _bannerView.LoadAd(new AdRequest());
        }
#else
        private void InitializeSdk()
        {
            Debug.Log("[MOCK] Ads run in mock mode. Import the Google Mobile Ads SDK and run Water Sort > Projeyi Kur to enable AdMob.");
        }
#endif

        #endregion

        private IEnumerator InvokeAfter(float delay, Action callback)
        {
            yield return new WaitForSecondsRealtime(delay);
            callback?.Invoke();
        }

        private void OnDestroy()
        {
            DestroyBannerAd();
        }
    }
}
