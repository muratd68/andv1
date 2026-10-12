using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace WaterSort.Core
{
    /// <summary>
    /// Generates level data procedurally or loads from resources
    /// Ensures levels are solvable and progressively difficult
    /// </summary>
    public static class LevelDataGenerator
    {
        private static Dictionary<int, LevelData> _cachedLevels = new Dictionary<int, LevelData>();

        public static LevelData GenerateLevel(int levelIndex)
        {
            // Check cache first
            if (_cachedLevels.ContainsKey(levelIndex))
                return _cachedLevels[levelIndex];

            LevelData level = null;

            // Try to load from Resources first (for handcrafted levels)
            level = LoadLevelFromResources(levelIndex);

            // If not found, generate procedurally
            if (level == null)
            {
                level = GenerateProceduralLevel(levelIndex);
            }

            // Cache it
            _cachedLevels[levelIndex] = level;

            return level;
        }

        private static LevelData LoadLevelFromResources(int levelIndex)
        {
            string path = $"Levels/Level_{levelIndex}";
            LevelData level = null;

            // Handcrafted levels are stored as JSON text assets (Resources/Levels/Level_N.json)
            TextAsset json = Resources.Load<TextAsset>(path);
            if (json != null)
            {
                level = LevelData.FromJson(json.text);
            }
            else
            {
                // ScriptableObject levels created via Create > WaterSort > Level Data
                LevelDataSO levelSO = Resources.Load<LevelDataSO>(path);
                level = levelSO != null ? levelSO.Data : null;
            }

            if (level == null)
                return null;

            if (!level.IsValid())
            {
                Debug.LogWarning($"Level {levelIndex} in Resources is invalid (each color must appear TubeCapacity times) - generating instead");
                return null;
            }

            level.LevelIndex = levelIndex;
            level.TubeCount = level.TubeConfigurations.Count;
            return level;
        }

        /// <summary>
        /// Procedurally generate a level based on difficulty curve.
        /// Deterministic: the same level index always produces the same level.
        /// </summary>
        public static LevelData GenerateProceduralLevel(int levelIndex)
        {
            System.Random random = new System.Random(GetSeed(levelIndex));

            // Difficulty progression
            int colorCount = CalculateColorCount(levelIndex);
            int extraEmptyTubes = CalculateEmptyTubes(levelIndex);
            int tubeCount = colorCount + extraEmptyTubes;
            int tubeCapacity = 4;

            LevelData level = new LevelData(levelIndex, tubeCount, colorCount)
            {
                TubeCapacity = tubeCapacity
            };

            // Generate solvable configuration
            List<List<LiquidColor>> tubes = GenerateSolvableConfiguration(colorCount, tubeCapacity, extraEmptyTubes, random, out int solutionLength);

            foreach (var tube in tubes)
            {
                level.AddTube(tube);
            }

            Debug.Log($"Generated Level {levelIndex}: {colorCount} colors, {tubeCount} tubes, solution: {solutionLength} moves");

            return level;
        }

        private static int GetSeed(int levelIndex)
        {
            return unchecked(levelIndex * 7919 + 104729);
        }

        private static int CalculateColorCount(int levelIndex)
        {
            // Start with 2-3 colors, gradually increase
            if (levelIndex < 5) return 2;
            if (levelIndex < 10) return 3;
            if (levelIndex < 20) return 4;
            if (levelIndex < 40) return 5;
            if (levelIndex < 70) return 6;
            if (levelIndex < 100) return 7;
            if (levelIndex < 150) return 8;
            if (levelIndex < 250) return 9;
            if (levelIndex < 400) return 10;

            // Max 12 colors for variety
            return Mathf.Min(12, 10 + (levelIndex - 400) / 100);
        }

        private static int CalculateEmptyTubes(int levelIndex)
        {
            // Early levels have an extra empty tube for easier solving.
            // Two empty tubes keep shuffled deals solvable at every color count;
            // difficulty comes from the number of colors instead.
            if (levelIndex < 10) return 3;
            return 2;
        }

        /// <summary>
        /// Classic deal: every color tube starts full and shuffled, plus empty tubes.
        /// Each deal is checked with LevelSolver; if no solvable deal is found the
        /// level is built by reverse moves from the solved state, which is solvable by construction.
        /// </summary>
        private static List<List<LiquidColor>> GenerateSolvableConfiguration(int colorCount, int capacity, int emptyTubes, System.Random random, out int solutionLength)
        {
            List<LiquidColor> availableColors = GetAvailableColors(colorCount);
            const int maxAttempts = 30;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                List<List<int>> deal = DealShuffled(availableColors, capacity, emptyTubes, random);

                if (!IsInteresting(deal, capacity))
                    continue;

                var solution = LevelSolver.Solve(deal, capacity, 50000);
                if (solution != null)
                {
                    solutionLength = solution.Count;
                    return ToColorLists(deal);
                }
            }

            List<List<int>> reversed = ShuffleByReverseMoves(availableColors, capacity, emptyTubes, random);
            var fallbackSolution = LevelSolver.Solve(reversed, capacity);
            solutionLength = fallbackSolution != null ? fallbackSolution.Count : -1;
            return ToColorLists(reversed);
        }

        private static List<List<int>> DealShuffled(List<LiquidColor> colors, int capacity, int emptyTubes, System.Random random)
        {
            var pool = new List<int>();
            foreach (var color in colors)
            {
                for (int i = 0; i < capacity; i++)
                    pool.Add((int)color);
            }

            // Fisher-Yates
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int tmp = pool[i];
                pool[i] = pool[j];
                pool[j] = tmp;
            }

            var tubes = new List<List<int>>();
            for (int t = 0; t < colors.Count; t++)
                tubes.Add(pool.GetRange(t * capacity, capacity));

            for (int i = 0; i < emptyTubes; i++)
                tubes.Add(new List<int>());

            return tubes;
        }

        /// <summary>
        /// Rejects deals where a tube is already sorted or nearly sorted
        /// </summary>
        private static bool IsInteresting(List<List<int>> tubes, int capacity)
        {
            foreach (var tube in tubes)
            {
                if (tube.Count == 0) continue;
                if (LevelSolver.TopRun(tube) >= capacity - 1 && tube.Count == capacity)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Starts from the solved state and applies random reverse moves.
        /// Every reverse move undoes a legal forward pour, so the result is always solvable.
        /// </summary>
        internal static List<List<int>> ShuffleByReverseMoves(List<LiquidColor> colors, int capacity, int emptyTubes, System.Random random)
        {
            var tubes = new List<List<int>>();
            foreach (var color in colors)
            {
                var tube = new List<int>();
                for (int i = 0; i < capacity; i++)
                    tube.Add((int)color);
                tubes.Add(tube);
            }

            for (int i = 0; i < emptyTubes; i++)
                tubes.Add(new List<int>());

            int shuffleMoves = 50 + colors.Count * 15;
            var candidates = new List<(int source, int target, int amount)>();

            for (int step = 0; step < shuffleMoves; step++)
            {
                candidates.Clear();

                for (int from = 0; from < tubes.Count; from++)
                {
                    var source = tubes[from];
                    if (source.Count == 0) continue;

                    int color = source[source.Count - 1];
                    int run = LevelSolver.TopRun(source);

                    for (int amount = 1; amount <= run; amount++)
                    {
                        // The forward pour put these segments on top of the same color
                        // (or into an empty tube), so that must remain true after removing them
                        if (amount == run && amount != source.Count)
                            continue;

                        for (int to = 0; to < tubes.Count; to++)
                        {
                            if (to == from) continue;

                            var target = tubes[to];
                            if (target.Count + amount > capacity) continue;

                            // The forward pour moved the whole run, so it must not join the same color
                            if (target.Count > 0 && target[target.Count - 1] == color) continue;

                            // The forward pour cannot start from a completed tube
                            if (target.Count == 0 && amount == capacity) continue;

                            candidates.Add((from, to, amount));
                        }
                    }
                }

                if (candidates.Count == 0) break;

                var pick = candidates[random.Next(candidates.Count)];
                var src = tubes[pick.source];
                int moved = src[src.Count - 1];
                src.RemoveRange(src.Count - pick.amount, pick.amount);
                for (int i = 0; i < pick.amount; i++)
                    tubes[pick.target].Add(moved);
            }

            return tubes;
        }

        private static List<List<LiquidColor>> ToColorLists(List<List<int>> tubes)
        {
            var result = new List<List<LiquidColor>>();
            foreach (var tube in tubes)
            {
                var colors = new List<LiquidColor>();
                foreach (int c in tube)
                    colors.Add((LiquidColor)c);
                result.Add(colors);
            }
            return result;
        }

        internal static List<LiquidColor> GetAvailableColors(int count)
        {
            List<LiquidColor> allColors = new List<LiquidColor>
            {
                LiquidColor.Red,
                LiquidColor.Blue,
                LiquidColor.Green,
                LiquidColor.Yellow,
                LiquidColor.Purple,
                LiquidColor.Orange,
                LiquidColor.Pink,
                LiquidColor.Cyan,
                LiquidColor.Brown,
                LiquidColor.Gray,
                LiquidColor.Lime,
                LiquidColor.Teal
            };

            return allColors.Take(count).ToList();
        }

        /// <summary>
        /// Create and save example levels to Resources folder
        /// Call this from editor script or debug menu
        /// </summary>
        public static void CreateExampleLevels()
        {
            // Level 0 - Tutorial level (very easy)
            LevelData level0 = new LevelData(0, 4, 2);
            level0.AddTube(new List<LiquidColor> { LiquidColor.Red, LiquidColor.Blue, LiquidColor.Red, LiquidColor.Blue });
            level0.AddTube(new List<LiquidColor> { LiquidColor.Blue, LiquidColor.Red, LiquidColor.Blue, LiquidColor.Red });
            level0.AddTube(new List<LiquidColor>()); // Empty
            level0.AddTube(new List<LiquidColor>()); // Empty

            Debug.Log("Level 0: " + level0.ToJson());

            // Level 1 - Easy
            LevelData level1 = new LevelData(1, 5, 3);
            level1.AddTube(new List<LiquidColor> { LiquidColor.Red, LiquidColor.Blue, LiquidColor.Green, LiquidColor.Red });
            level1.AddTube(new List<LiquidColor> { LiquidColor.Green, LiquidColor.Red, LiquidColor.Blue, LiquidColor.Green });
            level1.AddTube(new List<LiquidColor> { LiquidColor.Blue, LiquidColor.Green, LiquidColor.Red, LiquidColor.Blue });
            level1.AddTube(new List<LiquidColor>()); // Empty
            level1.AddTube(new List<LiquidColor>()); // Empty

            Debug.Log("Level 1: " + level1.ToJson());

            // Level 2 - Medium
            LevelData level2 = new LevelData(2, 6, 4);
            level2.AddTube(new List<LiquidColor> { LiquidColor.Red, LiquidColor.Yellow, LiquidColor.Blue, LiquidColor.Green });
            level2.AddTube(new List<LiquidColor> { LiquidColor.Green, LiquidColor.Red, LiquidColor.Yellow, LiquidColor.Blue });
            level2.AddTube(new List<LiquidColor> { LiquidColor.Yellow, LiquidColor.Blue, LiquidColor.Red, LiquidColor.Green });
            level2.AddTube(new List<LiquidColor> { LiquidColor.Blue, LiquidColor.Green, LiquidColor.Yellow, LiquidColor.Red });
            level2.AddTube(new List<LiquidColor>()); // Empty
            level2.AddTube(new List<LiquidColor>()); // Empty

            Debug.Log("Level 2: " + level2.ToJson());
        }
    }
}
