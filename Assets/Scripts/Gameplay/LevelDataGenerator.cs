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
            LevelDataSO levelSO = Resources.Load<LevelDataSO>($"Levels/Level_{levelIndex}");
            return levelSO?.Data;
        }

        /// <summary>
        /// Procedurally generate a level based on difficulty curve
        /// </summary>
        private static LevelData GenerateProceduralLevel(int levelIndex)
        {
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
            List<List<LiquidColor>> tubes = GenerateSolvableConfiguration(colorCount, tubeCapacity, extraEmptyTubes);

            foreach (var tube in tubes)
            {
                level.AddTube(tube);
            }

            Debug.Log($"Generated Level {levelIndex}: {colorCount} colors, {tubeCount} tubes, difficulty: {MoveValidator.CalculateDifficulty(tubes)}");

            return level;
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
            // Early levels have more empty tubes for easier solving
            if (levelIndex < 10) return 3;
            if (levelIndex < 30) return 2;
            if (levelIndex < 100) return 2;

            // Later levels may have only 1-2 empty tubes
            return Random.Range(1, 3);
        }

        /// <summary>
        /// Generate a solvable configuration by starting from solved state and shuffling
        /// </summary>
        private static List<List<LiquidColor>> GenerateSolvableConfiguration(int colorCount, int capacity, int emptyTubes)
        {
            List<List<LiquidColor>> tubes = new List<List<LiquidColor>>();

            // Create solved state first - each color in its own tube
            List<LiquidColor> availableColors = GetAvailableColors(colorCount);

            foreach (var color in availableColors)
            {
                List<LiquidColor> tube = new List<LiquidColor>();
                for (int i = 0; i < capacity; i++)
                {
                    tube.Add(color);
                }
                tubes.Add(tube);
            }

            // Add empty tubes
            for (int i = 0; i < emptyTubes; i++)
            {
                tubes.Add(new List<LiquidColor>());
            }

            // Now shuffle by doing random valid reverse moves
            int shuffleMoves = 50 + (colorCount * 10); // More colors = more shuffling

            System.Random random = new System.Random(System.DateTime.Now.Millisecond + colorCount);

            for (int i = 0; i < shuffleMoves; i++)
            {
                // Pick random source tube that has liquid
                var fullTubes = tubes.Where((t, idx) => t.Count > 0).ToList();
                if (fullTubes.Count == 0) break;

                int sourceIdx = tubes.IndexOf(fullTubes[random.Next(fullTubes.Count)]);

                // Pick random target tube that has space
                var notFullTubes = tubes.Where((t, idx) => idx != sourceIdx && t.Count < capacity).ToList();
                if (notFullTubes.Count == 0) continue;

                int targetIdx = tubes.IndexOf(notFullTubes[random.Next(notFullTubes.Count)]);

                // Move one segment
                if (tubes[sourceIdx].Count > 0)
                {
                    LiquidColor colorToMove = tubes[sourceIdx][tubes[sourceIdx].Count - 1];
                    tubes[sourceIdx].RemoveAt(tubes[sourceIdx].Count - 1);
                    tubes[targetIdx].Add(colorToMove);
                }
            }

            return tubes;
        }

        private static List<LiquidColor> GetAvailableColors(int count)
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
