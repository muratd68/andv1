using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaterSort.Core
{
    public enum TweenEase
    {
        Linear,
        OutQuad,
        OutBack,
        InBack,
        OutBounce
    }

    /// <summary>
    /// Handle returned by SimpleTween, used to chain settings:
    /// SimpleTween.Scale(t, Vector3.one, 0.2f).SetDelay(0.1f).OnComplete(() => ...)
    /// </summary>
    public class TweenHandle
    {
        internal Transform Target;
        internal float Duration;
        internal float Delay;
        internal float Elapsed;
        internal TweenEase Ease;
        internal Action Complete;
        internal bool Started;
        internal bool Finished;

        // Set when the tween starts (after its delay), so chained tweens begin from the current value
        internal Action OnStart;
        internal Action<float> Apply;

        public TweenHandle SetDelay(float delay)
        {
            Delay = delay;
            return this;
        }

        public TweenHandle SetEase(TweenEase ease)
        {
            Ease = ease;
            return this;
        }

        public TweenHandle OnComplete(Action onComplete)
        {
            Complete = onComplete;
            return this;
        }
    }

    /// <summary>
    /// Small built-in tween engine for the few animations the game needs
    /// (move, scale, shake, punch). Replaces DOTween so the project has no
    /// Asset Store dependency.
    /// </summary>
    public class SimpleTween : MonoBehaviour
    {
        private static SimpleTween _runner;
        private readonly List<TweenHandle> _tweens = new List<TweenHandle>();
        private readonly List<TweenHandle> _pending = new List<TweenHandle>();
        private bool _updating;

        private static SimpleTween Runner
        {
            get
            {
                if (_runner == null)
                {
                    var go = new GameObject("[SimpleTween]");
                    go.hideFlags = HideFlags.HideInHierarchy;
                    DontDestroyOnLoad(go);
                    _runner = go.AddComponent<SimpleTween>();
                }
                return _runner;
            }
        }

        #region Public API

        public static TweenHandle LocalMove(Transform target, Vector3 to, float duration, TweenEase ease = TweenEase.OutQuad)
        {
            Vector3 from = Vector3.zero;
            var handle = Create(target, duration, ease);
            handle.OnStart = () => from = target.localPosition;
            handle.Apply = t => target.localPosition = Vector3.LerpUnclamped(from, to, t);
            return handle;
        }

        public static TweenHandle Scale(Transform target, Vector3 to, float duration, TweenEase ease = TweenEase.OutQuad)
        {
            Vector3 from = Vector3.one;
            var handle = Create(target, duration, ease);
            handle.OnStart = () => from = target.localScale;
            handle.Apply = t => target.localScale = Vector3.LerpUnclamped(from, to, t);
            return handle;
        }

        /// <summary>
        /// Random horizontal shake that fades out and returns to the start position
        /// </summary>
        public static TweenHandle ShakePosition(Transform target, float duration, float strength)
        {
            Vector3 origin = Vector3.zero;
            var handle = Create(target, duration, TweenEase.Linear);
            handle.OnStart = () => origin = target.localPosition;
            handle.Apply = t =>
            {
                float falloff = 1f - t;
                float offset = t >= 1f ? 0f : UnityEngine.Random.Range(-strength, strength) * falloff;
                target.localPosition = origin + new Vector3(offset, 0f, 0f);
            };
            return handle;
        }

        /// <summary>
        /// Springs the scale out by punch and back to the start scale
        /// </summary>
        public static TweenHandle PunchScale(Transform target, Vector3 punch, float duration, int vibrato = 4)
        {
            Vector3 origin = Vector3.one;
            var handle = Create(target, duration, TweenEase.Linear);
            handle.OnStart = () => origin = target.localScale;
            handle.Apply = t =>
            {
                float wave = t >= 1f ? 0f : Mathf.Sin(t * Mathf.PI * vibrato) * (1f - t);
                target.localScale = origin + punch * wave;
            };
            return handle;
        }

        /// <summary>
        /// Stops every tween on the target. complete = true jumps them to their end state first
        /// and runs their OnComplete callbacks.
        /// </summary>
        public static void Kill(Transform target, bool complete = false)
        {
            if (_runner == null || target == null) return;
            _runner.KillInternal(target, complete);
        }

        #endregion

        private static TweenHandle Create(Transform target, float duration, TweenEase ease)
        {
            var handle = new TweenHandle
            {
                Target = target,
                Duration = Mathf.Max(0.0001f, duration),
                Ease = ease
            };

            var runner = Runner;
            if (runner._updating)
                runner._pending.Add(handle);
            else
                runner._tweens.Add(handle);

            return handle;
        }

        private void KillInternal(Transform target, bool complete)
        {
            KillIn(_tweens, target, complete);
            KillIn(_pending, target, complete);
        }

        private static void KillIn(List<TweenHandle> list, Transform target, bool complete)
        {
            // Copy: completing a tween can start or kill other tweens
            foreach (var tween in list.ToArray())
            {
                if (tween.Finished || tween.Target != target) continue;

                tween.Finished = true;
                if (complete)
                {
                    if (!tween.Started)
                    {
                        tween.Started = true;
                        tween.OnStart?.Invoke();
                    }
                    tween.Apply?.Invoke(1f);
                    tween.Complete?.Invoke();
                }
            }
        }

        private void Update()
        {
            _updating = true;
            float dt = Time.deltaTime;

            for (int i = 0; i < _tweens.Count; i++)
            {
                var tween = _tweens[i];
                if (tween.Finished) continue;

                // Target destroyed (e.g. scene change)
                if (tween.Target == null)
                {
                    tween.Finished = true;
                    continue;
                }

                if (tween.Delay > 0f)
                {
                    tween.Delay -= dt;
                    continue;
                }

                if (!tween.Started)
                {
                    tween.Started = true;
                    tween.OnStart?.Invoke();
                }

                tween.Elapsed += dt;
                float t = Mathf.Clamp01(tween.Elapsed / tween.Duration);
                tween.Apply?.Invoke(Evaluate(tween.Ease, t));

                if (t >= 1f)
                {
                    tween.Finished = true;
                    tween.Complete?.Invoke();
                }
            }

            _updating = false;
            _tweens.RemoveAll(tween => tween.Finished);
            _tweens.AddRange(_pending);
            _pending.Clear();
        }

        public static float Evaluate(TweenEase ease, float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            switch (ease)
            {
                case TweenEase.OutQuad:
                    return 1f - (1f - t) * (1f - t);
                case TweenEase.OutBack:
                    return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
                case TweenEase.InBack:
                    return c3 * t * t * t - c1 * t * t;
                case TweenEase.OutBounce:
                    return OutBounce(t);
                default:
                    return t;
            }
        }

        private static float OutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1) return n1 * t * t;
            if (t < 2f / d1) { t -= 1.5f / d1; return n1 * t * t + 0.75f; }
            if (t < 2.5f / d1) { t -= 2.25f / d1; return n1 * t * t + 0.9375f; }
            t -= 2.625f / d1;
            return n1 * t * t + 0.984375f;
        }
    }
}
