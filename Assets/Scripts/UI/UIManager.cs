using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace WaterSort.Core
{
    /// <summary>
    /// Central UI management - handles HUD, popups, messages
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<UIManager>();
                }
                return _instance;
            }
        }

        [Header("HUD Elements")]
        public TextMeshProUGUI LevelText;
        public TextMeshProUGUI MoveCountText;
        public TextMeshProUGUI UndoCountText;
        public TextMeshProUGUI CoinText;

        [Header("Buttons")]
        public Button UndoButton;
        public Button RestartButton;
        public Button SettingsButton;
        public Button ExtraTubeButton;
        public Button HintButton;
        public Button MenuButton;
        public Button SettingsCloseButton;

        [Header("Popups")]
        public GameObject LevelCompletePopup;
        public GameObject SettingsPopup;
        public GameObject RewardedAdPromptPopup;
        public GameObject MessagePopup;

        [Header("Popup References")]
        public TextMeshProUGUI LevelCompleteTitle;
        public TextMeshProUGUI LevelCompleteMoves;
        public Button LevelCompleteNextButton;

        public TextMeshProUGUI RewardedAdPromptTitle;
        public TextMeshProUGUI RewardedAdPromptMessage;
        public Button RewardedAdPromptWatchButton;
        public Button RewardedAdPromptCancelButton;

        public TextMeshProUGUI MessageText;

        [Header("Settings Elements")]
        public Toggle SoundToggle;
        public Toggle MusicToggle;
        public Toggle HapticToggle;

        private Action _currentRewardedAdCallback;

        private void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            SetupButtons();
            UpdateHUD();
            HideAllPopups();
        }

        private void SetupButtons()
        {
            if (UndoButton != null)
                UndoButton.onClick.AddListener(OnUndoClicked);

            if (RestartButton != null)
                RestartButton.onClick.AddListener(OnRestartClicked);

            if (SettingsButton != null)
                SettingsButton.onClick.AddListener(OnSettingsClicked);

            if (ExtraTubeButton != null)
                ExtraTubeButton.onClick.AddListener(OnExtraTubeClicked);

            if (HintButton != null)
                HintButton.onClick.AddListener(OnHintClicked);

            if (MenuButton != null)
                MenuButton.onClick.AddListener(() => GameManager.Instance.ChangeState(GameState.MainMenu));

            if (SettingsCloseButton != null)
                SettingsCloseButton.onClick.AddListener(HideSettingsPopup);

            // Settings toggles
            if (SoundToggle != null)
            {
                SoundToggle.isOn = GameManager.Instance.IsSoundEnabled;
                SoundToggle.onValueChanged.AddListener(OnSoundToggled);
            }

            if (MusicToggle != null)
            {
                MusicToggle.isOn = GameManager.Instance.IsMusicEnabled;
                MusicToggle.onValueChanged.AddListener(OnMusicToggled);
            }

            if (HapticToggle != null)
            {
                HapticToggle.isOn = GameManager.Instance.IsHapticEnabled;
                HapticToggle.onValueChanged.AddListener(OnHapticToggled);
            }
        }

        #region HUD Updates

        public void UpdateHUD()
        {
            UpdateLevelNumber();
            UpdateCoinCount();
        }

        public void UpdateLevelNumber()
        {
            if (LevelText != null && LevelManager.Instance != null)
            {
                LevelText.text = $"Level {LevelManager.Instance.CurrentLevelIndex + 1}";
            }
        }

        public void UpdateMoveCount(int moves)
        {
            if (MoveCountText != null)
            {
                MoveCountText.text = $"Hamle: {moves}";
            }
        }

        public void UpdateUndoCount(int remaining)
        {
            if (UndoCountText != null)
            {
                UndoCountText.text = $"Geri Al: {remaining}";
            }

            // Show that undo requires an ad once free undos are used up (and switch back after a refill)
            if (UndoButton != null)
            {
                var buttonText = UndoButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = remaining <= 0 ? "Geri Al (Reklam)" : "Geri Al";
            }
        }

        public void UpdateCoinCount()
        {
            if (CoinText != null && SaveManager.Instance != null)
            {
                CoinText.text = $"{SaveManager.Instance.GetCoins()}";
            }
        }

        #endregion

        #region Button Handlers

        private void OnUndoClicked()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.UndoLastMove();
            }
        }

        private void OnRestartClicked()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RestartLevel();
            }
        }

        private void OnSettingsClicked()
        {
            ShowSettingsPopup();
        }

        private void OnExtraTubeClicked()
        {
            RewardManager.Instance.ShowRewardedAdForExtraTube();
        }

        private void OnHintClicked()
        {
            RewardManager.Instance.ShowRewardedAdForHint();
        }

        #endregion

        #region Settings

        private void OnSoundToggled(bool value)
        {
            GameManager.Instance.IsSoundEnabled = value;
            GameManager.Instance.SaveSettings();
        }

        private void OnMusicToggled(bool value)
        {
            GameManager.Instance.SetMusicEnabled(value);
        }

        private void OnHapticToggled(bool value)
        {
            GameManager.Instance.IsHapticEnabled = value;
            GameManager.Instance.SaveSettings();
        }

        public void ShowSettingsPopup()
        {
            if (SettingsPopup != null)
            {
                SettingsPopup.SetActive(true);
            }
        }

        public void HideSettingsPopup()
        {
            if (SettingsPopup != null)
            {
                SettingsPopup.SetActive(false);
            }
        }

        #endregion

        #region Level Complete Popup

        public void ShowLevelCompletePopup(int levelIndex, int moveCount)
        {
            if (LevelCompletePopup == null) return;

            LevelCompletePopup.SetActive(true);

            if (LevelCompleteTitle != null)
            {
                LevelCompleteTitle.text = $"Level {levelIndex + 1} Tamamlandı!";
            }

            if (LevelCompleteMoves != null)
            {
                LevelCompleteMoves.text = $"{moveCount} hamle ile tamamladınız!";
            }

            if (LevelCompleteNextButton != null)
            {
                LevelCompleteNextButton.onClick.RemoveAllListeners();
                LevelCompleteNextButton.onClick.AddListener(() =>
                {
                    HideLevelCompletePopup();
                    LevelManager.Instance.LoadNextLevel();
                });
            }
        }

        public void HideLevelCompletePopup()
        {
            if (LevelCompletePopup != null)
            {
                LevelCompletePopup.SetActive(false);
            }
        }

        #endregion

        #region Rewarded Ad Prompt

        public void ShowRewardedAdPrompt(string title, string message, Action onWatchAd)
        {
            if (RewardedAdPromptPopup == null) return;

            _currentRewardedAdCallback = onWatchAd;

            RewardedAdPromptPopup.SetActive(true);

            if (RewardedAdPromptTitle != null)
            {
                RewardedAdPromptTitle.text = title;
            }

            if (RewardedAdPromptMessage != null)
            {
                RewardedAdPromptMessage.text = message;
            }

            if (RewardedAdPromptWatchButton != null)
            {
                RewardedAdPromptWatchButton.onClick.RemoveAllListeners();
                RewardedAdPromptWatchButton.onClick.AddListener(() =>
                {
                    HideRewardedAdPrompt();
                    _currentRewardedAdCallback?.Invoke();
                    _currentRewardedAdCallback = null;
                });
            }

            if (RewardedAdPromptCancelButton != null)
            {
                RewardedAdPromptCancelButton.onClick.RemoveAllListeners();
                RewardedAdPromptCancelButton.onClick.AddListener(() =>
                {
                    HideRewardedAdPrompt();
                    _currentRewardedAdCallback = null;
                });
            }
        }

        public void HideRewardedAdPrompt()
        {
            if (RewardedAdPromptPopup != null)
            {
                RewardedAdPromptPopup.SetActive(false);
            }
        }

        #endregion

        #region Message Toast

        public void ShowMessage(string message, float duration = 2f)
        {
            if (MessagePopup == null || MessageText == null) return;

            MessageText.text = message;
            MessagePopup.SetActive(true);

            // Auto-hide after duration
            CancelInvoke(nameof(HideMessage));
            Invoke(nameof(HideMessage), duration);
        }

        public void HideMessage()
        {
            if (MessagePopup != null)
            {
                MessagePopup.SetActive(false);
            }
        }

        #endregion

        private void HideAllPopups()
        {
            HideLevelCompletePopup();
            HideSettingsPopup();
            HideRewardedAdPrompt();
            HideMessage();
        }
    }
}
