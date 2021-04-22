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
        public new static float time => Application.isPlaying ? Time.time : Instance.Value._editorTime;

        public new static float unscaledTime =>
            Application.isPlaying ? Time.unscaledTime : Instance.Value._editorUnscaledTime;

        public new static float deltaTime => Application.isPlaying ? Time.deltaTime : Instance.Value._editorDeltaTime;

        public new static float unscaledDeltaTime =>
            Application.isPlaying ? Time.unscaledDeltaTime : Instance.Value._editorUnscaledDeltaTime;

        /// <summary>
        ///     If in editor mode, returns same value as <see cref="time" />.
        /// </summary>
        public new static float fixedTime => Application.isPlaying ? Time.fixedTime : Instance.Value._editorTime;

        /// <summary>
        ///     If in editor mode, returns same value as <see cref="unscaledTime" />.
        /// </summary>
        public new static float fixedUnscaledTime =>
            Application.isPlaying ? Time.fixedUnscaledTime : Instance.Value._editorUnscaledDeltaTime;

        /// <summary>
        ///     If in editor mode, returns same value as <see cref="deltaTime" />.
        /// </summary>
        public new static float fixedDeltaTime =>
            Application.isPlaying ? Time.fixedDeltaTime : Instance.Value._editorDeltaTime;

        /// <summary>
        ///     If in editor mode, returns same value as <see cref="unscaledDeltaTime" />.
        /// </summary>
        public new static float fixedUnscaledDeltaTime => Application.isPlaying
            ? Time.fixedUnscaledDeltaTime
            : Instance.Value._editorUnscaledDeltaTime;

        public new static float timeScale
        {
            get => Application.isPlaying ? Time.timeScale : Instance.Value._editorTimeScale;
            set
            {
                if (Application.isPlaying) Time.timeScale = value;
                else Instance.Value._editorTimeScale = value;
            }
        }

        private DinnerTime()
        {
        }

        private static readonly Lazy<DinnerTime> Instance = new Lazy<DinnerTime>(() =>
        {
            var self = new DinnerTime();
            if (EditorApplication.update != null) EditorApplication.update -= self.EditorUpdate;
            EditorApplication.update += self.EditorUpdate;
            self._lastTimeSinceStartup = EditorApplication.timeSinceStartup;
            self._editorTimeScale = 1;
            return self;
        });

        private double _lastTimeSinceStartup;
        private float _editorUnscaledTime;
        private float _editorTime;
        private float _editorUnscaledDeltaTime;
        private float _editorDeltaTime;
        private float _editorTimeScale;

        private void EditorUpdate()
        {
            if (Application.isPlaying) return;
            var timeSinceStartup = EditorApplication.timeSinceStartup;

            _editorUnscaledDeltaTime = (float) (timeSinceStartup - _lastTimeSinceStartup);
            _editorDeltaTime = _editorUnscaledDeltaTime * _editorTimeScale;
            _editorUnscaledTime += _editorUnscaledDeltaTime;
            _editorTime += _editorDeltaTime;

            _lastTimeSinceStartup = timeSinceStartup;
        }
#endif
    }
}