using UnityEngine;
using UnityEngine.SceneManagement;

namespace WaterSort.Core
{
    /// <summary>
    /// Global game state manager - Singleton pattern
    /// Manages overall game flow, scene transitions, and core systems
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        _instance = go.AddComponent<GameManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Game State")]
        public GameState CurrentState { get; private set; }

        [Header("Settings")]
        public bool IsSoundEnabled = true;
        public bool IsMusicEnabled = true;
        public bool IsHapticEnabled = true;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            // Initialize core systems
            Application.targetFrameRate = 60;

            // Load player settings
            LoadSettings();

            // Initialize subsystems
            AdsManager.Instance.Initialize();
            SaveManager.Instance.LoadGame();

            ChangeState(GameState.MainMenu);
        }

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;

            switch (newState)
            {
                case GameState.MainMenu:
                    LoadScene("MainMenu");
                    break;
                case GameState.Playing:
                    LoadScene("Game");
                    break;
                case GameState.Paused:
                    // Handle pause
                    break;
                case GameState.LevelComplete:
                    // Handle level complete
                    break;
            }
        }

        private void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void LoadSettings()
        {
            IsSoundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
            IsMusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
            IsHapticEnabled = PlayerPrefs.GetInt("HapticEnabled", 1) == 1;
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetInt("SoundEnabled", IsSoundEnabled ? 1 : 0);
            PlayerPrefs.SetInt("MusicEnabled", IsMusicEnabled ? 1 : 0);
            PlayerPrefs.SetInt("HapticEnabled", IsHapticEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void PlaySound(AudioClip clip)
        {
            if (!IsSoundEnabled || clip == null) return;
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 0.5f);
        }

        public void TriggerHaptic()
        {
            if (!IsHapticEnabled) return;
            #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
            #endif
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveManager.Instance.SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            SaveManager.Instance.SaveGame();
        }
    }

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        LevelComplete
    }
}
