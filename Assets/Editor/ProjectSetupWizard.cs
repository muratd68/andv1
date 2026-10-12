using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WaterSort.Core;

namespace WaterSort.EditorTools
{
    /// <summary>
    /// One-click project setup: generates sprites, prefabs and the Bootstrap, MainMenu
    /// and Game scenes with all UI references wired, then registers the scenes in
    /// Build Settings. Menu: Water Sort > Projeyi Kur.
    /// Safe to run again; it regenerates everything it owns.
    /// </summary>
    public static class ProjectSetupWizard
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string ArtFolder = "Assets/Art/Generated";
        private const string PrefabFolder = "Assets/Prefabs";
        private const string ResourcesPrefabFolder = "Assets/Resources/Prefabs";

        private const string BootstrapScene = ScenesFolder + "/Bootstrap.unity";
        private const string MainMenuScene = ScenesFolder + "/MainMenu.unity";
        private const string GameScene = ScenesFolder + "/Game.unity";

        private const string TubeSpritePath = ArtFolder + "/Tube.png";
        private const string SegmentSpritePath = ArtFolder + "/LiquidSegment.png";
        private const string SegmentPrefabPath = ResourcesPrefabFolder + "/LiquidSegment.prefab"; // LevelManager loads Resources/Prefabs/LiquidSegment
        private const string TubePrefabPath = PrefabFolder + "/Tube.prefab";
        private const string LevelButtonPrefabPath = PrefabFolder + "/UI/LevelButton.prefab";

        private const string TmpPackageName = "com.unity.textmeshpro";
        private const string TmpEssentialsFile = "Package Resources/TMP Essential Resources.unitypackage";

        // World sizes (100 pixels per unit)
        private const int PixelsPerUnit = 100;
        private const int TubeWidthPx = 80;
        private const int TubeHeightPx = 210;
        private const int SegmentWidthPx = 62;
        private const int SegmentHeightPx = 48;

        // Palette (matches the web preview)
        private static readonly Color Ink = Hex("0D2229");
        private static readonly Color Panel = Hex("133540");
        private static readonly Color Accent = Hex("FFD166");
        private static readonly Color AccentInk = Hex("2A1D00");
        private static readonly Color Text = Hex("EAF7F8");
        private static readonly Color Muted = Hex("8FB3B8");
        private static readonly Color Good = Hex("7EE0A1");
        private static readonly Color Scrim = new Color(0.02f, 0.06f, 0.07f, 0.75f);

        #region Menu

        [MenuItem("Water Sort/Projeyi Kur (Sahneler + Prefablar)", priority = 0)]
        public static void SetupProject()
        {
            if (!EnsureTmpEssentials())
                return;

            if (File.Exists(GameScene) &&
                !EditorUtility.DisplayDialog("Water Sort Kurulum",
                    "Sahneler ve prefablar zaten var. Yeniden oluşturulsun mu?\n\nSahnelerde elle yaptığın değişiklikler silinir.",
                    "Yeniden Oluştur", "Vazgeç"))
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            try
            {
                EditorUtility.DisplayProgressBar("Water Sort Kurulum", "Sprite'lar oluşturuluyor", 0.1f);
                EnsureFolder(ScenesFolder);
                EnsureFolder(ArtFolder);
                EnsureFolder(PrefabFolder + "/UI");
                EnsureFolder(ResourcesPrefabFolder);

                Sprite tubeSprite = CreateTubeSprite();
                Sprite segmentSprite = CreateSegmentSprite();

                EditorUtility.DisplayProgressBar("Water Sort Kurulum", "Prefablar oluşturuluyor", 0.3f);
                GameObject segmentPrefab = CreateSegmentPrefab(segmentSprite);
                GameObject tubePrefab = CreateTubePrefab(tubeSprite, segmentPrefab);
                Button levelButtonPrefab = CreateLevelButtonPrefab();

                EditorUtility.DisplayProgressBar("Water Sort Kurulum", "Bootstrap sahnesi", 0.5f);
                CreateBootstrapScene();

                EditorUtility.DisplayProgressBar("Water Sort Kurulum", "Ana menü sahnesi", 0.65f);
                CreateMainMenuScene(levelButtonPrefab);

                EditorUtility.DisplayProgressBar("Water Sort Kurulum", "Oyun sahnesi", 0.8f);
                CreateGameScene(tubePrefab);

                EditorBuildSettings.scenes = new[]
                {
                    new EditorBuildSettingsScene(BootstrapScene, true),
                    new EditorBuildSettingsScene(MainMenuScene, true),
                    new EditorBuildSettingsScene(GameScene, true)
                };

                PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

                AssetDatabase.SaveAssets();
                EditorSceneManager.OpenScene(BootstrapScene);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog("Water Sort Kurulum",
                "Hazır!\n\nSahneler: Bootstrap, MainMenu, Game (Build Settings'e eklendi)\n" +
                "Prefablar: Tube, LiquidSegment, LevelButton\n\n" +
                "Oynamak için: Water Sort > Oyunu Başlat",
                "Tamam");
        }

        [MenuItem("Water Sort/Oyunu Başlat", priority = 1)]
        public static void PlayFromBootstrap()
        {
            if (!File.Exists(BootstrapScene))
            {
                EditorUtility.DisplayDialog("Water Sort", "Önce 'Water Sort > Projeyi Kur' çalıştır.", "Tamam");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorSceneManager.OpenScene(BootstrapScene);
            EditorApplication.EnterPlaymode();
        }

        #endregion

        #region TextMeshPro

        /// <summary>
        /// TMP text needs the Essential Resources (default font, settings).
        /// Imports them if missing; the user then runs the menu again.
        /// </summary>
        private static bool EnsureTmpEssentials()
        {
            if (Resources.Load<TMP_Settings>("TMP Settings") != null)
                return true;

            // Packages live in Library/PackageCache; ask the Package Manager where
            var tmpPackage = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/" + TmpPackageName);
            string essentials = tmpPackage != null ? Path.Combine(tmpPackage.resolvedPath, TmpEssentialsFile) : null;

            if (essentials == null || !File.Exists(essentials))
            {
                EditorUtility.DisplayDialog("Water Sort Kurulum",
                    "TextMeshPro paketi bulunamadı. Window > Package Manager'dan TextMeshPro'yu kur, sonra tekrar dene.",
                    "Tamam");
                return false;
            }

            AssetDatabase.ImportPackage(essentials, false);
            EditorUtility.DisplayDialog("Water Sort Kurulum",
                "TextMeshPro Essential Resources içe aktarıldı.\n\nİçe aktarma bitince 'Water Sort > Projeyi Kur' menüsünü bir kez daha çalıştır.",
                "Tamam");
            return false;
        }

        #endregion

        #region Sprites

        private static Sprite CreateTubeSprite()
        {
            const int w = TubeWidthPx, h = TubeHeightPx;
            const float border = 4f, radius = 22f;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float d = RoundedBottomDistance(x + 0.5f, y + 0.5f, w, h, radius); // < 0 inside
                    Color c = Color.clear;

                    if (d <= 0f)
                    {
                        bool isBorder = d > -border;
                        bool isRim = y >= h - 8;
                        bool isShine = x >= 12 && x <= 18 && y > 24 && y < h - 22;

                        if (isBorder || isRim)
                            c = new Color(0.84f, 0.96f, 1f, 0.85f);
                        else if (isShine)
                            c = new Color(1f, 1f, 1f, 0.28f);
                        else
                            c = new Color(0.84f, 0.96f, 1f, 0.08f);
                    }

                    pixels[y * w + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return SaveSprite(tex, TubeSpritePath, new Vector2(0.5f, 0f));
        }

        /// <summary>
        /// Signed distance-ish test for a rectangle whose bottom corners are rounded
        /// </summary>
        private static float RoundedBottomDistance(float x, float y, float w, float h, float r)
        {
            if (y >= r)
                return Mathf.Max(-x, x - w);

            float cx = Mathf.Clamp(x, r, w - r);
            float dx = x - cx, dy = y - r;
            return Mathf.Sqrt(dx * dx + dy * dy) - r;
        }

        private static Sprite CreateSegmentSprite()
        {
            var tex = new Texture2D(SegmentWidthPx, SegmentHeightPx, TextureFormat.RGBA32, false);
            var pixels = new Color[SegmentWidthPx * SegmentHeightPx];

            for (int y = 0; y < SegmentHeightPx; y++)
            {
                for (int x = 0; x < SegmentWidthPx; x++)
                {
                    // White so SpriteRenderer.color tints it; light band on top as liquid surface
                    float shade = y >= SegmentHeightPx - 4 ? 1f : 0.92f;
                    pixels[y * SegmentWidthPx + x] = new Color(shade, shade, shade, 1f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return SaveSprite(tex, SegmentSpritePath, new Vector2(0.5f, 0f));
        }

        private static Sprite SaveSprite(Texture2D tex, string path, Vector2 pivot)
        {
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        #endregion

        #region Prefabs

        private static GameObject CreateSegmentPrefab(Sprite sprite)
        {
            var go = new GameObject("LiquidSegment");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 1; // behind the glass

            return SavePrefab(go, SegmentPrefabPath);
        }

        private static GameObject CreateTubePrefab(Sprite sprite, GameObject segmentPrefab)
        {
            float width = TubeWidthPx / (float)PixelsPerUnit;
            float height = TubeHeightPx / (float)PixelsPerUnit;

            var go = new GameObject("Tube");

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 2; // glass over the liquid

            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(width + 0.3f, height + 0.3f); // a bit larger for easier taps
            collider.offset = new Vector2(0f, height / 2f);

            var container = new GameObject("LiquidContainer").transform;
            container.SetParent(go.transform, false);
            container.localPosition = new Vector3(0f, 0.06f, 0f);

            var tube = go.AddComponent<TubeController>();
            tube.LiquidContainer = container;
            tube.TubeRenderer = renderer;
            tube.LiquidSegmentPrefab = segmentPrefab;
            tube.SegmentHeight = SegmentHeightPx / (float)PixelsPerUnit;
            tube.SelectionScale = 1.1f;
            tube.PourDuration = 0.4f;
            tube.MaxCapacity = 4;

            return SavePrefab(go, TubePrefabPath);
        }

        private static Button CreateLevelButtonPrefab()
        {
            var go = new GameObject("LevelButton", typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(160, 160);

            var image = go.AddComponent<Image>();
            image.sprite = UiSprite();
            image.type = Image.Type.Sliced;
            image.color = Ink;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;

            var label = CreateText(go.transform, "Label", "1", 56, Text, TextAlignmentOptions.Center);
            Stretch(label.rectTransform);

            GameObject prefab = SavePrefab(go, LevelButtonPrefabPath);
            return prefab.GetComponent<Button>();
        }

        private static GameObject SavePrefab(GameObject go, string path)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        #endregion

        #region Scenes

        private static Scene NewScene()
        {
            return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        private static Camera CreateCamera(float orthographicSize)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 0f, -10f);

            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Ink;
            go.AddComponent<AudioListener>();
            return cam;
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private static void CreateBootstrapScene()
        {
            Scene scene = NewScene();
            CreateCamera(5f);

            // GameManager loads the save, initializes ads and opens the MainMenu scene
            new GameObject("GameManager").AddComponent<GameManager>();
            new GameObject("PerformanceManager").AddComponent<PerformanceManager>();
            new GameObject("DeviceCompatibilityManager").AddComponent<DeviceCompatibilityManager>();

            EditorSceneManager.SaveScene(scene, BootstrapScene);
        }

        private static void CreateMainMenuScene(Button levelButtonPrefab)
        {
            Scene scene = NewScene();
            CreateCamera(5f);
            CreateEventSystem();

            RectTransform root = CreateCanvas("MainMenuCanvas");
            var menu = root.GetComponentInParent<Canvas>().gameObject.AddComponent<MainMenuUI>();

            // Title
            var title = CreateText(root, "Title", "Water\n<color=#FFD166>Sort</color>", 170, Text, TextAlignmentOptions.Center);
            title.fontStyle = FontStyles.Bold;
            title.lineSpacing = -20f;
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -330f), new Vector2(900f, 420f));

            // Info pills
            menu.CurrentLevelText = CreateText(root, "CurrentLevelText", "Level 1", 60, Text, TextAlignmentOptions.Center);
            Place(menu.CurrentLevelText.rectTransform, new Vector2(0.5f, 1f), new Vector2(-170f, -640f), new Vector2(320f, 90f));
            menu.CoinText = CreateText(root, "CoinText", "0", 60, Accent, TextAlignmentOptions.Center);
            Place(menu.CoinText.rectTransform, new Vector2(0.5f, 1f), new Vector2(170f, -640f), new Vector2(320f, 90f));

            // Buttons
            var column = CreateColumn(root, "Buttons", 28f);
            Place(column, new Vector2(0.5f, 0.5f), new Vector2(0f, -260f), new Vector2(700f, 1000f));

            menu.PlayButton = CreateButton(column, "PlayButton", "Oyna", true, 150f);
            menu.ContinueButton = CreateButton(column, "ContinueButton", "Devam Et", false, 120f);
            menu.LevelSelectButton = CreateButton(column, "LevelSelectButton", "Seviye Seç", false, 120f);
            menu.DailyRewardButton = CreateButton(column, "DailyRewardButton", "Günlük Ödül", false, 120f);
            menu.SettingsButton = CreateButton(column, "SettingsButton", "Ayarlar", false, 120f);
            menu.NoAdsButton = CreateButton(column, "NoAdsButton", "Reklamları Kaldır", false, 120f);

            // Green dot on the daily reward button when it can be claimed
            var dot = new GameObject("AvailableIndicator", typeof(RectTransform)).AddComponent<Image>();
            dot.transform.SetParent(menu.DailyRewardButton.transform, false);
            dot.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            dot.color = Good;
            dot.raycastTarget = false;
            Place(dot.rectTransform, new Vector2(1f, 0.5f), new Vector2(-60f, 0f), new Vector2(36f, 36f));
            menu.DailyRewardAvailableIndicator = dot.gameObject;

            // Settings popup
            RectTransform settingsCard = CreatePopup(root, "SettingsPopup", out GameObject settingsPopup);
            CreateText(settingsCard, "Title", "Ayarlar", 80, Text, TextAlignmentOptions.Center).gameObject.AddComponent<LayoutElement>().preferredHeight = 110f;
            menu.SoundToggle = CreateToggle(settingsCard, "SoundToggle", "Ses Efektleri");
            menu.MusicToggle = CreateToggle(settingsCard, "MusicToggle", "Müzik");
            menu.HapticToggle = CreateToggle(settingsCard, "HapticToggle", "Titreşim");
            menu.SettingsCloseButton = CreateButton(settingsCard, "CloseButton", "Kapat", true, 120f);
            menu.SettingsPopup = settingsPopup;

            // Level select popup
            RectTransform levelCard = CreatePopup(root, "LevelSelectPopup", out GameObject levelPopup);
            CreateText(levelCard, "Title", "Seviye Seç", 80, Text, TextAlignmentOptions.Center).gameObject.AddComponent<LayoutElement>().preferredHeight = 110f;
            menu.LevelButtonContainer = CreateScrollGrid(levelCard, 900f);
            menu.LevelButtonPrefab = levelButtonPrefab;
            menu.LevelSelectCloseButton = CreateButton(levelCard, "CloseButton", "Kapat", true, 120f);
            menu.LevelSelectPopup = levelPopup;

            EditorSceneManager.SaveScene(scene, MainMenuScene);
        }

        private static void CreateGameScene(GameObject tubePrefab)
        {
            Scene scene = NewScene();
            CreateCamera(6f);
            CreateEventSystem();

            // Level
            var levelGo = new GameObject("LevelManager");
            var level = levelGo.AddComponent<LevelManager>();
            var tubes = new GameObject("Tubes").transform;
            tubes.SetParent(levelGo.transform, false);
            level.TubesContainer = tubes;
            level.TubePrefab = tubePrefab;
            level.TubeSpacing = 1.2f;
            level.RowSpacing = 2.9f;
            level.MaxTubesPerRow = 5;
            level.TubeVisualHeight = TubeHeightPx / (float)PixelsPerUnit;
            level.TubeVisualWidth = TubeWidthPx / (float)PixelsPerUnit;

            // Input
            var input = new GameObject("InputManager").AddComponent<InputManager>();
            input.TubeLayer = 1 << LayerMask.NameToLayer("Default");

            // UI
            RectTransform root = CreateCanvas("GameCanvas");
            var ui = root.GetComponentInParent<Canvas>().gameObject.AddComponent<UIManager>();

            // Top bar
            ui.MenuButton = CreateButton(root, "MenuButton", "Menü", false, 110f);
            Place((RectTransform)ui.MenuButton.transform, new Vector2(0f, 1f), new Vector2(170f, -90f), new Vector2(260f, 110f));

            ui.LevelText = CreateText(root, "LevelText", "Level 1", 96, Text, TextAlignmentOptions.Center);
            ui.LevelText.fontStyle = FontStyles.Bold;
            Place(ui.LevelText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(500f, 120f));

            ui.SettingsButton = CreateButton(root, "SettingsButton", "Ayarlar", false, 110f);
            Place((RectTransform)ui.SettingsButton.transform, new Vector2(1f, 1f), new Vector2(-170f, -90f), new Vector2(260f, 110f));

            // Counters
            ui.MoveCountText = CreateText(root, "MoveCountText", "Hamle: 0", 52, Muted, TextAlignmentOptions.Center);
            Place(ui.MoveCountText.rectTransform, new Vector2(0.5f, 1f), new Vector2(-300f, -215f), new Vector2(340f, 70f));
            ui.UndoCountText = CreateText(root, "UndoCountText", "Geri Al: 5", 52, Muted, TextAlignmentOptions.Center);
            Place(ui.UndoCountText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -215f), new Vector2(340f, 70f));
            ui.CoinText = CreateText(root, "CoinText", "0", 52, Accent, TextAlignmentOptions.Center);
            Place(ui.CoinText.rectTransform, new Vector2(0.5f, 1f), new Vector2(300f, -215f), new Vector2(340f, 70f));

            // Bottom tools
            var tools = new GameObject("Tools", typeof(RectTransform)).GetComponent<RectTransform>();
            tools.SetParent(root, false);
            Place(tools, new Vector2(0.5f, 0f), new Vector2(0f, 120f), new Vector2(1000f, 170f));
            var row = tools.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 24f;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = true;
            row.childForceExpandHeight = true;

            ui.UndoButton = CreateButton(tools, "UndoButton", "Geri Al", false, 170f);
            ui.RestartButton = CreateButton(tools, "RestartButton", "Yeniden", false, 170f);
            ui.HintButton = CreateButton(tools, "HintButton", "İpucu", false, 170f);
            ui.ExtraTubeButton = CreateButton(tools, "ExtraTubeButton", "+ Tüp", false, 170f);

            // Level complete popup
            RectTransform completeCard = CreatePopup(root, "LevelCompletePopup", out GameObject completePopup);
            ui.LevelCompleteTitle = CreateText(completeCard, "Title", "Level 1 Tamamlandı!", 76, Text, TextAlignmentOptions.Center);
            ui.LevelCompleteTitle.fontStyle = FontStyles.Bold;
            ui.LevelCompleteTitle.gameObject.AddComponent<LayoutElement>().preferredHeight = 200f;
            ui.LevelCompleteMoves = CreateText(completeCard, "Moves", "0 hamle ile tamamladınız!", 50, Muted, TextAlignmentOptions.Center);
            ui.LevelCompleteMoves.gameObject.AddComponent<LayoutElement>().preferredHeight = 90f;
            ui.LevelCompleteNextButton = CreateButton(completeCard, "NextButton", "Sonraki Level", true, 150f);
            ui.LevelCompletePopup = completePopup;

            // Settings popup
            RectTransform settingsCard = CreatePopup(root, "SettingsPopup", out GameObject settingsPopup);
            CreateText(settingsCard, "Title", "Ayarlar", 80, Text, TextAlignmentOptions.Center).gameObject.AddComponent<LayoutElement>().preferredHeight = 110f;
            ui.SoundToggle = CreateToggle(settingsCard, "SoundToggle", "Ses Efektleri");
            ui.MusicToggle = CreateToggle(settingsCard, "MusicToggle", "Müzik");
            ui.HapticToggle = CreateToggle(settingsCard, "HapticToggle", "Titreşim");
            ui.SettingsCloseButton = CreateButton(settingsCard, "CloseButton", "Kapat", true, 120f);
            ui.SettingsPopup = settingsPopup;

            // Rewarded ad prompt
            RectTransform adCard = CreatePopup(root, "RewardedAdPromptPopup", out GameObject adPopup);
            ui.RewardedAdPromptTitle = CreateText(adCard, "Title", "Reklam", 76, Text, TextAlignmentOptions.Center);
            ui.RewardedAdPromptTitle.fontStyle = FontStyles.Bold;
            ui.RewardedAdPromptTitle.gameObject.AddComponent<LayoutElement>().preferredHeight = 110f;
            ui.RewardedAdPromptMessage = CreateText(adCard, "Message", "", 48, Muted, TextAlignmentOptions.Center);
            ui.RewardedAdPromptMessage.gameObject.AddComponent<LayoutElement>().preferredHeight = 160f;
            var adButtons = new GameObject("Buttons", typeof(RectTransform)).GetComponent<RectTransform>();
            adButtons.SetParent(adCard, false);
            adButtons.gameObject.AddComponent<LayoutElement>().preferredHeight = 130f;
            var adRow = adButtons.gameObject.AddComponent<HorizontalLayoutGroup>();
            adRow.spacing = 24f;
            adRow.childControlWidth = true;
            adRow.childControlHeight = true;
            adRow.childForceExpandWidth = true;
            adRow.childForceExpandHeight = true;
            ui.RewardedAdPromptCancelButton = CreateButton(adButtons, "CancelButton", "Vazgeç", false, 130f);
            ui.RewardedAdPromptWatchButton = CreateButton(adButtons, "WatchButton", "İzle", true, 130f);
            ui.RewardedAdPromptPopup = adPopup;

            // Toast message (does not block taps)
            var toast = new GameObject("MessagePopup", typeof(RectTransform)).AddComponent<Image>();
            toast.transform.SetParent(root, false);
            toast.sprite = UiSprite();
            toast.type = Image.Type.Sliced;
            toast.color = Text;
            toast.raycastTarget = false;
            Place(toast.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 360f), new Vector2(860f, 140f));
            ui.MessageText = CreateText(toast.transform, "MessageText", "", 46, Ink, TextAlignmentOptions.Center);
            ui.MessageText.raycastTarget = false;
            Stretch(ui.MessageText.rectTransform, 24f);
            ui.MessagePopup = toast.gameObject;

            EditorSceneManager.SaveScene(scene, GameScene);
        }

        #endregion

        #region UI Builders

        /// <summary>
        /// Screen-space canvas scaled for a 1080x1920 portrait reference, with a safe-area child.
        /// Returns the safe-area RectTransform that content goes under.
        /// </summary>
        private static RectTransform CreateCanvas(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();

            var safe = new GameObject("SafeArea", typeof(RectTransform)).GetComponent<RectTransform>();
            safe.SetParent(go.transform, false);
            Stretch(safe);
            safe.gameObject.AddComponent<SafeAreaPanel>();
            return safe;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size, Color color, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string label, bool primary, float height)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(600f, height);

            var image = go.AddComponent<Image>();
            image.sprite = UiSprite();
            image.type = Image.Type.Sliced;
            image.color = primary ? Accent : Panel;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            button.colors = colors;

            var layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = height;
            layout.minHeight = height;

            var text = CreateText(go.transform, "Label", label, primary ? 60 : 50, primary ? AccentInk : Text, TextAlignmentOptions.Center);
            text.fontStyle = FontStyles.Bold;
            Stretch(text.rectTransform, 12f);

            return button;
        }

        private static Toggle CreateToggle(Transform parent, string name, string label)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.AddComponent<LayoutElement>().preferredHeight = 110f;

            var background = new GameObject("Background", typeof(RectTransform)).AddComponent<Image>();
            background.transform.SetParent(go.transform, false);
            background.sprite = UiSprite();
            background.type = Image.Type.Sliced;
            background.color = Ink;
            Place(background.rectTransform, new Vector2(0f, 0.5f), new Vector2(50f, 0f), new Vector2(90f, 90f));

            var check = new GameObject("Checkmark", typeof(RectTransform)).AddComponent<Image>();
            check.transform.SetParent(background.transform, false);
            check.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
            check.color = Accent;
            Stretch(check.rectTransform, 10f);

            var text = CreateText(go.transform, "Label", label, 54, Text, TextAlignmentOptions.MidlineLeft);
            Place(text.rectTransform, new Vector2(0f, 0.5f), new Vector2(380f, 0f), new Vector2(560f, 100f));

            var toggle = go.AddComponent<Toggle>();
            toggle.targetGraphic = background;
            toggle.graphic = check;
            toggle.isOn = true;
            return toggle;
        }

        private static RectTransform CreateColumn(Transform parent, string name, float spacing)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return rect;
        }

        /// <summary>
        /// Full-screen dim layer with a centered card that sizes to its content.
        /// Returns the card; popupRoot is the object to show/hide.
        /// </summary>
        private static RectTransform CreatePopup(Transform parent, string name, out GameObject popupRoot)
        {
            var scrim = new GameObject(name, typeof(RectTransform)).AddComponent<Image>();
            scrim.transform.SetParent(parent, false);
            scrim.color = Scrim;
            scrim.raycastTarget = true; // blocks taps on the tubes behind
            Stretch(scrim.rectTransform);
            popupRoot = scrim.gameObject;

            var card = new GameObject("Card", typeof(RectTransform)).AddComponent<Image>();
            card.transform.SetParent(scrim.transform, false);
            card.sprite = UiSprite();
            card.type = Image.Type.Sliced;
            card.color = Panel;
            Place(card.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 600f));

            var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(60, 60, 60, 60);
            layout.spacing = 30f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = card.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return card.rectTransform;
        }

        /// <summary>
        /// Vertical ScrollRect with a 5-column grid. Returns the grid content transform.
        /// </summary>
        private static Transform CreateScrollGrid(Transform parent, float height)
        {
            var scroll = new GameObject("LevelScroll", typeof(RectTransform)).AddComponent<ScrollRect>();
            scroll.transform.SetParent(parent, false);
            scroll.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            scroll.horizontal = false;

            var viewport = new GameObject("Viewport", typeof(RectTransform)).AddComponent<RectMask2D>().GetComponent<RectTransform>();
            viewport.SetParent(scroll.transform, false);
            Stretch(viewport);

            var content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;

            var grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(140f, 140f);
            grid.spacing = new Vector2(18f, 18f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.childAlignment = TextAnchor.UpperCenter;

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = content;
            return content;
        }

        private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect, float padding = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }

        private static Sprite UiSprite()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        #endregion

        #region Helpers

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            return color;
        }

        #endregion
    }
}
