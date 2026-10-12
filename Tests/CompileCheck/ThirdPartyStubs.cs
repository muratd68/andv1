// Compile-only stand-in for TextMeshPro (com.unity.textmeshpro is a Unity package, not on NuGet).
// Declares the members this project uses, with the same names and types as TMP 3.0.
using UnityEngine;

namespace TMPro
{
    public enum TextAlignmentOptions
    {
        TopLeft = 257, Top = 258, TopRight = 260,
        Left = 513, Center = 514, Right = 516,
        BottomLeft = 1025, Bottom = 1026, BottomRight = 1028,
        MidlineLeft = 4097, Midline = 4098, MidlineRight = 4100
    }

    [System.Flags]
    public enum FontStyles { Normal = 0, Bold = 1, Italic = 2, Underline = 4 }

    public abstract class TMP_Text : UnityEngine.UI.MaskableGraphic
    {
        public string text { get; set; }
        public float fontSize { get; set; }
        public TextAlignmentOptions alignment { get; set; }
        public FontStyles fontStyle { get; set; }
        public bool enableWordWrapping { get; set; }
        public float lineSpacing { get; set; }
    }

    public class TextMeshProUGUI : TMP_Text { }

    public class TMP_Settings : ScriptableObject { }
}
