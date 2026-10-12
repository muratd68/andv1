using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace WaterSort.Core
{
    /// <summary>
    /// Manages level loading, progression, and win/lose conditions
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        private static LevelManager _instance;
        public static LevelManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<LevelManager>();
                }
                return _instance;
            }
        }

        // Level Setup
        public int CurrentLevelIndex { get; private set; }
        public LevelData CurrentLevel { get; private set; }

        // Game State
        public int MoveCount { get; private set; }
        public int UndoMovesRemaining { get; private set; }

        [Header("Settings")]
        public int UndoMovesPerLevel = 5;
        public int MaxTubesPerRow = 5;
        public float TubeSpacing = 1.5f;
        public float RowSpacing = 3f;

        [Header("References")]
        public Transform TubesContainer;
        public GameObject TubePrefab;

        private List<TubeController> _tubes = new List<TubeController>();
        private Stack<GameMove> _moveHistory = new Stack<GameMove>();

        // Input is ignored until pour/undo animations have finished
        private float _busyUntil;
        public bool IsBusy => Time.time < _busyUntil;

        private void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            // Make sure GameManager exists so the save is loaded (e.g. when the Game scene is played directly)
            GameManager.Instance.EnsureState(GameState.Playing);

            // OPTIMIZATION: Initialize object pools
            InitializeObjectPools();

            // Load current level from save
            CurrentLevelIndex = SaveManager.Instance.GetCurrentLevel();
            LoadLevel(CurrentLevelIndex);
        }

        private void InitializeObjectPools()
        {
            // Create liquid segment pool with pre-warming
            GameObject liquidPrefab = Resources.Load<GameObject>("Prefabs/LiquidSegment");

            if (liquidPrefab == null)
            {
                // Try to get from TubeController
                TubeController tubeSample = TubePrefab?.GetComponent<TubeController>();
                if (tubeSample != null)
                {
                    liquidPrefab = tubeSample.LiquidSegmentPrefab;
                }
            }

            if (liquidPrefab != null && ObjectPoolManager.Instance != null)
            {
                // Pre-allocate based on device tier
                int poolSize = PerformanceManager.Instance.IsLowEndDevice() ? 50 : 100;

                ObjectPoolManager.Instance.CreatePool("LiquidSegment", liquidPrefab, poolSize);
                Debug.Log($"Liquid segment pool created with {poolSize} objects");
            }
            else
            {
                Debug.LogWarning("Could not initialize liquid segment pool - prefab not found");
            }
        }

        public void LoadLevel(int levelIndex)
        {
            CurrentLevelIndex = levelIndex;
            CurrentLevel = LevelDataGenerator.GenerateLevel(levelIndex);

            if (CurrentLevel == null)
            {
                Debug.LogError($"Failed to load level {levelIndex}");
                return;
            }

            SetupLevel();

            // Analytics
            AnalyticsManager.Instance.LogEvent("level_start", new Dictionary<string, object>
            {
                { "level_id", levelIndex }
            });
        }

        private void SetupLevel()
        {
            // Clear existing tubes
            ClearTubes();

            // Reset counters
            MoveCount = 0;
            UndoMovesRemaining = UndoMovesPerLevel;
            _moveHistory.Clear();
            _busyUntil = 0f;

            // Create tubes from level data
            for (int i = 0; i < CurrentLevel.TubeConfigurations.Count; i++)
            {
                CreateTube(i, CurrentLevel.GetTubeColors(i));
            }

            LayoutTubes();

            // Back to playing (input is disabled while the level complete popup is shown)
            GameManager.Instance.EnsureState(GameState.Playing);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateLevelNumber();
                UIManager.Instance.UpdateMoveCount(MoveCount);
                UIManager.Instance.UpdateUndoCount(UndoMovesRemaining);
            }
        }

        private TubeController CreateTube(int index, List<LiquidColor> colors)
        {
            GameObject tubeObj = Instantiate(TubePrefab, TubesContainer);
            TubeController tube = tubeObj.GetComponent<TubeController>();
            tube.Initialize(index, colors, CurrentLevel.TubeCapacity);
            _tubes.Add(tube);
            return tube;
        }

        /// <summary>
        /// Arranges tubes in centered rows of at most MaxTubesPerRow
        /// </summary>
        private void LayoutTubes()
        {
            int total = _tubes.Count;
            if (total == 0) return;

            int rows = Mathf.CeilToInt(total / (float)MaxTubesPerRow);
            int perRow = Mathf.CeilToInt(total / (float)rows); // balance rows, e.g. 7 tubes -> 4 + 3

            for (int i = 0; i < total; i++)
            {
                int row = i / perRow;
                int col = i % perRow;
                int tubesInRow = Mathf.Min(perRow, total - row * perRow);

                float xOffset = (col - (tubesInRow - 1) / 2f) * TubeSpacing;
                float yOffset = ((rows - 1) / 2f - row) * RowSpacing;

                _tubes[i].transform.localPosition = new Vector3(xOffset, yOffset, 0);
            }
        }

        private void ClearTubes()
        {
            InputManager.Instance?.ResetSelection();

            foreach (var tube in _tubes)
            {
                if (tube != null)
                {
                    // Return liquid segments to the pool before the tube is destroyed
                    tube.ClearLiquid();
                    Destroy(tube.gameObject);
                }
            }
            _tubes.Clear();
        }

        private void MarkBusy()
        {
            float duration = _tubes.Count > 0 ? _tubes[0].PourDuration : 0.4f;
            _busyUntil = Time.time + duration;
        }

        public bool TryMove(int fromTubeIndex, int toTubeIndex)
        {
            if (IsBusy || GameManager.Instance.CurrentState != GameState.Playing)
                return false;

            if (fromTubeIndex < 0 || fromTubeIndex >= _tubes.Count ||
                toTubeIndex < 0 || toTubeIndex >= _tubes.Count)
                return false;

            TubeController fromTube = _tubes[fromTubeIndex];
            TubeController toTube = _tubes[toTubeIndex];

            if (!MoveValidator.IsValidMove(fromTube, toTube))
                return false;

            // Execute move
            LiquidColor movedColor = fromTube.GetTopColor();
            int amountMoved = fromTube.GetTopColorCount();

            fromTube.RemoveTop(amountMoved);
            toTube.AddLiquid(movedColor, amountMoved);

            // Record move for undo
            _moveHistory.Push(new GameMove
            {
                FromTubeIndex = fromTubeIndex,
                ToTubeIndex = toTubeIndex,
                Color = movedColor,
                Amount = amountMoved
            });

            MoveCount++;
            UIManager.Instance.UpdateMoveCount(MoveCount);
            MarkBusy();

            // Check win condition
            if (CheckWinCondition())
            {
                OnLevelComplete();
            }

            return true;
        }

        public void UndoLastMove()
        {
            if (IsBusy || GameManager.Instance.CurrentState != GameState.Playing)
                return;

            if (_moveHistory.Count == 0)
            {
                Debug.Log("No moves to undo");
                return;
            }

            if (UndoMovesRemaining <= 0)
            {
                // Show rewarded ad for undo
                RewardManager.Instance.ShowRewardedAdForUndo();
                return;
            }

            GameMove lastMove = _moveHistory.Pop();

            // Reverse the move
            _tubes[lastMove.ToTubeIndex].RemoveTop(lastMove.Amount);
            _tubes[lastMove.FromTubeIndex].AddLiquid(lastMove.Color, lastMove.Amount);

            MoveCount--;
            UndoMovesRemaining--;
            MarkBusy();
            InputManager.Instance?.DeselectAll();

            UIManager.Instance.UpdateMoveCount(MoveCount);
            UIManager.Instance.UpdateUndoCount(UndoMovesRemaining);
        }

        public void AddExtraTube()
        {
            if (CurrentLevel == null) return;

            // Add an empty tube to help player, then re-center all tubes
            CreateTube(_tubes.Count, new List<LiquidColor>());
            LayoutTubes();
        }

        public void AddUndoMoves(int count)
        {
            UndoMovesRemaining += count;
            UIManager.Instance.UpdateUndoCount(UndoMovesRemaining);
        }

        private bool CheckWinCondition()
        {
            foreach (var tube in _tubes)
            {
                if (!tube.IsComplete())
                    return false;
            }
            return true;
        }

        private void OnLevelComplete()
        {
            // Stops input; SetupLevel switches back to Playing for the next level
            GameManager.Instance.ChangeState(GameState.LevelComplete);

            // Save progress
            SaveManager.Instance.CompleteLevel(CurrentLevelIndex, MoveCount);

            // Analytics
            AnalyticsManager.Instance.LogEvent("level_complete", new Dictionary<string, object>
            {
                { "level_id", CurrentLevelIndex },
                { "move_count", MoveCount }
            });

            // Show completion UI once the last pour animation has finished
            StartCoroutine(ShowLevelCompleteDelayed(CurrentLevelIndex, MoveCount));
        }

        private IEnumerator ShowLevelCompleteDelayed(int levelIndex, int moveCount)
        {
            float delay = Mathf.Max(0f, _busyUntil - Time.time) + 0.2f;
            yield return new WaitForSeconds(delay);
            UIManager.Instance.ShowLevelCompletePopup(levelIndex, moveCount);
        }

        public void LoadNextLevel()
        {
            // Show interstitial ad before loading next level
            AdsManager.Instance.ShowInterstitial(() =>
            {
                LoadLevel(CurrentLevelIndex + 1);
            });
        }

        public void RestartLevel()
        {
            LoadLevel(CurrentLevelIndex);
        }

        public void SkipLevel()
        {
            // Show rewarded ad to skip level
            RewardManager.Instance.ShowRewardedAdForSkip(() =>
            {
                // Persist the skip so the player does not return to the skipped level
                SaveManager.Instance.SetCurrentLevel(CurrentLevelIndex + 1);
                LoadLevel(CurrentLevelIndex + 1);
            });
        }

        public List<TubeController> GetTubes()
        {
            return _tubes;
        }

        /// <summary>
        /// Current tube contents in LevelSolver format
        /// </summary>
        public List<List<int>> GetSolverState()
        {
            var state = new List<List<int>>(_tubes.Count);
            foreach (var tube in _tubes)
            {
                var colors = new List<int>();
                foreach (var segment in tube.GetLiquidStack())
                    colors.Add((int)segment.Color);
                state.Add(colors);
            }
            return state;
        }
    }

    [System.Serializable]
    public class GameMove
    {
        public int FromTubeIndex;
        public int ToTubeIndex;
        public LiquidColor Color;
        public int Amount;
    }
}
