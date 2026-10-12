// Compile-only stand-ins for the optional SDKs, used when building with -p:WithSdks=true.
// They mirror the public API of Google Mobile Ads Unity plugin 8.x/9.x, Firebase Unity SDK
// (Analytics, Remote Config) and Unity In-App Purchasing 4.x, as used by this project.
// They only prove our code is consistent with that API; the real SDKs are checked inside Unity.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleMobileAds.Api
{
    public class InitializationStatus { }
    public class AdRequest { }
    public class AdError { }
    public class LoadAdError : AdError { }
    public class Reward { public double Amount; public string Type; }
    public class AdSize { public static readonly AdSize Banner = new AdSize(); }
    public enum AdPosition { Top, Bottom }

    public static class MobileAds
    {
        public static bool RaiseAdEventsOnUnityMainThread { get; set; }
        public static void Initialize(Action<InitializationStatus> initCompleteAction) { }
    }

    public class InterstitialAd
    {
        public event Action OnAdFullScreenContentClosed;
        public event Action<AdError> OnAdFullScreenContentFailed;
        public static void Load(string adUnitId, AdRequest request, Action<InterstitialAd, LoadAdError> callback) { }
        public bool CanShowAd() => false;
        public void Show() { OnAdFullScreenContentClosed?.Invoke(); OnAdFullScreenContentFailed?.Invoke(null); }
        public void Destroy() { }
    }

    public class RewardedAd
    {
        public event Action OnAdFullScreenContentClosed;
        public event Action<AdError> OnAdFullScreenContentFailed;
        public static void Load(string adUnitId, AdRequest request, Action<RewardedAd, LoadAdError> callback) { }
        public bool CanShowAd() => false;
        public void Show(Action<Reward> userRewardEarnedCallback) { OnAdFullScreenContentClosed?.Invoke(); OnAdFullScreenContentFailed?.Invoke(null); }
        public void Destroy() { }
    }

    public class BannerView
    {
        public BannerView(string adUnitId, AdSize adSize, AdPosition position) { }
        public void LoadAd(AdRequest request) { }
        public void Hide() { }
        public void Show() { }
        public void Destroy() { }
    }
}

namespace Firebase
{
    public enum DependencyStatus { Available, UnavailableOther }

    public class FirebaseApp
    {
        public static Task<DependencyStatus> CheckAndFixDependenciesAsync() => Task.FromResult(DependencyStatus.Available);
    }
}

namespace Firebase.Extensions
{
    public static class TaskExtension
    {
        public static Task ContinueWithOnMainThread(this Task task, Action<Task> continuation) => task;
        public static Task ContinueWithOnMainThread<T>(this Task<T> task, Action<Task<T>> continuation) => task;
    }
}

namespace Firebase.Analytics
{
    public class Parameter
    {
        public Parameter(string name, long value) { }
        public Parameter(string name, double value) { }
        public Parameter(string name, string value) { }
    }

    public static class FirebaseAnalytics
    {
        public static void SetAnalyticsCollectionEnabled(bool enabled) { }
        public static void LogEvent(string name) { }
        public static void LogEvent(string name, params Parameter[] parameters) { }
        public static void SetUserProperty(string name, string property) { }
    }
}

namespace Firebase.RemoteConfig
{
    public struct ConfigValue
    {
        public long LongValue => 0;
        public double DoubleValue => 0;
        public bool BooleanValue => false;
        public string StringValue => "";
    }

    public class FirebaseRemoteConfig
    {
        public static FirebaseRemoteConfig DefaultInstance { get; } = new FirebaseRemoteConfig();
        public Task SetDefaultsAsync(IDictionary<string, object> defaults) => Task.CompletedTask;
        public Task FetchAsync(TimeSpan cacheExpiration) => Task.CompletedTask;
        public Task<bool> ActivateAsync() => Task.FromResult(true);
        public ConfigValue GetValue(string key) => default;
    }
}

namespace Unity.Services.Core
{
    public static class UnityServices
    {
        public static Task InitializeAsync() => Task.CompletedTask;
    }
}

namespace UnityEngine.Purchasing
{
    public enum ProductType { Consumable, NonConsumable, Subscription }
    public enum PurchaseProcessingResult { Complete, Pending }
    public enum InitializationFailureReason { PurchasingUnavailable, NoProductsAvailable, AppNotKnown }
    public enum PurchaseFailureReason { PurchasingUnavailable, ExistingPurchasePending, ProductUnavailable, SignatureInvalid, UserCancelled, PaymentDeclined, DuplicateTransaction, Unknown }

    public class ProductDefinition { public string id; }
    public class ProductMetadata { public string isoCurrencyCode; public decimal localizedPrice; }
    public class Product { public ProductDefinition definition; public bool hasReceipt; public ProductMetadata metadata; }
    public class ProductCollection { public Product WithID(string id) => null; }
    public class PurchaseEventArgs { public Product purchasedProduct; }

    public interface IStoreController
    {
        ProductCollection products { get; }
        void InitiatePurchase(string productId);
    }

    public interface IStoreExtension { }
    public interface IAppleExtensions : IStoreExtension { void RestoreTransactions(Action<bool> callback); }
    public interface IExtensionProvider { T GetExtension<T>() where T : IStoreExtension; }

    public interface IStoreListener
    {
        void OnInitialized(IStoreController controller, IExtensionProvider extensions);
        void OnInitializeFailed(InitializationFailureReason error);
        void OnInitializeFailed(InitializationFailureReason error, string message);
        PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent);
        void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason);
    }

    public interface IPurchasingModule { }
    public class StandardPurchasingModule : IPurchasingModule { public static StandardPurchasingModule Instance() => new StandardPurchasingModule(); }

    public class ConfigurationBuilder
    {
        public static ConfigurationBuilder Instance(IPurchasingModule first, params IPurchasingModule[] rest) => new ConfigurationBuilder();
        public ConfigurationBuilder AddProduct(string id, ProductType type) => this;
    }

    public static class UnityPurchasing
    {
        public static void Initialize(IStoreListener listener, ConfigurationBuilder builder) { }
    }
}
