using System;
using UnityEngine;
#if WATERSORT_IAP
using Unity.Services.Core;
using UnityEngine.Purchasing;
#endif

namespace WaterSort.Core
{
    /// <summary>
    /// In-app purchases: the non-consumable "Remove Ads" product.
    ///
    /// Requires Unity In-App Purchasing 4.x (Package Manager) and the WATERSORT_IAP
    /// scripting define (Water Sort > Projeyi Kur adds it when the package is detected).
    /// Create a product with the same id in Google Play Console / App Store Connect.
    /// Without the package, purchases are simulated in the Editor and unavailable in builds.
    /// </summary>
    public class IAPManager : MonoBehaviour
#if WATERSORT_IAP
        , IStoreListener
#endif
    {
        private static IAPManager _instance;
        public static IAPManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("IAPManager");
                    _instance = go.AddComponent<IAPManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public static bool Exists => _instance != null;

        [Tooltip("Store product id of the non-consumable Remove Ads product")]
        public string NoAdsProductId = "com.yourcompany.watersort.noads";

        /// <summary>
        /// Raised when Remove Ads becomes owned (purchase or restore)
        /// </summary>
        public event Action NoAdsOwned;

#if WATERSORT_IAP
        private IStoreController _controller;
        private IExtensionProvider _extensions;
        private bool _initializing;
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

        /// <summary>
        /// Restore is required on iOS; Google Play restores non-consumables automatically
        /// </summary>
        public bool NeedsRestoreButton => Application.platform == RuntimePlatform.IPhonePlayer;

        public bool IsReady
        {
            get
            {
#if WATERSORT_IAP
                return _controller != null;
#else
                return Application.isEditor;
#endif
            }
        }

#if WATERSORT_IAP
        public async void Initialize()
        {
            if (_controller != null || _initializing) return;
            _initializing = true;

            try
            {
                // IAP 4.x expects Unity Gaming Services to be initialized first
                await UnityServices.InitializeAsync();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Unity Services init failed (IAP may still work): " + ex.Message);
            }

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder.AddProduct(NoAdsProductId, ProductType.NonConsumable);
            UnityPurchasing.Initialize(this, builder);
        }

        public void BuyNoAds()
        {
            if (_controller == null)
            {
                Toast.Show(Loc.Get("iap.unavailable"));
                Initialize();
                return;
            }

            _controller.InitiatePurchase(NoAdsProductId);
        }

        public void RestorePurchases()
        {
            if (_extensions == null)
            {
                Toast.Show(Loc.Get("iap.unavailable"));
                return;
            }

            _extensions.GetExtension<IAppleExtensions>().RestoreTransactions(success =>
            {
                // Restored products arrive through ProcessPurchase
                if (success) Toast.Show(Loc.Get("iap.restored"));
            });
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            _initializing = false;

            Product product = controller.products.WithID(NoAdsProductId);
            if (product != null && product.hasReceipt)
                GrantNoAds();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            _initializing = false;
            Debug.LogWarning("IAP init failed: " + error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            _initializing = false;
            Debug.LogWarning($"IAP init failed: {error} {message}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            if (args.purchasedProduct.definition.id == NoAdsProductId)
            {
                GrantNoAds();
                Toast.Show(Loc.Get("iap.no_ads_done"));
                AnalyticsManager.Instance.LogPurchase(NoAdsProductId,
                    args.purchasedProduct.metadata.isoCurrencyCode,
                    (double)args.purchasedProduct.metadata.localizedPrice);
            }

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.LogWarning($"Purchase failed: {product?.definition.id} {reason}");
            if (reason != PurchaseFailureReason.UserCancelled)
                Toast.Show(Loc.Get("iap.failed"));
        }
#else
        public void Initialize()
        {
            Debug.Log("[MOCK] IAP not installed. Add Unity In-App Purchasing and run Water Sort > Projeyi Kur.");
        }

        public void BuyNoAds()
        {
            if (Application.isEditor)
            {
                Debug.Log("[MOCK] Remove Ads purchased (Editor only)");
                GrantNoAds();
                Toast.Show(Loc.Get("iap.no_ads_done"));
            }
            else
            {
                Toast.Show(Loc.Get("iap.unavailable"));
            }
        }

        public void RestorePurchases()
        {
            Toast.Show(Loc.Get("iap.unavailable"));
        }
#endif

        private void GrantNoAds()
        {
            if (!SaveManager.Instance.HasNoAdsPurchased())
                SaveManager.Instance.SetNoAdsPurchased(true);
            else
                AdsManager.Instance.RemoveAds();

            NoAdsOwned?.Invoke();
        }
    }
}
