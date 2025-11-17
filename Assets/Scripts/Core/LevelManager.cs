using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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

        [Header("Level Setup")]
        public int CurrentLevelIndex { get; private set; }
        public LevelData CurrentLevel { get; private set; }

        [Header("Game State")]
        public int MoveCount { get; private set; }
        public int UndoMovesRemaining { get; private set; } = 5;

        [Header("References")]
        public Transform TubesContainer;
        public GameObject TubePrefab;

        private List<TubeController> _tubes = new List<TubeController>();
        private Stack<GameMove> _moveHistory = new Stack<GameMove>();

        private void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            // Load current level from save
            CurrentLevelIndex = SaveManager.Instance.GetCurrentLevel();
            LoadLevel(CurrentLevelIndex);
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
            _moveHistory.Clear();

            // Create tubes from level data
            for (int i = 0; i < CurrentLevel.TubeConfigurations.Count; i++)
            {
                GameObject tubeObj = Instantiate(TubePrefab, TubesContainer);
                TubeController tube = tubeObj.GetComponent<TubeController>();

                // Position tubes in a grid or line
                float spacing = 1.5f;
                int tubesPerRow = Mathf.Min(5, CurrentLevel.TubeConfigurations.Count);
                int row = i / tubesPerRow;
                int col = i % tubesPerRow;

                float xOffset = (col - tubesPerRow / 2f) * spacing;
                float yOffset = -row * spacing;

                tubeObj.transform.localPosition = new Vector3(xOffset, yOffset, 0);

                tube.Initialize(i, CurrentLevel.TubeConfigurations[i]);
                _tubes.Add(tube);
            }

            UIManager.Instance.UpdateMoveCount(MoveCount);
        }

        private void ClearTubes()
        {
            foreach (var tube in _tubes)
            {
                if (tube != null)
                    Destroy(tube.gameObject);
            }
            _tubes.Clear();
        }

        public bool TryMove(int fromTubeIndex, int toTubeIndex)
        {
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

            // Check win condition
            if (CheckWinCondition())
            {
                OnLevelComplete();
            }

            return true;
        }

        public void UndoLastMove()
        {
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

            UIManager.Instance.UpdateMoveCount(MoveCount);
            UIManager.Instance.UpdateUndoCount(UndoMovesRemaining);
        }

        public void AddExtraTube()
        {
            // Add an empty tube to help player
            GameObject tubeObj = Instantiate(TubePrefab, TubesContainer);
            TubeController tube = tubeObj.GetComponent<TubeController>();

            // Position at the end
            int index = _tubes.Count;
            float spacing = 1.5f;
            int tubesPerRow = 5;
            int row = index / tubesPerRow;
            int col = index % tubesPerRow;

            float xOffset = (col - tubesPerRow / 2f) * spacing;
            float yOffset = -row * spacing;

            tubeObj.transform.localPosition = new Vector3(xOffset, yOffset, 0);

            tube.Initialize(index, new List<LiquidColor>());
            _tubes.Add(tube);
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
            GameManager.Instance.ChangeState(GameState.LevelComplete);

            // Save progress
            SaveManager.Instance.CompleteLevel(CurrentLevelIndex, MoveCount);

            // Analytics
            AnalyticsManager.Instance.LogEvent("level_complete", new Dictionary<string, object>
            {
                { "level_id", CurrentLevelIndex },
                { "move_count", MoveCount }
            });

            // Show completion UI and ad
            UIManager.Instance.ShowLevelCompletePopup(CurrentLevelIndex, MoveCount);
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
                LoadLevel(CurrentLevelIndex + 1);
            });
        }

        public List<TubeController> GetTubes()
        {
            return _tubes;
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
