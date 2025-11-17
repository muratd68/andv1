using UnityEngine;
using UnityEngine.UI;

namespace WaterSort.Core
{
    /// <summary>
    /// Device Compatibility Manager - Ensures game works on ALL devices
    /// Handles screen sizes, notches, Android versions, low-RAM devices
    /// CRITICAL for maximum device support
    /// </summary>
    public class DeviceCompatibilityManager : MonoBehaviour
    {
        private static DeviceCompatibilityManager _instance;
        public static DeviceCompatibilityManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("DeviceCompatibilityManager");
                    _instance = go.AddComponent<DeviceCompatibilityManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Screen Settings")]
        public CanvasScaler MainCanvasScaler;
        public float ReferenceWidth = 1080f;
        public float ReferenceHeight = 1920f;

        [Header("Device Info")]
        public string DeviceModel;
        public string OS;
        public int AndroidAPILevel;
        public bool HasNotch;
        public float AspectRatio;
        public int ScreenWidth;
        public int ScreenHeight;
        public int DPI;

        [Header("Compatibility Flags")]
        public bool SupportsVibration = true;
        public bool SupportsHighFPS = true;
        public bool IsLowRAMDevice = false;
        public bool IsOldAndroidVersion = false;

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

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            GatherDeviceInfo();
            ApplyCompatibilitySettings();
            HandleSafeArea();

            Debug.Log($"Device Compatibility initialized for: {DeviceModel}");
        }

        private void GatherDeviceInfo()
        {
            DeviceModel = SystemInfo.deviceModel;
            OS = SystemInfo.operatingSystem;
            ScreenWidth = Screen.width;
            ScreenHeight = Screen.height;
            DPI = (int)Screen.dpi;

            // Calculate aspect ratio
            AspectRatio = (float)ScreenWidth / ScreenHeight;

            // Check for notch (aspect ratio > 2:1 usually indicates notch)
            HasNotch = AspectRatio > 2.0f || AspectRatio < 0.5f;

            // Android specific
            #if UNITY_ANDROID
            AndroidAPILevel = GetAndroidAPILevel();
            IsOldAndroidVersion = AndroidAPILevel < 21; // Below Android 5.0

            // Check if vibration is supported
            SupportsVibration = SystemInfo.supportsVibration;
            #endif

            // RAM check
            int systemMemoryMB = SystemInfo.systemMemorySize;
            IsLowRAMDevice = systemMemoryMB < 2048; // Less than 2GB

            // High FPS support
            int refreshRate = Screen.currentResolution.refreshRate;
            SupportsHighFPS = refreshRate >= 90;

            Debug.Log($"Device: {DeviceModel}, OS: {OS}, RAM: {systemMemoryMB}MB, Screen: {ScreenWidth}x{ScreenHeight}, DPI: {DPI}");
        }

        private int GetAndroidAPILevel()
        {
            #if UNITY_ANDROID
            try
            {
                using (AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    return version.GetStatic<int>("SDK_INT");
                }
            }
            catch
            {
                return 0;
            }
            #else
            return 0;
            #endif
        }

        private void ApplyCompatibilitySettings()
        {
            // Screen orientation - lock to portrait for mobile
            #if UNITY_ANDROID || UNITY_IOS
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            #endif

            // Frame rate based on device capability
            if (SupportsHighFPS && !IsLowRAMDevice)
            {
                Application.targetFrameRate = 60;
            }
            else
            {
                Application.targetFrameRate = 30;
            }

            // Old Android compatibility
            if (IsOldAndroidVersion)
            {
                // Disable heavy features for old devices
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.antiAliasing = 0;
            }

            // Low RAM optimizations
            if (IsLowRAMDevice)
            {
                QualitySettings.masterTextureLimit = 1; // Reduce texture quality
                QualitySettings.particleRaycastBudget = 64; // Reduce particle budget
            }

            // DPI scaling
            ApplyDPIScaling();
        }

        private void ApplyDPIScaling()
        {
            if (DPI < 1) DPI = 160; // Default DPI if not detected

            float dpiScale = DPI / 160f; // 160 is baseline DPI

            // Adjust UI scale based on DPI
            if (MainCanvasScaler != null)
            {
                MainCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                MainCanvasScaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
                MainCanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

                // Match based on aspect ratio
                if (AspectRatio > 1.5f) // Wide screen
                {
                    MainCanvasScaler.matchWidthOrHeight = 0f; // Match width
                }
                else if (AspectRatio < 0.6f) // Tall screen
                {
                    MainCanvasScaler.matchWidthOrHeight = 1f; // Match height
                }
                else
                {
                    MainCanvasScaler.matchWidthOrHeight = 0.5f; // Balance
                }
            }
        }

        private void HandleSafeArea()
        {
            if (!HasNotch) return;

            Rect safeArea = Screen.safeArea;

            // Calculate safe area margins
            float leftMargin = safeArea.x;
            float rightMargin = Screen.width - (safeArea.x + safeArea.width);
            float topMargin = Screen.height - (safeArea.y + safeArea.height);
            float bottomMargin = safeArea.y;

            Debug.Log($"Safe Area - L:{leftMargin} R:{rightMargin} T:{topMargin} B:{bottomMargin}");

            // Apply safe area to UI (you'll need to call this for each UI panel)
            // Example in your UI scripts:
            // DeviceCompatibilityManager.Instance.ApplySafeAreaToPa nel(yourRectTransform);
        }

        /// <summary>
        /// Apply safe area margins to a UI panel
        /// Call this for top/bottom panels to avoid notch overlap
        /// </summary>
        public void ApplySafeAreaToPanel(RectTransform panel)
        {
            if (panel == null) return;

            Rect safeArea = Screen.safeArea;
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;
        }

        /// <summary>
        /// Check if device supports a specific feature
        /// </summary>
        public bool SupportsFeature(DeviceFeature feature)
        {
            switch (feature)
            {
                case DeviceFeature.Vibration:
                    return SupportsVibration;

                case DeviceFeature.HighFPS:
                    return SupportsHighFPS && !IsLowRAMDevice;

                case DeviceFeature.AdvancedGraphics:
                    return !IsLowRAMDevice && !IsOldAndroidVersion;

                case DeviceFeature.ParticleEffects:
                    return !IsLowRAMDevice;

                case DeviceFeature.PostProcessing:
                    return !IsLowRAMDevice && SystemInfo.systemMemorySize >= 3072;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Get recommended quality level for this device
        /// </summary>
        public int GetRecommendedQualityLevel()
        {
            if (IsLowRAMDevice || IsOldAndroidVersion)
                return 0; // Low

            if (SystemInfo.systemMemorySize >= 4096 && SystemInfo.processorCount >= 6)
                return 2; // High

            return 1; // Medium
        }

        /// <summary>
        /// Get maximum recommended pool sizes for this device
        /// </summary>
        public int GetMaxPoolSize()
        {
            if (IsLowRAMDevice)
                return 50; // Conservative

            if (SystemInfo.systemMemorySize >= 4096)
                return 200; // Generous

            return 100; // Default
        }

        /// <summary>
        /// Check if device needs special handling
        /// </summary>
        public bool IsProblematicDevice()
        {
            // List of known problematic devices/patterns
            string model = DeviceModel.ToLower();

            // Very old devices
            if (IsOldAndroidVersion) return true;

            // Specific problematic models (add as needed)
            if (model.Contains("sm-j") || model.Contains("sm-a") && SystemInfo.systemMemorySize < 2048)
                return true;

            return false;
        }

        /// <summary>
        /// Apply emergency low-performance mode
        /// </summary>
        public void EnableEmergencyMode()
        {
            Debug.LogWarning("Enabling Emergency Performance Mode");

            Application.targetFrameRate = 20;
            QualitySettings.SetQualityLevel(0, true);
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.antiAliasing = 0;
            QualitySettings.masterTextureLimit = 2;

            // Disable all visual effects
            // You can add more here based on your game

            AnalyticsManager.Instance?.LogEvent("emergency_mode_enabled", new System.Collections.Generic.Dictionary<string, object>
            {
                { "device", DeviceModel },
                { "ram", SystemInfo.systemMemorySize }
            });
        }

        /// <summary>
        /// Test performance and auto-adjust
        /// Call this after a few seconds of gameplay
        /// </summary>
        public void AutoTunePerformance()
        {
            float avgFPS = PerformanceManager.Instance?.AverageFPS ?? 60f;

            if (avgFPS < 20f)
            {
                Debug.LogWarning($"Very low FPS detected: {avgFPS}. Enabling emergency mode.");
                EnableEmergencyMode();
            }
            else if (avgFPS < 30f)
            {
                Debug.LogWarning($"Low FPS detected: {avgFPS}. Reducing quality.");
                QualitySettings.DecreaseLevel();
            }
        }

        /// <summary>
        /// Log device info for analytics
        /// </summary>
        public void ReportDeviceInfo()
        {
            AnalyticsManager.Instance?.LogEvent("device_info", new System.Collections.Generic.Dictionary<string, object>
            {
                { "model", DeviceModel },
                { "os", OS },
                { "ram_mb", SystemInfo.systemMemorySize },
                { "cpu_cores", SystemInfo.processorCount },
                { "gpu", SystemInfo.graphicsDeviceName },
                { "screen_width", ScreenWidth },
                { "screen_height", ScreenHeight },
                { "dpi", DPI },
                { "has_notch", HasNotch },
                { "aspect_ratio", AspectRatio }
            });
        }
    }

    public enum DeviceFeature
    {
        Vibration,
        HighFPS,
        AdvancedGraphics,
        ParticleEffects,
        PostProcessing
    }
}
