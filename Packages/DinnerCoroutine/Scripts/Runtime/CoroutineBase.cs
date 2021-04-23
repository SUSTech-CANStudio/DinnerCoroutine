using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CANStudio.DinnerCoroutine
{
    /// <summary>
    /// Base class of <see cref="ICoroutine"/>s.
    /// </summary>
    public abstract class CoroutineBase : ICoroutine
    {
        private const string ErrorInfo = "Can not {0}, this coroutine is {1}.";

        private readonly IEnumerator _coroutine;
        private readonly string _functionName;
        private readonly bool _hasKeeper;
        private readonly Object _keeper;

        private UpdateCase _nextUpdate = UpdateCase.Update;

        private static readonly FieldInfo WaitForSecondsInfo =
            typeof(WaitForSeconds).GetField("m_Seconds",
                BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);
        private float _waitSeconds;

        bool ICoroutine.IsParallel => IsParallel();

        public CoroutineStatus Status { get; private set; }

        /// <summary>
        ///     Callback when the coroutine finished or stopped.
        /// </summary>
        public event Action callback;

        UpdateCase ICoroutine.NextUpdate => _hasKeeper && !_keeper ? UpdateCase.None : _nextUpdate;

        protected CoroutineBase(IEnumerator coroutine)
        {
            _coroutine = coroutine;
            Status = CoroutineStatus.NotStarted;
        }

        protected CoroutineBase(string functionName, IEnumerator coroutine)
        {
            _functionName = functionName;
            _coroutine = coroutine;
            Status = CoroutineStatus.NotStarted;
        }

        protected CoroutineBase(Object keeper, IEnumerator coroutine)
        {
            _hasKeeper = true;
            _keeper = keeper;
            _coroutine = coroutine;
            Status = CoroutineStatus.NotStarted;
        }

        protected CoroutineBase(Object keeper, string functionName, IEnumerator coroutine)
        {
            _functionName = functionName;
            _hasKeeper = true;
            _keeper = keeper;
            _coroutine = coroutine;
            Status = CoroutineStatus.NotStarted;
        }

        protected abstract bool IsParallel();

        public void Start()
        {
            switch (Status)
            {
                case CoroutineStatus.NotStarted:
                    Status = CoroutineStatus.Running;
                    _nextUpdate = UpdateCase.Update;
                    if (_hasKeeper) Daemon.Instance.Register(_keeper, this, _functionName);
                    else Daemon.Instance.Register(this, _functionName);
                    break;

                case CoroutineStatus.Paused:
                    Status = CoroutineStatus.Running;
                    break;

                case CoroutineStatus.Running:
                case CoroutineStatus.Finished:
                    throw new InvalidOperationException(string.Format(ErrorInfo, DinnerUtilities.ToPlainText(nameof(Start)), DinnerUtilities.ToPlainText(Status.ToString())));

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Stop()
        {
            switch (Status)
            {
                case CoroutineStatus.Running:
                case CoroutineStatus.Paused:
                    Status = CoroutineStatus.Finished;
                    OnCallback();
                    break;

                case CoroutineStatus.NotStarted:
                case CoroutineStatus.Finished:
                    throw new InvalidOperationException(string.Format(ErrorInfo, DinnerUtilities.ToPlainText(nameof(Stop)), DinnerUtilities.ToPlainText(Status.ToString())));

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Pause()
        {
            switch (Status)
            {
                case CoroutineStatus.Running:
                    Status = CoroutineStatus.Paused;
                    break;

                case CoroutineStatus.NotStarted:
                case CoroutineStatus.Paused:
                case CoroutineStatus.Finished:
                    throw new InvalidOperationException(string.Format(ErrorInfo, DinnerUtilities.ToPlainText(nameof(Pause)), DinnerUtilities.ToPlainText(Status.ToString())));

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Interrupt()
        {
            switch (Status)
            {
                case CoroutineStatus.Running:
                case CoroutineStatus.Paused:
                    Status = CoroutineStatus.Finished;
                    break;

                case CoroutineStatus.NotStarted:
                case CoroutineStatus.Finished:
                    throw new InvalidOperationException(string.Format(ErrorInfo,
                        DinnerUtilities.ToPlainText(nameof(Interrupt)),
                        DinnerUtilities.ToPlainText(Status.ToString())));

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void ICoroutine.GeneralUpdate(float deltaTime)
        {
            if (_coroutine is null)
            {
                Exit(false);
                return;
            }

            if (_waitSeconds > 0)
            {
                _waitSeconds -= deltaTime;
                return;
            }

            switch (Status)
            {
                case CoroutineStatus.Running:
                    break;
                case CoroutineStatus.Paused:
                    return;
                case CoroutineStatus.Finished:
                    _nextUpdate = UpdateCase.None;
                    return;
                case CoroutineStatus.NotStarted:
                    throw new InvalidOperationException("Not started coroutine shouldn't be updated");
                default:
                    throw new ArgumentOutOfRangeException();
            }

            try
            {
                if (!_coroutine.MoveNext())
                {
                    Exit(true);
                    return;
                }
            }
            catch (Exception e)
            {
                if (_hasKeeper) Debug.LogException(e, _keeper);
                else Debug.LogException(e);
                Exit(false);
                return;
            }

            var value = _coroutine.Current;
            switch (value)
            {
                case YieldInstruction instruction:
                    switch (instruction)
                    {
                        case WaitForSeconds waitForSeconds:
                            _waitSeconds += (float)WaitForSecondsInfo.GetValue(waitForSeconds);
                            _nextUpdate = UpdateCase.Update;
                            break;
                        case WaitForFixedUpdate _:
                            if (Application.isPlaying) _nextUpdate = UpdateCase.FixedUpdate;
                            break;
                        case WaitForEndOfFrame _:
                            if (Application.isPlaying) _nextUpdate = UpdateCase.OnPostRender;
                            break;
                    }
                    break;
                default:
                    _nextUpdate = UpdateCase.Update;
                    break;
            }
        }

        private void Exit(bool invokeCallBack)
        {
            Status = CoroutineStatus.Finished;
            if (invokeCallBack) OnCallback();
            _nextUpdate = UpdateCase.None;
        }

        private void OnCallback()
        {
            callback?.Invoke();
        }
    }
}