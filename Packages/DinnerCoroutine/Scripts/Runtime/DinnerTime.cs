using System;
using UnityEditor;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace CANStudio.DinnerCoroutine
{
    /// <summary>
    ///     The <see cref="UnityEngine.Time" /> class doesn't work in editor mode.
    ///     When writing coroutines thar will run in editor, use this class instead.
    ///     <para />
    ///     Notice methods in this class can only be accessed in main thread.
    /// </summary>
    public class DinnerTime : Time
    {
#if UNITY_EDITOR
        public new static float time => Application.isPlaying ? UnityEngine.Time.time : Instance.Value.Time;

        public new static float unscaledTime =>
            Application.isPlaying ? UnityEngine.Time.unscaledTime : Instance.Value.UnscaledTime;

        public new static float deltaTime =>
            Application.isPlaying ? UnityEngine.Time.deltaTime : Instance.Value.DeltaTime;

        public new static float unscaledDeltaTime =>
            Application.isPlaying ? UnityEngine.Time.unscaledDeltaTime : Instance.Value.UnscaledDeltaTime;

        public new static float fixedTime =>
            Application.isPlaying ? UnityEngine.Time.fixedTime : Instance.Value.FixedTime;

        public new static float fixedUnscaledTime => Application.isPlaying
            ? UnityEngine.Time.fixedUnscaledTime
            : Instance.Value.FixedUnscaledTime;

        public new static float fixedDeltaTime =>
            Application.isPlaying ? UnityEngine.Time.fixedDeltaTime : Instance.Value.FixedDeltaTime;

        public new static float fixedUnscaledDeltaTime => UnityEngine.Time.fixedUnscaledDeltaTime;

        public new static float timeScale
        {
            get => Application.isPlaying ? UnityEngine.Time.timeScale : Instance.Value._timeScale;
            set
            {
                if (Application.isPlaying) UnityEngine.Time.timeScale = value;
                else Instance.Value._timeScale = value;
            }
        }

        public new static bool inFixedTimeStep =>
            Application.isPlaying ? UnityEngine.Time.inFixedTimeStep : Instance.Value._inFixedTimeStep;

        /// <summary>
        ///     Update event only exist in editor mode and only invoked when not playing.
        /// </summary>
        internal static event Action update;

        /// <summary>
        ///     Fixed update event only exist in editor mode and only invoked when not playing.
        /// </summary>
        internal static event Action fixedUpdate;

        /// <summary>
        ///     Create dinner time singleton.
        /// </summary>
        internal static void Awake()
        {
            if (!Instance.IsValueCreated) _ = Instance.Value;
        }

        private DinnerTime()
        {
            if (EditorApplication.update != null) EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
            _lastTimeSinceStartup = EditorApplication.timeSinceStartup;
            _timeScale = 1;
        }

        private static readonly Lazy<DinnerTime> Instance = new Lazy<DinnerTime>(() => new DinnerTime());

        private double _lastTimeSinceStartup;
        private double _unscaledDeltaTime;
        private double _unscaledTime;
        private double _fixedUnscaledTime;
        private double _deltaTime;
        private double _time;
        private double _fixedTime;
        private double _fixedDeltaTime;

        private bool _inFixedTimeStep;
        private float _timeScale;

        private float UnscaledTime => (float) _unscaledTime;
        private float Time => (float) _time;
        private float UnscaledDeltaTime => (float) _unscaledDeltaTime;
        private float DeltaTime => (float) _deltaTime;
        private float FixedUnscaledTime => (float) _fixedUnscaledTime;
        private float FixedTime => (float) _fixedTime;
        private float FixedDeltaTime => (float) _fixedDeltaTime;

        private void EditorUpdate()
        {
            if (Application.isPlaying) return;
            var timeSinceStartup = EditorApplication.timeSinceStartup;

            _unscaledDeltaTime = timeSinceStartup - _lastTimeSinceStartup;
            _unscaledTime += _unscaledDeltaTime;
            _deltaTime = _unscaledDeltaTime * _timeScale;
            _time += DeltaTime;

            _lastTimeSinceStartup = timeSinceStartup;

            // fixed update
            _inFixedTimeStep = true;
            while (_fixedUnscaledTime < _unscaledTime)
            {
                _fixedDeltaTime = UnityEngine.Time.fixedUnscaledDeltaTime * _timeScale;
                _fixedUnscaledTime += UnityEngine.Time.fixedUnscaledDeltaTime;
                _fixedTime += _fixedDeltaTime;
                fixedUpdate?.Invoke();
            }
            _inFixedTimeStep = false;

            // update
            update?.Invoke();
        }
#endif
    }
}