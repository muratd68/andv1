using UnityEngine;
using System.Collections.Generic;

namespace WaterSort.Core
{
    /// <summary>
    /// Data structure for a single level
    /// Can be stored as ScriptableObject or serialized to JSON
    /// </summary>
    [System.Serializable]
    public class LevelData
    {
        public int LevelIndex;
        public int TubeCount;
        public int ColorCount;
        public int TubeCapacity = 4;

        // One entry per tube. Wrapped in TubeConfig because JsonUtility and the
        // Unity serializer cannot handle nested lists (List<List<T>>).
        public List<TubeConfig> TubeConfigurations = new List<TubeConfig>();

        public LevelData()
        {
        }

        public LevelData(int levelIndex, int tubeCount, int colorCount)
        {
            LevelIndex = levelIndex;
            TubeCount = tubeCount;
            ColorCount = colorCount;
        }

        public void AddTube(List<LiquidColor> colors)
        {
            TubeConfigurations.Add(new TubeConfig { Colors = new List<LiquidColor>(colors) });
            TubeCount = TubeConfigurations.Count;
        }

        /// <summary>
        /// Colors of one tube, bottom to top
        /// </summary>
        public List<LiquidColor> GetTubeColors(int tubeIndex)
        {
            return TubeConfigurations[tubeIndex].Colors ?? new List<LiquidColor>();
        }

        /// <summary>
        /// Tube contents as plain int lists - the format LevelSolver works with
        /// </summary>
        public List<List<int>> ToSolverState()
        {
            var state = new List<List<int>>();
            for (int i = 0; i < TubeConfigurations.Count; i++)
            {
                var tube = new List<int>();
                foreach (var color in GetTubeColors(i))
                    tube.Add((int)color);
                state.Add(tube);
            }
            return state;
        }

        /// <summary>
        /// Checks that every color appears exactly TubeCapacity times and no tube overflows
        /// </summary>
        public bool IsValid()
        {
            if (TubeConfigurations == null || TubeConfigurations.Count == 0 || TubeCapacity <= 0)
                return false;

            var counts = new Dictionary<LiquidColor, int>();
            for (int i = 0; i < TubeConfigurations.Count; i++)
            {
                var colors = GetTubeColors(i);
                if (colors.Count > TubeCapacity)
                    return false;

                foreach (var color in colors)
                {
                    if (color == LiquidColor.None)
                        return false;
                    counts.TryGetValue(color, out int c);
                    counts[color] = c + 1;
                }
            }

            foreach (var count in counts.Values)
            {
                if (count != TubeCapacity)
                    return false;
            }

            return counts.Count > 0;
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        public static LevelData FromJson(string json)
        {
            return JsonUtility.FromJson<LevelData>(json);
        }
    }

    [System.Serializable]
    public class TubeConfig
    {
        // Bottom to top
        public List<LiquidColor> Colors = new List<LiquidColor>();
    }

    /// <summary>
    /// ScriptableObject version for easy editing in Unity Editor
    /// </summary>
    [CreateAssetMenu(fileName = "Level_", menuName = "WaterSort/Level Data")]
    public class LevelDataSO : ScriptableObject
    {
        public LevelData Data;

        public void Initialize(int levelIndex, int tubeCount, int colorCount)
        {
            Data = new LevelData(levelIndex, tubeCount, colorCount);
        }
    }
}
