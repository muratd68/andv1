using UnityEngine;

namespace WaterSort.Core
{
    /// <summary>
    /// Validates if a move from one tube to another is legal according to game rules
    /// </summary>
    public static class MoveValidator
    {
        /// <summary>
        /// Check if liquid can be poured from sourceTube to targetTube
        /// Rules:
        /// 1. Source tube must not be empty
        /// 2. Source tube must not be complete (to avoid unnecessary moves)
        /// 3. Target tube must have available space
        /// 4. If target is empty, any color can be poured
        /// 5. If target has liquid, top colors must match
        /// </summary>
        public static bool IsValidMove(TubeController sourceTube, TubeController targetTube)
        {
            // Cannot pour from empty tube
            if (sourceTube.IsEmpty())
            {
                Debug.Log("Invalid: Source tube is empty");
                return false;
            }

            // Cannot pour to same tube
            if (sourceTube == targetTube)
            {
                return false;
            }

            // Don't allow pouring from a completed tube
            if (sourceTube.IsComplete())
            {
                Debug.Log("Invalid: Source tube is already complete");
                return false;
            }

            // Target tube must have space
            if (targetTube.IsFull())
            {
                Debug.Log("Invalid: Target tube is full");
                return false;
            }

            // If target is empty, any color can be poured
            if (targetTube.IsEmpty())
            {
                return true;
            }

            // If target has liquid, colors must match
            LiquidColor sourceColor = sourceTube.GetTopColor();
            LiquidColor targetColor = targetTube.GetTopColor();

            if (sourceColor != targetColor)
            {
                Debug.Log($"Invalid: Colors don't match - {sourceColor} vs {targetColor}");
                return false;
            }

            // Check if there's enough space for the amount being poured
            int amountToPour = sourceTube.GetTopColorCount();
            int availableSpace = targetTube.GetAvailableSpace();

            if (amountToPour > availableSpace)
            {
                Debug.Log($"Invalid: Not enough space - trying to pour {amountToPour} but only {availableSpace} available");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the current level state is solvable
        /// This is a simple heuristic - can be improved
        /// </summary>
        public static bool IsLevelSolvable(System.Collections.Generic.List<TubeController> tubes)
        {
            // Count occurrences of each color
            System.Collections.Generic.Dictionary<LiquidColor, int> colorCounts =
                new System.Collections.Generic.Dictionary<LiquidColor, int>();

            foreach (var tube in tubes)
            {
                var stack = tube.GetLiquidStack();
                foreach (var segment in stack)
                {
                    if (segment.Color == LiquidColor.None) continue;

                    if (!colorCounts.ContainsKey(segment.Color))
                        colorCounts[segment.Color] = 0;

                    colorCounts[segment.Color]++;
                }
            }

            // Each color should appear exactly MaxCapacity times
            int maxCapacity = tubes[0].MaxCapacity;
            foreach (var count in colorCounts.Values)
            {
                if (count != maxCapacity)
                {
                    Debug.LogWarning($"Invalid level: Color appears {count} times instead of {maxCapacity}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Calculate difficulty score for a level (0-100)
        /// Higher score = more difficult
        /// </summary>
        public static int CalculateDifficulty(System.Collections.Generic.List<System.Collections.Generic.List<LiquidColor>> tubeConfigs)
        {
            int difficulty = 0;

            // More tubes = harder
            difficulty += tubeConfigs.Count * 2;

            // Count number of different colors
            System.Collections.Generic.HashSet<LiquidColor> uniqueColors =
                new System.Collections.Generic.HashSet<LiquidColor>();

            foreach (var tube in tubeConfigs)
            {
                foreach (var color in tube)
                {
                    if (color != LiquidColor.None)
                        uniqueColors.Add(color);
                }
            }

            // More colors = harder
            difficulty += uniqueColors.Count * 5;

            // Calculate fragmentation (how mixed up the colors are)
            int fragmentationScore = 0;
            foreach (var tube in tubeConfigs)
            {
                LiquidColor prevColor = LiquidColor.None;
                for (int i = 0; i < tube.Count; i++)
                {
                    if (i > 0 && tube[i] != prevColor && tube[i] != LiquidColor.None)
                    {
                        fragmentationScore += 3; // Color change = fragmentation
                    }
                    prevColor = tube[i];
                }
            }

            difficulty += fragmentationScore;

            // Count empty tubes - more empty tubes = easier
            int emptyTubes = 0;
            foreach (var tube in tubeConfigs)
            {
                if (tube.Count == 0)
                    emptyTubes++;
            }

            difficulty -= emptyTubes * 10;

            // Clamp to 0-100
            return Mathf.Clamp(difficulty, 0, 100);
        }
    }
}
