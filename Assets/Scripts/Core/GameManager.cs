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

        // Game State
        public GameState CurrentState { get; private set; }

        [Header("Settings")]
        public bool IsSoundEnabled = true;
        public bool IsMusicEnabled = true;
        public bool IsHapticEnabled = true;

        [Header("Scenes")]
        public string MainMenuSceneName = "MainMenu";
        public string GameSceneName = "Game";

        [Header("Audio")]
        public AudioSource MusicSource;
        public AudioClip BackgroundMusic;
        [Range(0f, 1f)] public float SfxVolume = 0.5f;
        private AudioSource _sfxSource;

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
            SetupAudio();

            // Initialize subsystems - the save must be loaded first so AdsManager
            // can see whether "No Ads" was purchased
            SaveManager.Instance.LoadGame();
            AdsManager.Instance.Initialize();

            // When a scene is played directly in the editor, stay in it instead of jumping to the menu
            string activeScene = SceneManager.GetActiveScene().name;
            if (activeScene == GameSceneName)
                CurrentState = GameState.Playing;
            else if (activeScene == MainMenuSceneName)
                CurrentState = GameState.MainMenu;
            else
                ChangeState(GameState.MainMenu);
        }

        /// <summary>
        /// Sets the state without loading a scene (e.g. back to Playing when the next level starts)
        /// </summary>
        public void EnsureState(GameState state)
        {
            CurrentState = state;
        }

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;

            switch (newState)
            {
                case GameState.MainMenu:
                    LoadScene(MainMenuSceneName);
                    break;
                case GameState.Playing:
                    LoadScene(GameSceneName);
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
            // Avoid reloading the scene we are already in (and resetting it)
            if (SceneManager.GetActiveScene().name == sceneName)
                return;

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
            if (!IsSoundEnabled || clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip, SfxVolume);
        }

        #region Music

        private void SetupAudio()
        {
            // 2D sources on this persistent object - survive scene loads and need no camera
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.spatialBlend = 0f;

            if (MusicSource == null)
            {
                MusicSource = gameObject.AddComponent<AudioSource>();
                MusicSource.playOnAwake = false;
                MusicSource.spatialBlend = 0f;
                MusicSource.volume = 0.4f;
            }
            MusicSource.loop = true;

            ApplyMusicSetting();
        }

        public void SetMusicEnabled(bool enabled)
        {
            IsMusicEnabled = enabled;
            SaveSettings();
            ApplyMusicSetting();
        }

        public void ApplyMusicSetting()
        {
            if (MusicSource == null) return;

            if (MusicSource.clip == null)
                MusicSource.clip = BackgroundMusic;

            if (IsMusicEnabled && MusicSource.clip != null)
            {
                if (!MusicSource.isPlaying)
                    MusicSource.Play();
            }
            else
            {
                MusicSource.Stop();
            }
        }

        #endregion

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
