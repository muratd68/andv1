// Compile-only stand-in for TextMeshPro (not available on NuGet).
// Signatures follow the real libraries for the members this project uses.
using System;
using UnityEngine;

namespace TMPro
{
    public class TextMeshProUGUI : UnityEngine.UI.MaskableGraphic
    {
        public string text { get; set; }
    }
}
