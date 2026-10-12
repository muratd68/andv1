using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WaterSort.Core
{
    /// <summary>
    /// Main menu UI controller
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        public Button PlayButton;
        public Button ContinueButton;
        public Button LevelSelectButton;
        public Button SettingsButton;
        public Button NoAdsButton;
        public Button DailyRewardButton;

        [Header("Info")]
        public TextMeshProUGUI CurrentLevelText;
        public TextMeshProUGUI CoinText;

        [Header("Daily Reward Indicator")]
        public GameObject DailyRewardAvailableIndicator;

        [Header("Settings Popup")]
        public GameObject SettingsPopup;
        public Toggle SoundToggle;
        public Toggle MusicToggle;
        public Toggle HapticToggle;
        public Button SettingsCloseButton;
        public Button LanguageButton;

        [Header("Level Select Popup")]
        public GameObject LevelSelectPopup;
        [Tooltip("Parent for level buttons, e.g. the Content of a ScrollRect with a GridLayoutGroup")]
        public Transform LevelButtonContainer;
        [Tooltip("Button prefab with a TextMeshProUGUI child for the level number")]
        public Button LevelButtonPrefab;
        public Button LevelSelectCloseButton;

        private void OnEnable()
        {
            Loc.LanguageChanged += UpdateUI;
        }

        private void OnDisable()
        {
            Loc.LanguageChanged -= UpdateUI;
        }

        private void Start()
        {
            SetupButtons();
            SetupSettings();
            UpdateUI();
            CheckDailyReward();

            if (SettingsPopup != null) SettingsPopup.SetActive(false);
            if (LevelSelectPopup != null) LevelSelectPopup.SetActive(false);
        }

        private void SetupButtons()
        {
            if (PlayButton != null)
                PlayButton.onClick.AddListener(OnPlayClicked);

            if (ContinueButton != null)
                ContinueButton.onClick.AddListener(OnContinueClicked);

            if (LevelSelectButton != null)
                LevelSelectButton.onClick.AddListener(OnLevelSelectClicked);

            if (SettingsButton != null)
                SettingsButton.onClick.AddListener(OnSettingsClicked);

            if (NoAdsButton != null)
                NoAdsButton.onClick.AddListener(OnNoAdsClicked);

            if (DailyRewardButton != null)
                DailyRewardButton.onClick.AddListener(OnDailyRewardClicked);
        }

        private void UpdateUI()
        {
            int currentLevel = SaveManager.Instance.GetCurrentLevel();

            if (CurrentLevelText != null)
            {
                CurrentLevelText.text = Loc.Get("hud.level", currentLevel + 1);
            }

            if (CoinText != null)
            {
                CoinText.text = $"{SaveManager.Instance.GetCoins()}";
            }

            // Show/hide continue button based on progress
            if (ContinueButton != null)
            {
                ContinueButton.gameObject.SetActive(currentLevel > 0);
            }
        }

        private void CheckDailyReward()
        {
            bool available = RewardManager.Instance.CanClaimDailyReward();

            if (DailyRewardAvailableIndicator != null)
            {
                DailyRewardAvailableIndicator.SetActive(available);
            }
        }

        private void OnPlayClicked()
        {
            // New game: start from level 0 (highest level reached is kept for level select)
            SaveManager.Instance.SetCurrentLevel(0);
            GameManager.Instance.ChangeState(GameState.Playing);
        }

        private void OnContinueClicked()
        {
            // Continue from saved level
            GameManager.Instance.ChangeState(GameState.Playing);
        }

        #region Settings

        private void SetupSettings()
        {
            if (SoundToggle != null)
            {
                SoundToggle.isOn = GameManager.Instance.IsSoundEnabled;
                SoundToggle.onValueChanged.AddListener(value =>
                {
                    GameManager.Instance.IsSoundEnabled = value;
                    GameManager.Instance.SaveSettings();
                });
            }

            if (MusicToggle != null)
            {
                MusicToggle.isOn = GameManager.Instance.IsMusicEnabled;
                MusicToggle.onValueChanged.AddListener(value => GameManager.Instance.SetMusicEnabled(value));
            }

            if (HapticToggle != null)
            {
                HapticToggle.isOn = GameManager.Instance.IsHapticEnabled;
                HapticToggle.onValueChanged.AddListener(value =>
                {
                    GameManager.Instance.IsHapticEnabled = value;
                    GameManager.Instance.SaveSettings();
                });
            }

            if (SettingsCloseButton != null)
                SettingsCloseButton.onClick.AddListener(() => SettingsPopup.SetActive(false));

            if (LanguageButton != null)
                LanguageButton.onClick.AddListener(Loc.ToggleLanguage);
        }

        private void OnSettingsClicked()
        {
            if (SettingsPopup != null)
                SettingsPopup.SetActive(true);
        }

        #endregion

        #region Level Select

        private void OnLevelSelectClicked()
        {
            if (LevelSelectPopup == null || LevelButtonContainer == null || LevelButtonPrefab == null)
            {
                Debug.LogWarning("Level select popup is not set up in the inspector");
                return;
            }

            BuildLevelButtons();
            LevelSelectPopup.SetActive(true);

            if (LevelSelectCloseButton != null)
            {
                LevelSelectCloseButton.onClick.RemoveAllListeners();
                LevelSelectCloseButton.onClick.AddListener(() => LevelSelectPopup.SetActive(false));
            }
        }

        /// <summary>
        /// One button per unlocked level (every level up to the highest reached)
        /// </summary>
        private void BuildLevelButtons()
        {
            for (int i = LevelButtonContainer.childCount - 1; i >= 0; i--)
                Destroy(LevelButtonContainer.GetChild(i).gameObject);

            int highest = SaveManager.Instance.GetHighestLevel();
            for (int level = 0; level <= highest; level++)
            {
                int levelIndex = level; // captured by the click handler
                Button button = Instantiate(LevelButtonPrefab, LevelButtonContainer);

                var label = button.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = (levelIndex + 1).ToString();

                button.onClick.AddListener(() =>
                {
                    SaveManager.Instance.SetCurrentLevel(levelIndex);
                    GameManager.Instance.ChangeState(GameState.Playing);
                });
            }
        }

        #endregion

        private void OnNoAdsClicked()
        {
            // TODO: Show IAP purchase screen
            Debug.Log("No Ads Purchase - TODO: Implement IAP");
        }

        private void OnDailyRewardClicked()
        {
            if (RewardManager.Instance.CanClaimDailyReward())
            {
                RewardManager.Instance.ShowDailyRewardAd(() =>
                {
                    // Update indicator and coins once the reward is granted
                    CheckDailyReward();
                    UpdateUI();
                });
            }
            else
            {
                Debug.Log("Daily reward already claimed");
            }
        }
    }
}
