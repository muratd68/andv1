using System;
using System.Collections.Generic;
using UnityEngine;
#if WATERSORT_FIREBASE
using Firebase;
using Firebase.Extensions;
#endif

namespace WaterSort.Core
{
    /// <summary>
    /// Runs Firebase's dependency check once and notifies every waiting service.
    /// Without the WATERSORT_FIREBASE define (Firebase SDK not imported) it reports "not available".
    /// </summary>
    public static class FirebaseInit
    {
        private enum State { NotStarted, Checking, Ready, Failed }

        private static State _state = State.NotStarted;
        private static readonly List<Action<bool>> _waiting = new List<Action<bool>>();

        /// <summary>
        /// Calls onDone(true) on the main thread once Firebase can be used, onDone(false) if it cannot
        /// </summary>
        public static void WhenReady(Action<bool> onDone)
        {
#if WATERSORT_FIREBASE
            switch (_state)
            {
                case State.Ready: onDone(true); return;
                case State.Failed: onDone(false); return;
            }

            _waiting.Add(onDone);
            if (_state == State.Checking) return;

            _state = State.Checking;
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                bool ok = !task.IsFaulted && !task.IsCanceled && task.Result == DependencyStatus.Available;
                _state = ok ? State.Ready : State.Failed;
                if (!ok)
                    Debug.LogError("Firebase is not available: " + (task.IsFaulted ? task.Exception?.Message : task.Result.ToString()));

                var callbacks = _waiting.ToArray();
                _waiting.Clear();
                foreach (var callback in callbacks)
                    callback(ok);
            });
#else
            onDone(false);
#endif
        }
    }
}
