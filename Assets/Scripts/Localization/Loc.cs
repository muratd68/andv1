using System;
using UnityEngine;

namespace WaterSort.Core
{
    /// <summary>
    /// Runtime access to translated text.
    /// Default language follows the device (Turkish devices get Turkish, everything else English);
    /// the player's choice in Settings is saved and wins from then on.
    /// </summary>
    public static class Loc
    {
        private const string PrefKey = "Language";

        private static bool _loaded;
        private static Language _current;

        public static event Action LanguageChanged;

        public static Language Current
        {
            get
            {
                if (!_loaded)
                {
                    _loaded = true;
                    _current = PlayerPrefs.HasKey(PrefKey)
                        ? (Language)PlayerPrefs.GetInt(PrefKey)
                        : (Application.systemLanguage == SystemLanguage.Turkish ? Language.Turkish : Language.English);
                }
                return _current;
            }
        }

        public static void SetLanguage(Language language)
        {
            _loaded = true;
            _current = language;
            PlayerPrefs.SetInt(PrefKey, (int)language);
            PlayerPrefs.Save();
            LanguageChanged?.Invoke();
        }

        /// <summary>
        /// Switches between Turkish and English
        /// </summary>
        public static void ToggleLanguage()
        {
            SetLanguage(Current == Language.Turkish ? Language.English : Language.Turkish);
        }

        public static string Get(string key)
        {
            if (LocalizationTable.Entries.TryGetValue(key, out string[] values))
            {
                int index = (int)Current;
                return index < values.Length ? values[index] : values[0];
            }

            Debug.LogWarning($"Missing localization key: {key}");
            return key;
        }

        public static string Get(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }
    }
}
