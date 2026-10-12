// Minimal stand-ins for the UnityEngine APIs used by the gameplay logic files.
// Only used by the test runner; Unity never compiles this folder.
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UnityEngine
{
    public class Object { }
    public class ScriptableObject : Object { }

    public class TextAsset : Object
    {
        public string text;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CreateAssetMenuAttribute : Attribute
    {
        public string fileName;
        public string menuName;
    }

    public static class Debug
    {
        public static bool Quiet = true;
        public static void Log(object message) { if (!Quiet) Console.WriteLine(message); }
        public static void LogWarning(object message) { Console.WriteLine("WARN: " + message); }
        public static void LogError(object message) { Console.WriteLine("ERROR: " + message); }
    }

    public static class Mathf
    {
        public static int Min(int a, int b) => Math.Min(a, b);
        public static int Max(int a, int b) => Math.Max(a, b);
        public static int Clamp(int v, int min, int max) => Math.Clamp(v, min, max);
    }

    public static class Resources
    {
        // Loads Resources/<path>.json copied next to the test binary
        public static T Load<T>(string path) where T : Object
        {
            if (typeof(T) != typeof(TextAsset)) return null;

            string file = Path.Combine(AppContext.BaseDirectory, path + ".json");
            if (!File.Exists(file)) return null;

            return new TextAsset { text = File.ReadAllText(file) } as T;
        }
    }

    public static class JsonUtility
    {
        // Like JsonUtility: public fields only, enums as numbers
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            IncludeFields = true,
            WriteIndented = true,
        };

        public static T FromJson<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
        public static string ToJson(object obj, bool prettyPrint = false) => JsonSerializer.Serialize(obj, obj.GetType(), Options);
    }
}
