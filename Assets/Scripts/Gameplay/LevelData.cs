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

        // Each list represents one tube's liquid configuration from bottom to top
        public List<List<LiquidColor>> TubeConfigurations;

        public LevelData(int levelIndex, int tubeCount, int colorCount)
        {
            LevelIndex = levelIndex;
            TubeCount = tubeCount;
            ColorCount = colorCount;
            TubeConfigurations = new List<List<LiquidColor>>();
        }

        public void AddTube(List<LiquidColor> colors)
        {
            TubeConfigurations.Add(new List<LiquidColor>(colors));
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
