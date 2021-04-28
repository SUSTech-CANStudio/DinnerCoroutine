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

        private readonly ConcurrentStack<LinkedListNode<ICoroutine>> _toDestroy =
            new ConcurrentStack<LinkedListNode<ICoroutine>>();

        // all coroutines
        private readonly Dictionary<Object, CoroutinePool> _objectSpoonCoroutines = new Dictionary<Object, CoroutinePool>();
        private readonly CoroutinePool _protectedSpoonCoroutines = new CoroutinePool();
        private readonly Dictionary<Object, CoroutinePool> _objectForkCoroutines = new Dictionary<Object, CoroutinePool>();
        private readonly CoroutinePool _protectedForkCoroutines = new CoroutinePool();

        // fixed update & post render cached coroutines
        private readonly Queue<LinkedListNode<ICoroutine>> _fixedUpdateSpoon = new Queue<LinkedListNode<ICoroutine>>();
        private readonly ConcurrentQueue<LinkedListNode<ICoroutine>> _fixedUpdateFork = new ConcurrentQueue<LinkedListNode<ICoroutine>>();
        private readonly Queue<LinkedListNode<ICoroutine>> _onPostRenderSpoon = new Queue<LinkedListNode<ICoroutine>>();
        private readonly ConcurrentQueue<LinkedListNode<ICoroutine>> _onPostRenderFork = new ConcurrentQueue<LinkedListNode<ICoroutine>>();

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
            foreach (var coroutineNode in _objectForkCoroutines[keeper])
            {
                if (coroutineNode.Value.Status == CoroutineStatus.Paused || coroutineNode.Value.Status == CoroutineStatus.Running)
                    coroutineNode.Value.Stop();
            }

            foreach (var coroutineNode in _objectSpoonCoroutines[keeper])
            {
                if (coroutineNode.Value.Status == CoroutineStatus.Paused || coroutineNode.Value.Status == CoroutineStatus.Running)
                    coroutineNode.Value.Stop();
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

            // update fork coroutines
            var task = Task.Run(() =>
            {
                var roughGarbageCount = 0;
                DeleteKeys(_objectForkCoroutines);

                var coroutines = _objectForkCoroutines.SelectMany(pair => pair.Value).ToList();
                coroutines.AddRange(_protectedForkCoroutines);

                Parallel.ForEach(coroutines, coroutineNode =>
                {
                    if (coroutineNode.Value.NextUpdate == UpdateCase.Update) GeneralUpdateAndEnqueue(coroutineNode, true, deltaTime);
                });

                return roughGarbageCount;
            });

            // update scoop coroutines
            DeleteKeys(_objectSpoonCoroutines);
            foreach (var coroutineNode in _objectSpoonCoroutines.SelectMany(pair => pair.Value))
            {
                if (coroutineNode.Value.NextUpdate == UpdateCase.Update) GeneralUpdateAndEnqueue(coroutineNode, false, deltaTime);
            }

            foreach (var coroutineNode in _protectedSpoonCoroutines)
            {
                if (coroutineNode.Value.NextUpdate == UpdateCase.Update) GeneralUpdateAndEnqueue(coroutineNode, false, deltaTime);
            }

            // wait fork coroutine update finish
            task.Wait();

            // clean finished coroutines
            while (_toDestroy.TryPop(out var coroutineNode))
            {
                coroutineNode.List.Remove(coroutineNode);
            }
        }

        private void FixedUpdate() => QueueUpdate(UpdateCase.FixedUpdate);

        private void OnPostRender() => QueueUpdate(UpdateCase.OnPostRender);

        private void QueueUpdate(UpdateCase @case)
        {
            ConcurrentQueue<LinkedListNode<ICoroutine>> fork;
            Queue<LinkedListNode<ICoroutine>> spoon;

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
                    if (!fork.TryDequeue(out var coroutineNode)) return;
                    GeneralUpdateAndEnqueue(coroutineNode, true);
                });
            });

            // update scoop coroutines
            var count = spoon.Count;
            for (var i = 0; i < count; i++)
            {
                var coroutineNode = spoon.Dequeue();
                GeneralUpdateAndEnqueue(coroutineNode, false);
            }

            // wait fork coroutine update finish
            task.Wait();
        }

        private void GeneralUpdateAndEnqueue(LinkedListNode<ICoroutine> coroutineNode, bool isFork, float deltaTime = 0)
        {
            coroutineNode.Value.GeneralUpdate(deltaTime);
            switch (coroutineNode.Value.NextUpdate)
            {
                case UpdateCase.FixedUpdate:
                    if (isFork) _fixedUpdateFork.Enqueue(coroutineNode);
                    else _fixedUpdateSpoon.Enqueue(coroutineNode);
                    break;
                case UpdateCase.OnPostRender:
                    if (isFork) _onPostRenderFork.Enqueue(coroutineNode);
                    else _onPostRenderSpoon.Enqueue(coroutineNode);
                    break;
                case UpdateCase.None:
                    _toDestroy.Push(coroutineNode);
                    break;
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
    }
}