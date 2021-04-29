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
        private readonly Dictionary<Object, CoroutinePool> _objectCoroutines = new Dictionary<Object, CoroutinePool>();
        private readonly CoroutinePool _protectedCoroutines = new CoroutinePool();

        // queue for updating coroutines
        private readonly Queue<LinkedListNode<ICoroutine>> _updateSpoon = new Queue<LinkedListNode<ICoroutine>>();
        private readonly ConcurrentQueue<LinkedListNode<ICoroutine>> _updateFork =
            new ConcurrentQueue<LinkedListNode<ICoroutine>>();
        private readonly Queue<LinkedListNode<ICoroutine>> _fixedUpdateSpoon = new Queue<LinkedListNode<ICoroutine>>();
        private readonly ConcurrentQueue<LinkedListNode<ICoroutine>> _fixedUpdateFork =
            new ConcurrentQueue<LinkedListNode<ICoroutine>>();
        private readonly Queue<LinkedListNode<ICoroutine>> _onPostRenderSpoon = new Queue<LinkedListNode<ICoroutine>>();
        private readonly ConcurrentQueue<LinkedListNode<ICoroutine>> _onPostRenderFork =
            new ConcurrentQueue<LinkedListNode<ICoroutine>>();

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
        ///     Register and start a coroutine
        /// </summary>
        /// <param name="keeper">When this object destroyed, the coroutine will also be interrupted</param>
        /// <param name="coroutine"></param>
        /// <param name="functionName"></param>
        public void Register(Object keeper, ICoroutine coroutine, string functionName = null)
        {
            if (!keeper || coroutine is null) return;

            if (!_objectCoroutines.TryGetValue(keeper, out var pool))
            {
                pool = new CoroutinePool();
                _objectCoroutines.Add(keeper, pool);
            }

            var node = string.IsNullOrEmpty(functionName) ? pool.Add(coroutine) : pool.Add(functionName, coroutine);

            if (coroutine.IsParallel) _updateFork.Enqueue(node);
            else _updateSpoon.Enqueue(node);
        }

        /// <summary>
        ///     Register and start a coroutine
        /// </summary>
        /// <param name="coroutine"></param>
        /// <param name="functionName"></param>
        public void Register(ICoroutine coroutine, string functionName = null)
        {
            if (coroutine is null) return;

            var node = string.IsNullOrEmpty(functionName)
                ? _protectedCoroutines.Add(coroutine)
                : _protectedCoroutines.Add(functionName, coroutine);

            if (coroutine.IsParallel) _updateFork.Enqueue(node);
            else _updateSpoon.Enqueue(node);
        }

        /// <summary>
        ///     Stops all coroutines under keeper.
        /// </summary>
        /// <param name="keeper"></param>
        public void StopAll(Object keeper)
        {
            if (!_objectCoroutines.TryGetValue(keeper, out var coroutinePool)) return;
            foreach (var coroutineNode in coroutinePool)
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
            if (!_objectCoroutines.TryGetValue(keeper, out var coroutinePool) ||
                !coroutinePool.TryGetValue(coroutine, out var coroutines)) return;
            foreach (var c in coroutines)
            {
                if (c.Status == CoroutineStatus.Paused || c.Status == CoroutineStatus.Running)
                    c.Stop();
            }
        }

        private void Update()
        {
            DeleteKeys(_objectCoroutines);
            QueueUpdate(UpdateCase.Update);
        }

        private void FixedUpdate() => QueueUpdate(UpdateCase.FixedUpdate);

        private void OnPostRender() => QueueUpdate(UpdateCase.OnPostRender);

        private void QueueUpdate(UpdateCase @case)
        {
            ConcurrentQueue<LinkedListNode<ICoroutine>> fork;
            Queue<LinkedListNode<ICoroutine>> spoon;

            float deltaTime = 0;
            switch (@case)
            {
                case UpdateCase.Update:
                    fork = _updateFork;
                    spoon = _updateSpoon;
                    deltaTime = DinnerTime.deltaTime;
                    break;
                case UpdateCase.FixedUpdate:
                    fork = _fixedUpdateFork;
                    spoon = _fixedUpdateSpoon;
                    break;
                case UpdateCase.OnPostRender:
                    fork = _onPostRenderFork;
                    spoon = _onPostRenderSpoon;
                    break;
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
                    GeneralUpdateAndEnqueue(coroutineNode, true, deltaTime);
                });
            });

            // update scoop coroutines
            var count = spoon.Count;
            for (var i = 0; i < count; i++)
            {
                var coroutineNode = spoon.Dequeue();
                GeneralUpdateAndEnqueue(coroutineNode, false, deltaTime);
            }

            // wait fork coroutine update finish
            task.Wait();

            // clean finished coroutines
            while (_toDestroy.TryPop(out var coroutineNode))
            {
                coroutineNode.List.Remove(coroutineNode);
            }
        }

        private void GeneralUpdateAndEnqueue(LinkedListNode<ICoroutine> coroutineNode, bool isFork, float deltaTime)
        {
            coroutineNode.Value.GeneralUpdate(deltaTime);
            switch (coroutineNode.Value.NextUpdate)
            {
                case UpdateCase.Update:
                    if (isFork) _updateFork.Enqueue(coroutineNode);
                    else _updateSpoon.Enqueue(coroutineNode);
                    break;
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