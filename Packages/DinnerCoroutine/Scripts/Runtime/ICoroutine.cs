﻿namespace CANStudio.DinnerCoroutine
{
    internal interface ICoroutine
    {
        /// <summary>
        ///     Get current status of this coroutine.
        /// </summary>
        CoroutineStatus Status { get; }

        UpdateCase NextUpdate { get; }

        /// <summary>
        ///     Start the coroutine.
        /// </summary>
        void Start();

        /// <summary>
        ///     Stop the coroutine.
        /// </summary>
        void Stop();

        /// <summary>
        ///     Pause the coroutine, you can <see cref="Start" /> it again from where you paused.
        /// </summary>
        void Pause();

        /// <summary>
        ///     Stop the coroutine, but don't send the finish callback.
        /// </summary>
        void Interrupt();

        /// <summary>
        ///     This can be called in update, fixed update or late update, depending on <see cref="NextUpdate"/>.
        /// </summary>
        void GeneralUpdate(float deltaTime);
    }
}