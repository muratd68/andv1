using TMPro;
using UnityEngine;

namespace WaterSort.Core
{
    /// <summary>
    /// Shows the translation of Key on this text and updates it when the language changes.
    /// For fixed labels (button captions, titles); dynamic text is set from code via Loc.Get.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        public string Key;

        private TMP_Text _text;

        private void OnEnable()
        {
            _text = GetComponent<TMP_Text>();
            Loc.LanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            Loc.LanguageChanged -= Refresh;
        }

        public void Refresh()
        {
            if (_text != null && !string.IsNullOrEmpty(Key))
                _text.text = Loc.Get(Key);
        }
    }
}
