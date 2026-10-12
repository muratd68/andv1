// Compile-only stand-ins for DOTween and TextMeshPro (not available on NuGet).
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

namespace DG.Tweening
{
    public enum Ease { Unset, Linear, InBack, OutBack, OutBounce, OutQuad, InOutSine }

    public delegate void TweenCallback();

    public class Tween { }
    public class Tweener : Tween { }

    public static class TweenSettingsExtensions
    {
        public static T SetEase<T>(this T t, Ease ease) where T : Tween => t;
        public static T SetDelay<T>(this T t, float delay) where T : Tween => t;
        public static T SetLoops<T>(this T t, int loops) where T : Tween => t;
        public static T OnComplete<T>(this T t, TweenCallback action) where T : Tween => t;
    }

    public static class ShortcutExtensions
    {
        public static Tweener DOLocalMove(this Transform target, Vector3 endValue, float duration, bool snapping = false) => new Tweener();
        public static Tweener DOMove(this Transform target, Vector3 endValue, float duration, bool snapping = false) => new Tweener();
        public static Tweener DOScale(this Transform target, Vector3 endValue, float duration) => new Tweener();
        public static Tweener DOScale(this Transform target, float endValue, float duration) => new Tweener();
        public static Tweener DOShakePosition(this Transform target, float duration, float strength = 1, int vibrato = 10, float randomness = 90, bool snapping = false, bool fadeOut = true) => new Tweener();
        public static Tweener DOPunchScale(this Transform target, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1) => new Tweener();
        public static Tweener DOColor(this SpriteRenderer target, Color endValue, float duration) => new Tweener();
        public static int DOKill(this Component target, bool complete = false) => 0;
    }
}
