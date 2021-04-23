using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CANStudio.DinnerCoroutine
{
    [ExcludeFromPreset]
    [AddComponentMenu("")]
    internal class Daemon : MonoBehaviour
    {
        /// <summary>
        ///     Daemon game object name.
        /// </summary>
        private const string DefaultName = "Dinner Coroutine Daemon";

        /// <summary>
        ///     Clean not needed coroutines when <see cref="MaxGarbageCount"/> reached.
        /// </summary>
        private const float MaxGarbageCount = 30;

        // all coroutines
        private Dictionary<Object, CoroutinePool> _objectSpoonCoroutines;
        private CoroutinePool _protectedSpoonCoroutines;
        private Dictionary<Object, CoroutinePool> _objectForkCoroutines;
        private CoroutinePool _protectedForkCoroutines;

        // fixed update & post render cached coroutines
        private Queue<ICoroutine> _fixedUpdateSpoon;
        private ConcurrentQueue<ICoroutine> _fixedUpdateFork;
        private Queue<ICoroutine> _onPostRenderSpoon;
        private ConcurrentQueue<ICoroutine> _onPostRenderFork;

        /// <summary>
        ///     Get the daemon singleton instance.
        /// </summary>
        public static Daemon Instance => Lazy.Value;

        private static readonly Lazy<Daemon> Lazy =
            new Lazy<Daemon>(() =>
            {
                var go = GameObject.Find(DefaultName);
                if (!go) go = new GameObject(DefaultName);
                go.hideFlags = HideFlags.HideAndDontSave;
                var daemon = go.GetComponent<Daemon>();
                if (!daemon) daemon = go.AddComponent<Daemon>();
                daemon.Init();
                return daemon;
            });

        /// <summary>
        ///     Register a coroutine
        /// </summary>
        /// <param name="keeper">When this object destroyed, the coroutine will also be interrupted</param>
        /// <param name="coroutine"></param>
        /// <param name="functionName"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Register(Object keeper, ICoroutine coroutine, string functionName = null)
        {
            if (Application.isEditor) Init();
            if (!keeper || coroutine is null) return;

            var objectCoroutines = coroutine.IsParallel ? _objectForkCoroutines : _objectSpoonCoroutines;

            if (objectCoroutines.TryGetValue(keeper, out var pool))
            {
                if (string.IsNullOrEmpty(functionName)) pool.Add(coroutine);
                else pool.Add(functionName, coroutine);
            }
            else
            {
                objectCoroutines.Add(keeper,
                    string.IsNullOrEmpty(functionName)
                        ? new CoroutinePool {coroutine}
                        : new CoroutinePool {{functionName, coroutine}});
            }
        }

        /// <summary>
        ///     Register a coroutine
        /// </summary>
        /// <param name="coroutine"></param>
        /// <param name="functionName"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Register(ICoroutine coroutine, string functionName = null)
        {
            if (Application.isEditor) Init();
            if (coroutine is null) return;

            var protectedCoroutines = coroutine.IsParallel ? _protectedForkCoroutines : _protectedSpoonCoroutines;

            if (string.IsNullOrEmpty(functionName))
                protectedCoroutines.Add(coroutine);
            else
                protectedCoroutines.Add(functionName, coroutine);
        }

        private void Init()
        {
            if (Application.isPlaying) DontDestroyOnLoad(this);

            if (_objectSpoonCoroutines is null) _objectSpoonCoroutines = new Dictionary<Object, CoroutinePool>();
            if (_protectedSpoonCoroutines is null) _protectedSpoonCoroutines = new CoroutinePool();
            if (_objectForkCoroutines is null) _objectForkCoroutines = new Dictionary<Object, CoroutinePool>();
            if (_protectedForkCoroutines is null) _protectedForkCoroutines = new CoroutinePool();
            if (_fixedUpdateSpoon is null) _fixedUpdateSpoon = new Queue<ICoroutine>();
            if (_fixedUpdateFork is null) _fixedUpdateFork = new ConcurrentQueue<ICoroutine>();
            if (_onPostRenderSpoon is null) _onPostRenderSpoon = new Queue<ICoroutine>();
            if (_onPostRenderFork is null) _onPostRenderFork = new ConcurrentQueue<ICoroutine>();

#if UNITY_EDITOR
            if (!(EditorApplication.update is null)) EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
#endif
        }

        private void Awake()
        {
            Init();
        }

#if UNITY_EDITOR
        private void EditorUpdate()
        {
            if (Application.isPlaying) return;
            Update();
            FixedUpdate();
            OnPostRender();
        }
#endif

        private void Update()
        {
            var deltaTime = DinnerTime.deltaTime;

            var garbageCount = 0;

            // update fork coroutines
            var task = Task.Run(() =>
            {
                var roughGarbageCount = 0;
                DeleteKeys(_objectForkCoroutines);

                var coroutines = _objectForkCoroutines.SelectMany(pair => pair.Value).ToList();
                coroutines.AddRange(_protectedForkCoroutines);

                Parallel.ForEach(coroutines, coroutine =>
                {
                    if (coroutine.NextUpdate == UpdateCase.Update) coroutine.GeneralUpdate(deltaTime);
                    if (coroutine.NextUpdate == UpdateCase.None) roughGarbageCount++;
                    Enqueue(coroutine, true);
                });

                return roughGarbageCount;
            });

            // update scoop coroutines
            DeleteKeys(_objectSpoonCoroutines);
            foreach (var coroutine in _objectSpoonCoroutines.SelectMany(pair => pair.Value))
            {
                if (coroutine.NextUpdate == UpdateCase.Update) coroutine.GeneralUpdate(deltaTime);
                if (coroutine.NextUpdate == UpdateCase.None) garbageCount++;
                Enqueue(coroutine, false);
            }

            foreach (var coroutine in _protectedSpoonCoroutines)
            {
                if (coroutine.NextUpdate == UpdateCase.Update) coroutine.GeneralUpdate(deltaTime);
                if (coroutine.NextUpdate == UpdateCase.None) garbageCount++;
                Enqueue(coroutine, false);
            }

            // wait fork coroutine update finish
            task.Wait();
            garbageCount += task.Result;

            if (garbageCount > MaxGarbageCount) Clean();
        }

        private void FixedUpdate() => QueueUpdate(UpdateCase.FixedUpdate);

        private void OnPostRender() => QueueUpdate(UpdateCase.OnPostRender);

        private void QueueUpdate(UpdateCase @case)
        {
            ConcurrentQueue<ICoroutine> fork;
            Queue<ICoroutine> spoon;

            switch (@case)
            {
                case UpdateCase.FixedUpdate:
                    fork = _fixedUpdateFork;
                    spoon = _fixedUpdateSpoon;
                    break;
                case UpdateCase.OnPostRender:
                    fork = _onPostRenderFork;
                    spoon = _onPostRenderSpoon;
                    break;
                case UpdateCase.Update:
                case UpdateCase.None:
                    throw new ArgumentOutOfRangeException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(@case), @case, null);
            }

            // update fork coroutines
            var task = Task.Run(() =>
            {
                Parallel.For(0, fork.Count, i =>
                {
                    if (!fork.TryDequeue(out var coroutine)) return;
                    if (coroutine.NextUpdate == @case) coroutine.GeneralUpdate();
                    Enqueue(coroutine, true);
                });
            });

            // update scoop coroutines
            var count = spoon.Count;
            for (var i = 0; i < count; i++)
            {
                var coroutine = spoon.Dequeue();
                if (coroutine.NextUpdate == @case) coroutine.GeneralUpdate();
                Enqueue(coroutine, false);
            }

            // wait fork coroutine update finish
            task.Wait();
        }

        private void Enqueue(ICoroutine coroutine, bool isFork)
        {
            switch (coroutine.NextUpdate)
            {
                case UpdateCase.FixedUpdate:
                    if (isFork) _fixedUpdateFork.Enqueue(coroutine);
                    else _fixedUpdateSpoon.Enqueue(coroutine);
                    break;
                case UpdateCase.OnPostRender:
                    if (isFork) _onPostRenderFork.Enqueue(coroutine);
                    else _onPostRenderSpoon.Enqueue(coroutine);
                    break;
                case UpdateCase.None:
                case UpdateCase.Update:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Delete null keys.
        /// </summary>
        /// <param name="dictionary"></param>
        private static void DeleteKeys(Dictionary<Object, CoroutinePool> dictionary)
        {
            var list = dictionary.Keys.Where(key => !key);
            foreach (var key in list) dictionary.Remove(key);
        }

        private void Clean()
        {
            foreach (var pool in _objectSpoonCoroutines.Values)
            {
                pool.Clean();
            }
            foreach (var pool in _objectForkCoroutines.Values)
            {
                pool.Clean();
            }
            _protectedSpoonCoroutines.Clean();
            _protectedSpoonCoroutines.Clean();
        }
    }
}