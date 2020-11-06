using System;
using System.Collections;
using Object = UnityEngine.Object;

namespace CANStudio.DinnerCoroutine
{
    public class ForkCoroutine : CoroutineBase
    {
        /// <summary>
        ///     Create a fork coroutine.
        /// </summary>
        /// <param name="coroutine"></param>
        public ForkCoroutine(IEnumerator coroutine) : base(coroutine){}

        /// <summary>
        ///     Create a fork coroutine.
        /// </summary>
        /// <param name="coroutine"></param>
        /// <param name="callback">Method to be called after coroutine finished or stopped.</param>
        public ForkCoroutine(IEnumerator coroutine, Action callback) : base(coroutine)
        {
            this.callback += callback;
        }

        internal ForkCoroutine(string functionName, IEnumerator coroutine) : base(functionName, coroutine){}

        internal ForkCoroutine(Object keeper, IEnumerator coroutine) : base(keeper, coroutine){}

        internal ForkCoroutine(Object keeper, string functionName, IEnumerator coroutine) : base(keeper, functionName, coroutine){}
    }
}