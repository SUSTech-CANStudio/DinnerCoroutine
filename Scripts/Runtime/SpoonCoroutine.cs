using System;
using System.Collections;
using Object = UnityEngine.Object;

namespace CANStudio.DinnerCoroutine
{
    /// <summary>
    ///     Spoon coroutine is just like unity's original coroutine, but it provides control methods such as pause and stop.
    /// </summary>
    public class SpoonCoroutine : CoroutineBase
    {
        /// <summary>
        ///     Create a coroutine, you should call Start() to start it.
        /// </summary>
        /// <param name="coroutine"></param>
        public SpoonCoroutine(IEnumerator coroutine) : base(coroutine){}

        /// <summary>
        ///     Create a coroutine, you should call Start() to start it.
        /// </summary>
        /// <param name="coroutine"></param>
        /// <param name="callback">Method to be called after coroutine finished or stopped.</param>
        public SpoonCoroutine(IEnumerator coroutine, Action callback) : base(coroutine)
        {
            this.callback += callback;
        }

        internal SpoonCoroutine(string functionName, IEnumerator coroutine) : base(functionName, coroutine){}

        internal SpoonCoroutine(Object keeper, IEnumerator coroutine) : base(keeper, coroutine){}

        internal SpoonCoroutine(Object keeper, string functionName, IEnumerator coroutine) : base(keeper, functionName, coroutine){}

        protected override bool IsParallel() => false;
    }
}