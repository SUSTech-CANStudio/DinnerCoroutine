using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly Dictionary<Object, CoroutinePool> _objectSpoonCoroutines = new Dictionary<Object, CoroutinePool>();
        private readonly CoroutinePool _protectedSpoonCoroutines = new CoroutinePool();
        private readonly Dictionary<Object, CoroutinePool> _objectForkCoroutines = new Dictionary<Object, CoroutinePool>();
        private readonly CoroutinePool _protectedForkCoroutines = new CoroutinePool();

        // fixed update & post render cached coroutines
        private readonly Queue<ICoroutine> _fixedUpdateSpoon = new Queue<ICoroutine>();
        private readonly ConcurrentQueue<ICoroutine> _fixedUpdateFork = new ConcurrentQueue<ICoroutine>();
        private readonly Queue<ICoroutine> _onPostRenderSpoon = new Queue<ICoroutine>();
        private readonly ConcurrentQueue<ICoroutine> _onPostRenderFork = new ConcurrentQueue<ICoroutine>();

        /// <summary>
        ///     Get the daemon singleton instance.
        /// </summary>
        public static Daemon Instance => Lazy.Value;

        private static readonly Lazy<Daemon> Lazy =
            new Lazy<Daemon>(() =>
            {
                var go = GameObject.Find(DefaultName);
                Daemon daemon;
                if (go)
                {
                    daemon = go.GetComponent<Daemon>();
                    if (!daemon) daemon = go.AddComponent<Daemon>();
                }
                else
                {
                    go = new GameObject(DefaultName) {hideFlags = HideFlags.HideAndDontSave};
#if UNITY_EDITOR
                    if (Application.isPlaying)
#endif
                    DontDestroyOnLoad(go);
                    daemon = go.AddComponent<Daemon>();
                }

#if UNITY_EDITOR
                if (Application.isPlaying)
#endif
                DontDestroyOnLoad(daemon);
                DinnerTime.update -= daemon.Update;
                DinnerTime.update -= daemon.OnPostRender;
                DinnerTime.fixedUpdate -= daemon.FixedUpdate;
                DinnerTime.update += daemon.Update;
                DinnerTime.update += daemon.OnPostRender;
                DinnerTime.fixedUpdate += daemon.FixedUpdate;

                DinnerTime.Awake();

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
            if (coroutine is null) return;

            var protectedCoroutines = coroutine.IsParallel ? _protectedForkCoroutines : _protectedSpoonCoroutines;

            if (string.IsNullOrEmpty(functionName))
                protectedCoroutines.Add(coroutine);
            else
                protectedCoroutines.Add(functionName, coroutine);
        }

        /// <summary>
        ///     Stops all coroutines under keeper.
        /// </summary>
        /// <param name="keeper"></param>
        public void StopAll(Object keeper)
        {
            foreach (var c in _objectForkCoroutines[keeper])
            {
                if (c.Status == CoroutineStatus.Paused || c.Status == CoroutineStatus.Running)
                    c.Stop();
            }

            foreach (var c in _objectSpoonCoroutines[keeper])
            {
                if (c.Status == CoroutineStatus.Paused || c.Status == CoroutineStatus.Running)
                    c.Stop();
            }
        }

        /// <summary>
        ///     Stops all coroutines that registered with function name under keeper.
        /// </summary>
        /// <param name="keeper"></param>
        /// <param name="coroutine">The function name when registering the coroutine.</param>
        public void StopAll(Object keeper, string coroutine)
        {
            foreach (var c in _objectForkCoroutines[keeper][coroutine])
            {
                if (c.Status == CoroutineStatus.Paused || c.Status == CoroutineStatus.Running)
                    c.Stop();
            }

            foreach (var c in _objectSpoonCoroutines[keeper][coroutine])
            {
                if (c.Status == CoroutineStatus.Paused || c.Status == CoroutineStatus.Running)
                    c.Stop();
            }
        }

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
                    if (coroutine.NextUpdate == UpdateCase.Update) GeneralUpdateAndEnqueue(coroutine, true, deltaTime);
                    if (coroutine.NextUpdate == UpdateCase.None) roughGarbageCount++;
                });

                return roughGarbageCount;
            });

            // update scoop coroutines
            DeleteKeys(_objectSpoonCoroutines);
            foreach (var coroutine in _objectSpoonCoroutines.SelectMany(pair => pair.Value))
            {
                if (coroutine.NextUpdate == UpdateCase.Update) GeneralUpdateAndEnqueue(coroutine, false, deltaTime);
                if (coroutine.NextUpdate == UpdateCase.None) garbageCount++;
            }

            foreach (var coroutine in _protectedSpoonCoroutines)
            {
                if (coroutine.NextUpdate == UpdateCase.Update) GeneralUpdateAndEnqueue(coroutine, false, deltaTime);
                if (coroutine.NextUpdate == UpdateCase.None) garbageCount++;
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
                    GeneralUpdateAndEnqueue(coroutine, true);
                });
            });

            // update scoop coroutines
            var count = spoon.Count;
            for (var i = 0; i < count; i++)
            {
                var coroutine = spoon.Dequeue();
                GeneralUpdateAndEnqueue(coroutine, false);
            }

            // wait fork coroutine update finish
            task.Wait();
        }

        private void GeneralUpdateAndEnqueue(ICoroutine coroutine, bool isFork, float deltaTime = 0)
        {
            coroutine.GeneralUpdate(deltaTime);
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
            var list = dictionary.Keys.Where(key => !key).ToArray();
            foreach (var key in list)
            {
                dictionary[key].Destroy();
                dictionary.Remove(key);
            }
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