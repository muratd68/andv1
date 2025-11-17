using UnityEngine;
using System.Collections.Generic;

namespace WaterSort.Core
{
    /// <summary>
    /// Performance monitoring and optimization system
    /// Monitors FPS, memory, battery, and automatically adjusts quality
    /// CRITICAL for low-end device support
    /// </summary>
    public class PerformanceManager : MonoBehaviour
    {
        private static PerformanceManager _instance;
        public static PerformanceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("PerformanceManager");
                    _instance = go.AddComponent<PerformanceManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Performance Settings")]
        public bool EnablePerformanceMonitoring = true;
        public bool AutoAdjustQuality = true;
        public float MonitoringInterval = 1f; // Check every second

        [Header("FPS Targets")]
        public int TargetFPS = 60;
        public int MinAcceptableFPS = 30;
        public int LowEndDeviceFPS = 30;

        [Header("Quality Levels")]
        public DevicePerformanceTier CurrentTier = DevicePerformanceTier.High;

        [Header("Runtime Statistics")]
        public float CurrentFPS;
        public float AverageFPS;
        public long UsedMemoryMB;
        public long TotalMemoryMB;
        public float MemoryUsagePercent;

        private float _fpsAccumulator = 0f;
        private int _fpsFrameCount = 0;
        private float _fpsTimer = 0f;

        private List<float> _fpsHistory = new List<float>(60);
        private float _nextMonitorTime = 0f;

        private bool _hasOptimized = false;

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

        public void Initialize()
        {
            // Detect device tier
            DetectDeviceTier();

            // Apply initial settings
            ApplyPerformanceSettings();

            // Set target frame rate
            Application.targetFrameRate = TargetFPS;

            Debug.Log($"PerformanceManager initialized - Device Tier: {CurrentTier}");
        }

        private void Update()
        {
            if (!EnablePerformanceMonitoring) return;

            // Calculate FPS
            CalculateFPS();

            // Monitor performance periodically
            if (Time.time >= _nextMonitorTime)
            {
                MonitorPerformance();
                _nextMonitorTime = Time.time + MonitoringInterval;
            }
        }

        private void CalculateFPS()
        {
            _fpsAccumulator += Time.unscaledDeltaTime;
            _fpsFrameCount++;

            _fpsTimer += Time.unscaledDeltaTime;

            if (_fpsTimer >= 0.5f) // Update twice per second
            {
                CurrentFPS = _fpsFrameCount / _fpsAccumulator;

                // Store in history
                _fpsHistory.Add(CurrentFPS);
                if (_fpsHistory.Count > 60)
                {
                    _fpsHistory.RemoveAt(0);
                }

                // Calculate average
                float sum = 0f;
                foreach (float fps in _fpsHistory)
                {
                    sum += fps;
                }
                AverageFPS = sum / _fpsHistory.Count;

                // Reset
                _fpsAccumulator = 0f;
                _fpsFrameCount = 0;
                _fpsTimer = 0f;
            }
        }

        private void MonitorPerformance()
        {
            // Memory monitoring
            UsedMemoryMB = System.GC.GetTotalMemory(false) / (1024 * 1024);
            TotalMemoryMB = SystemInfo.systemMemorySize;
            MemoryUsagePercent = (float)UsedMemoryMB / TotalMemoryMB * 100f;

            // Auto-adjust quality if FPS drops
            if (AutoAdjustQuality && !_hasOptimized && AverageFPS < MinAcceptableFPS)
            {
                Debug.LogWarning($"Low FPS detected ({AverageFPS:F1}). Reducing quality...");
                ReduceQuality();
                _hasOptimized = true;
            }

            // Memory warning
            if (MemoryUsagePercent > 80f)
            {
                Debug.LogWarning($"High memory usage: {MemoryUsagePercent:F1}%");
                OptimizeMemory();
            }
        }

        private void DetectDeviceTier()
        {
            // Device tier detection based on hardware specs
            int cpuCores = SystemInfo.processorCount;
            int ramMB = SystemInfo.systemMemorySize;
            int gpuMemoryMB = SystemInfo.graphicsMemorySize;

            // High-end device
            if (cpuCores >= 6 && ramMB >= 4096 && gpuMemoryMB >= 2048)
            {
                CurrentTier = DevicePerformanceTier.High;
                TargetFPS = 60;
            }
            // Mid-range device
            else if (cpuCores >= 4 && ramMB >= 2048)
            {
                CurrentTier = DevicePerformanceTier.Medium;
                TargetFPS = 60;
            }
            // Low-end device
            else
            {
                CurrentTier = DevicePerformanceTier.Low;
                TargetFPS = LowEndDeviceFPS;
            }

            Debug.Log($"Device Specs - CPU: {cpuCores} cores, RAM: {ramMB}MB, GPU: {gpuMemoryMB}MB");
        }

        private void ApplyPerformanceSettings()
        {
            switch (CurrentTier)
            {
                case DevicePerformanceTier.High:
                    QualitySettings.SetQualityLevel(5, true); // Ultra
                    Application.targetFrameRate = 60;
                    break;

                case DevicePerformanceTier.Medium:
                    QualitySettings.SetQualityLevel(3, true); // Medium
                    Application.targetFrameRate = 60;
                    break;

                case DevicePerformanceTier.Low:
                    QualitySettings.SetQualityLevel(1, true); // Low
                    Application.targetFrameRate = 30;
                    // Disable expensive features
                    QualitySettings.shadows = ShadowQuality.Disable;
                    QualitySettings.antiAliasing = 0;
                    break;
            }

            // Mobile optimizations
            QualitySettings.vSyncCount = 0; // Disable VSync on mobile
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void ReduceQuality()
        {
            // Progressive quality reduction
            if (CurrentTier == DevicePerformanceTier.High)
            {
                CurrentTier = DevicePerformanceTier.Medium;
                ApplyPerformanceSettings();
            }
            else if (CurrentTier == DevicePerformanceTier.Medium)
            {
                CurrentTier = DevicePerformanceTier.Low;
                ApplyPerformanceSettings();
            }

            Debug.Log($"Quality reduced to: {CurrentTier}");
        }

        private void OptimizeMemory()
        {
            // Force garbage collection
            System.GC.Collect();

            // Unload unused assets
            Resources.UnloadUnusedAssets();

            Debug.Log("Memory optimization performed");
        }

        /// <summary>
        /// Manual performance optimization trigger
        /// Call this when changing scenes or after intensive operations
        /// </summary>
        public void OptimizeNow()
        {
            OptimizeMemory();

            // Clear object pools if needed
            if (MemoryUsagePercent > 70f)
            {
                ObjectPoolManager.Instance?.ClearAllPools();
            }
        }

        /// <summary>
        /// Get device performance tier for other systems to adapt
        /// </summary>
        public DevicePerformanceTier GetDeviceTier()
        {
            return CurrentTier;
        }

        /// <summary>
        /// Check if current device is low-end
        /// </summary>
        public bool IsLowEndDevice()
        {
            return CurrentTier == DevicePerformanceTier.Low;
        }

        /// <summary>
        /// Get current performance grade (A, B, C, D, F)
        /// </summary>
        public string GetPerformanceGrade()
        {
            if (AverageFPS >= 55) return "A";
            if (AverageFPS >= 45) return "B";
            if (AverageFPS >= 35) return "C";
            if (AverageFPS >= 25) return "D";
            return "F";
        }

        /// <summary>
        /// Debug UI display
        /// </summary>
        private void OnGUI()
        {
            if (!EnablePerformanceMonitoring || !Debug.isDebugBuild) return;

            int w = Screen.width, h = Screen.height;
            GUIStyle style = new GUIStyle();
            Rect rect = new Rect(10, 10, w, h * 2 / 50);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 50;
            style.normal.textColor = AverageFPS >= MinAcceptableFPS ? Color.green : Color.red;

            string text = $"FPS: {AverageFPS:F1} | Tier: {CurrentTier} | Mem: {UsedMemoryMB}MB ({MemoryUsagePercent:F1}%)";
            GUI.Label(rect, text, style);
        }
    }

    public enum DevicePerformanceTier
    {
        Low,
        Medium,
        High
    }
}
