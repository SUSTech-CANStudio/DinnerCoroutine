using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CANStudio.DinnerCoroutine
{
    internal class CoroutinePool : IEnumerable<ICoroutine>
    {
        private readonly IDictionary<string, List<ICoroutine>> _dictionary;

        private readonly List<ICoroutine> _list;

        public CoroutinePool()
        {
            _dictionary = new Dictionary<string, List<ICoroutine>>();
            _list = new List<ICoroutine>();
        }

        public IEnumerable<ICoroutine> this[string name] => _dictionary[name];

        public IEnumerator<ICoroutine> GetEnumerator()
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
        public void Add(string name, ICoroutine coroutine)
        {
            if (coroutine is null) return;
            if (_dictionary.TryGetValue(name, out var list)) list.Add(coroutine);
            else _dictionary.Add(name, new List<ICoroutine> {coroutine});
        }

        /// <summary>
        ///     Add a coroutine without name.
        /// </summary>
        /// <param name="coroutine"></param>
        public void Add(ICoroutine coroutine)
        {
            if (coroutine is null) return;
            _list.Add(coroutine);
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
        ///     Clean coroutines that no longer need update and keys whose value is empty.
        /// </summary>
        public void Clean()
        {
            foreach (var list in _dictionary.Values)
            {
                list.RemoveAll(coroutine => coroutine.NextUpdate == UpdateCase.None);
            }

            _list.RemoveAll(coroutine => coroutine.NextUpdate == UpdateCase.None);
            
            var toDelete =
                _dictionary.Keys.Where(s => _dictionary[s] is null || !_dictionary[s].GetEnumerator().MoveNext());
            foreach (var key in toDelete) _dictionary.Remove(key);
        }

        private class Enumerator : IEnumerator<ICoroutine>
        {
            private readonly CoroutinePool _pool;
            private IEnumerator<KeyValuePair<string, List<ICoroutine>>> _dictionaryEnumerator;
            private IEnumerator<ICoroutine> _listEnumerator;

            public Enumerator(CoroutinePool pool)
            {
                _pool = pool;
                Reset();
            }

            public bool MoveNext()
            {
                while (true)
                {
                    if (_listEnumerator.MoveNext()) return true;
                    if (!_dictionaryEnumerator.MoveNext()) return false;
                    _listEnumerator.Dispose();
                    _listEnumerator = _dictionaryEnumerator.Current.Value.GetEnumerator();
                }
            }

            public void Reset()
            {
                _listEnumerator = _pool._list.GetEnumerator();
                _dictionaryEnumerator = _pool._dictionary.GetEnumerator();
            }

            public ICoroutine Current => _listEnumerator.Current;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _listEnumerator.Dispose();
                _dictionaryEnumerator.Dispose();
            }
        }
    }
}