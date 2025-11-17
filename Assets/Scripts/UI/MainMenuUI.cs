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

        private void Start()
        {
            SetupButtons();
            UpdateUI();
            CheckDailyReward();
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
                CurrentLevelText.text = $"Level {currentLevel + 1}";
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
            // Start from level 0
            GameManager.Instance.ChangeState(GameState.Playing);
        }

        private void OnContinueClicked()
        {
            // Continue from saved level
            GameManager.Instance.ChangeState(GameState.Playing);
        }

        private void OnLevelSelectClicked()
        {
            // TODO: Show level select screen
            Debug.Log("Level Select - TODO");
        }

        private void OnSettingsClicked()
        {
            // TODO: Show settings popup
            Debug.Log("Settings - TODO");
        }

        private void OnNoAdsClicked()
        {
            // TODO: Show IAP purchase screen
            Debug.Log("No Ads Purchase - TODO: Implement IAP");
        }

        private void OnDailyRewardClicked()
        {
            if (RewardManager.Instance.CanClaimDailyReward())
            {
                RewardManager.Instance.ShowDailyRewardAd();
                CheckDailyReward(); // Update indicator
            }
            else
            {
                Debug.Log("Daily reward already claimed");
            }
        }
    }
}
