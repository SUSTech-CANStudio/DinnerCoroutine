using System;
using System.Collections;
using System.Collections.Generic;

namespace CANStudio.DinnerCoroutine
{
    internal class CoroutinePool : IEnumerable<LinkedListNode<ICoroutine>>
    {
        private readonly IDictionary<string, LinkedList<ICoroutine>> _dictionary;

        private readonly LinkedList<ICoroutine> _list;

        public CoroutinePool()
        {
            _dictionary = new Dictionary<string, LinkedList<ICoroutine>>();
            _list = new LinkedList<ICoroutine>();
        }

        public IEnumerator<LinkedListNode<ICoroutine>> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Add a coroutine with name.
        /// </summary>
        /// <param name="name">Function name.</param>
        /// <param name="coroutine"></param>
        public LinkedListNode<ICoroutine> Add(string name, ICoroutine coroutine)
        {
            if (coroutine is null)
                throw new ArgumentNullException(nameof(coroutine));

            if (!_dictionary.TryGetValue(name, out var list))
            {
                list = new LinkedList<ICoroutine>();
                _dictionary.Add(name, list);
            }

            return list.AddLast(coroutine);
        }

        /// <summary>
        ///     Add a coroutine without name.
        /// </summary>
        /// <param name="coroutine"></param>
        public LinkedListNode<ICoroutine> Add(ICoroutine coroutine)
        {
            if (coroutine is null)
                throw new ArgumentNullException(nameof(coroutine));
            return _list.AddLast(coroutine);
        }

        /// <summary>
        ///     Try to get coroutines with function name.
        /// </summary>
        /// <param name="name">Function name.</param>
        /// <param name="coroutines"></param>
        /// <returns></returns>
        public bool TryGetValue(string name, out IEnumerable<ICoroutine> coroutines)
        {
            var result = _dictionary.TryGetValue(name, out var cs);
            coroutines = cs;
            return result;
        }

        /// <summary>
        ///     Interrupt all coroutines in this pool.
        /// </summary>
        public void Destroy()
        {
            foreach (var coroutineNode in this)
            {
                if (coroutineNode.Value.Status == CoroutineStatus.Paused || coroutineNode.Value.Status == CoroutineStatus.Running)
                    coroutineNode.Value.Interrupt();
            }
        }

        private class Enumerator : IEnumerator<LinkedListNode<ICoroutine>>
        {
            private readonly CoroutinePool _pool;
            private IEnumerator<KeyValuePair<string, LinkedList<ICoroutine>>> _dictionaryEnumerator;
            private bool _firstNode;

            public Enumerator(CoroutinePool pool)
            {
                _pool = pool;
                Reset();
            }

            public bool MoveNext()
            {
                while (true)
                {
                    if (_firstNode) _firstNode = false;
                    else Current = Current?.Next;
                    if (Current != null) return true;
                    if (!_dictionaryEnumerator.MoveNext()) return false;
                    Current = _dictionaryEnumerator.Current.Value.First;
                    _firstNode = true;
                }
            }

            public void Reset()
            {
                Current = _pool._list.First;
                _firstNode = true;
                _dictionaryEnumerator = _pool._dictionary.GetEnumerator();
            }

            public LinkedListNode<ICoroutine> Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _dictionaryEnumerator.Dispose();
            }
        }
    }
}