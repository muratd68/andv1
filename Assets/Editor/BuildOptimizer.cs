using UnityEditor;
using UnityEngine;

namespace WaterSort.Editor
{
    /// <summary>
    /// Build optimization script - Automates build settings for maximum performance
    /// Run before creating production builds
    /// </summary>
    public class BuildOptimizer : EditorWindow
    {
        [MenuItem("Water Sort/Build Optimizer")]
        public static void ShowWindow()
        {
            GetWindow<BuildOptimizer>("Build Optimizer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Build Optimization Tools", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Optimize for Android Release", GUILayout.Height(40)))
            {
                OptimizeAndroidBuild();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Optimize for iOS Release", GUILayout.Height(40)))
            {
                OptimizeIOSBuild();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Optimize for Development Build", GUILayout.Height(40)))
            {
                OptimizeDevelopmentBuild();
            }

            GUILayout.Space(20);
            GUILayout.Label("Quick Optimizations", EditorStyles.boldLabel);

            if (GUILayout.Button("Strip Engine Code"))
            {
                StripEngineCode();
            }

            if (GUILayout.Button("Optimize Texture Sizes"))
            {
                OptimizeTextures();
            }

            if (GUILayout.Button("Setup IL2CPP"))
            {
                SetupIL2CPP();
            }
        }

        private static void OptimizeAndroidBuild()
        {
            Debug.Log("Optimizing Android build settings...");

            // Graphics settings
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

            // Scripting backend
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

            // Code stripping
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.High);

            // API compatibility
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Standard_2_0);

            // Graphics API
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] {
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3,
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2
            });

            // APK expansion files
            PlayerSettings.Android.useAPKExpansionFiles = false; // For small games

            // Optimize mesh data
            PlayerSettings.stripUnusedMeshComponents = true;

            // Multithreaded rendering
            PlayerSettings.MTRendering = true;
            PlayerSettings.graphicsJobs = true;

            Debug.Log("Android build optimized!");
            EditorUtility.DisplayDialog("Success", "Android build settings optimized for maximum performance!", "OK");
        }

        private static void OptimizeIOSBuild()
        {
            Debug.Log("Optimizing iOS build settings...");

            // Scripting backend
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);

            // Code stripping
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.iOS, ManagedStrippingLevel.High);

            // API compatibility
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, ApiCompatibilityLevel.NET_Standard_2_0);

            // Graphics API
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new UnityEngine.Rendering.GraphicsDeviceType[] {
                UnityEngine.Rendering.GraphicsDeviceType.Metal
            });

            // Target iOS version
            PlayerSettings.iOS.targetOSVersionString = "11.0";

            // Optimize mesh data
            PlayerSettings.stripUnusedMeshComponents = true;

            // Multithreaded rendering
            PlayerSettings.MTRendering = true;
            PlayerSettings.graphicsJobs = true;

            Debug.Log("iOS build optimized!");
            EditorUtility.DisplayDialog("Success", "iOS build settings optimized for maximum performance!", "OK");
        }

        private static void OptimizeDevelopmentBuild()
        {
            Debug.Log("Optimizing development build settings...");

            // Enable development settings
            EditorUserBuildSettings.development = true;
            EditorUserBuildSettings.connectProfiler = true;

            // Disable optimizations for faster iteration
            PlayerSettings.stripEngineCode = false;

            Debug.Log("Development build optimized!");
            EditorUtility.DisplayDialog("Success", "Development build settings configured!", "OK");
        }

        private static void StripEngineCode()
        {
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.High);
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.iOS, ManagedStrippingLevel.High);

            Debug.Log("Engine code stripping enabled!");
        }

        private static void OptimizeTextures()
        {
            Debug.Log("Optimizing textures...");

            string[] texturePaths = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
            int optimizedCount = 0;

            foreach (string guid in texturePaths)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null)
                {
                    // Android settings
                    TextureImporterPlatformSettings androidSettings = importer.GetPlatformTextureSettings("Android");
                    androidSettings.overridden = true;
                    androidSettings.maxTextureSize = 2048; // Max 2K textures
                    androidSettings.format = TextureImporterFormat.ETC2_RGBA8;
                    androidSettings.compressionQuality = (int)UnityEditor.TextureCompressionQuality.Normal;
                    importer.SetPlatformTextureSettings(androidSettings);

                    // iOS settings
                    TextureImporterPlatformSettings iosSettings = importer.GetPlatformTextureSettings("iPhone");
                    iosSettings.overridden = true;
                    iosSettings.maxTextureSize = 2048;
                    iosSettings.format = TextureImporterFormat.ASTC_6x6;
                    iosSettings.compressionQuality = (int)UnityEditor.TextureCompressionQuality.Normal;
                    importer.SetPlatformTextureSettings(iosSettings);

                    // Common settings
                    importer.mipmapEnabled = false; // For UI textures
                    importer.isReadable = false; // Reduce memory

                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();

                    optimizedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Optimized {optimizedCount} textures!");
            EditorUtility.DisplayDialog("Success", $"Optimized {optimizedCount} textures for mobile platforms!", "OK");
        }

        private static void SetupIL2CPP()
        {
            // Android
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

            // iOS
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);

            Debug.Log("IL2CPP configured for Android and iOS!");
            EditorUtility.DisplayDialog("Success", "IL2CPP scripting backend configured!", "OK");
        }

        [MenuItem("Water Sort/Quick Build/Android Release")]
        public static void QuickBuildAndroid()
        {
            OptimizeAndroidBuild();

            string path = EditorUtility.SaveFilePanel("Save Android Build", "", "WaterSort", "apk");

            if (!string.IsNullOrEmpty(path))
            {
                BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, path, BuildTarget.Android, BuildOptions.None);
                Debug.Log($"Android build completed: {path}");
            }
        }

        [MenuItem("Water Sort/Quick Build/iOS Release")]
        public static void QuickBuildIOS()
        {
            OptimizeIOSBuild();

            string path = EditorUtility.SaveFolderPanel("Save iOS Build", "", "iOSBuild");

            if (!string.IsNullOrEmpty(path))
            {
                BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, path, BuildTarget.iOS, BuildOptions.None);
                Debug.Log($"iOS build completed: {path}");
            }
        }
    }
}
