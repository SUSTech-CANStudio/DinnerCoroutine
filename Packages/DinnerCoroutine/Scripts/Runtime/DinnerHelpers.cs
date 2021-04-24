using System;
using System.Collections;
using UnityEngine;

namespace CANStudio.DinnerCoroutine
{
    public static class DinnerHelpers
    {
        /// <summary>
        ///     Start a <see cref="SpoonCoroutine"/>.
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="coroutine">Name of the coroutine function.</param>
        /// <param name="callback">Method to be called after coroutine finished or stopped.</param>
        /// <returns></returns>
        public static SpoonCoroutine StartSpoon(this MonoBehaviour monoBehaviour, string coroutine, Action callback = null)
        {
            var spoon = new SpoonCoroutine(monoBehaviour, coroutine, DinnerUtilities.GetCoroutine(monoBehaviour, coroutine));
            if (!(callback is null)) spoon.callback += callback;
            spoon.Start();
            return spoon;
        }

        /// <summary>
        ///     Start a <see cref="SpoonCoroutine"/>.
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="coroutine">Name of the coroutine function.</param>
        /// <param name="value">Pass any value as parameter.</param>
        /// <param name="callback">Method to be called after coroutine finished or stopped.</param>
        /// <returns></returns>
        public static SpoonCoroutine StartSpoon(this MonoBehaviour monoBehaviour, string coroutine, object value, Action callback = null)
        {
            var spoon = new SpoonCoroutine(monoBehaviour, coroutine,
                DinnerUtilities.GetCoroutine(monoBehaviour, coroutine, value));
            if (!(callback is null)) spoon.callback += callback;
            spoon.Start();
            return spoon;
        }

        /// <summary>
        ///     Start a <see cref="SpoonCoroutine"/>.
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="coroutine"></param>
        /// <param name="callback">Method to be called after coroutine finished or stopped.</param>
        /// <returns></returns>
        public static SpoonCoroutine StartSpoon(this MonoBehaviour monoBehaviour, IEnumerator coroutine,
            Action callback = null)
        {
            var spoon = new SpoonCoroutine(monoBehaviour, coroutine);
            if (!(callback is null)) spoon.callback += callback;
            spoon.Start();
            return spoon;
        }

        /// <summary>
        ///     Start a <see cref="ForkCoroutine"/>.
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="coroutine">Name of the coroutine function.</param>
        /// <param name="callback">Method to be called after coroutine finished or stopped.</param>
        /// <returns></returns>
        public static ForkCoroutine StartFork(this MonoBehaviour monoBehaviour, string coroutine, Action callback = null)
        {
            var fork = new ForkCoroutine(monoBehaviour, coroutine, DinnerUtilities.GetCoroutine(monoBehaviour, coroutine));
            if (!(callback is null)) fork.callback += callback;
            fork.Start();
            return fork;
        }

        /// <summary>
        ///     Start a <see cref="ForkCoroutine"/>.
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="coroutine">Name of the coroutine function.</param>
        /// <param name="value">Pass any value as parameter.</param>
        /// <param name="callback">Method to be called after coroutine finished or stopped.</param>
        /// <returns></returns>
        public static ForkCoroutine StartFork(this MonoBehaviour monoBehaviour, string coroutine, object value, Action callback = null)
        {
            var fork = new ForkCoroutine(monoBehaviour, coroutine,
                DinnerUtilities.GetCoroutine(monoBehaviour, coroutine, value));
            if (!(callback is null)) fork.callback += callback;
            fork.Start();
            return fork;
        }

        /// <summary>
        ///     Start a <see cref="ForkCoroutine"/>.
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="coroutine"></param>
        /// <param name="callback">Method to be called after coroutine finished or stopped.</param>
        /// <returns></returns>
        public static ForkCoroutine StartFork(this MonoBehaviour monoBehaviour, IEnumerator coroutine,
            Action callback = null)
        {
            var fork = new ForkCoroutine(monoBehaviour, coroutine);
            if (!(callback is null)) fork.callback += callback;
            fork.Start();
            return fork;
        }

        /// <summary>
        ///     Create a <see cref="SpoonCoroutine"/>.
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="coroutine">Name of the coroutine function.</param>
        /// <param name="callback">Method to be called after coroutine finished or stopped.</param>
        /// <returns></returns>
        public static SpoonCoroutine CreateSpoon(this MonoBehaviour monoBehaviour, string coroutine, Action callback = null)
        {
            var spoon = new SpoonCoroutine(monoBehaviour, coroutine, DinnerUtilities.GetCoroutine(monoBehaviour, coroutine));
            if (!(callback is null)) spoon.callback += callback;
            return spoon;
        }

        /// <summary>
        ///     Create a <see cref="SpoonCoroutine"/>.
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="coroutine">Name of the coroutine function.</param>
        /// <param name="value">Pass any value as parameter.</param>
        /// <param name="callback">Method to be called after coroutine finished or stopped.</param>
        /// <returns></returns>
        public static SpoonCoroutine CreateSpoon(this MonoBehaviour monoBehaviour, string coroutine, object value, Action callback = null)
        {
            var spoon = new SpoonCoroutine(monoBehaviour, coroutine,
                DinnerUtilities.GetCoroutine(monoBehaviour, coroutine, value));
            if (!(callback is null)) spoon.callback += callback;
            return spoon;
        }

        /// <summary>
        ///     Create a <see cref="SpoonCoroutine"/>.
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="coroutine"></param>
        /// <param name="callback">Method to be called after coroutine finished or stopped.</param>
        /// <returns></returns>
        public static SpoonCoroutine CreateSpoon(this MonoBehaviour monoBehaviour, IEnumerator coroutine,
            Action callback = null)
        {
            var spoon = new SpoonCoroutine(monoBehaviour, coroutine);
            if (!(callback is null)) spoon.callback += callback;
            return spoon;
        }

        /// <summary>
        ///     Create a <see cref="ForkCoroutine"/>.
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="coroutine">Name of the coroutine function.</param>
        /// <param name="callback">Method to be called after coroutine finished or stopped.</param>
        /// <returns></returns>
        public static ForkCoroutine CreateFork(this MonoBehaviour monoBehaviour, string coroutine, Action callback = null)
        {
            var fork = new ForkCoroutine(monoBehaviour, coroutine, DinnerUtilities.GetCoroutine(monoBehaviour, coroutine));
            if (!(callback is null)) fork.callback += callback;
            return fork;
        }

        /// <summary>
        ///     Create a <see cref="ForkCoroutine"/>.
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="coroutine">Name of the coroutine function.</param>
        /// <param name="value">Pass any value as parameter.</param>
        /// <param name="callback">Method to be called after coroutine finished or stopped.</param>
        /// <returns></returns>
        public static ForkCoroutine CreateFork(this MonoBehaviour monoBehaviour, string coroutine, object value, Action callback = null)
        {
            var fork = new ForkCoroutine(monoBehaviour, coroutine,
                DinnerUtilities.GetCoroutine(monoBehaviour, coroutine, value));
            if (!(callback is null)) fork.callback += callback;
            return fork;
        }

        /// <summary>
        ///     Create a <see cref="ForkCoroutine"/>.
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="coroutine"></param>
        /// <param name="callback">Method to be called after coroutine finished or stopped.</param>
        /// <returns></returns>
        public static ForkCoroutine CreateFork(this MonoBehaviour monoBehaviour, IEnumerator coroutine,
            Action callback = null)
        {
            var fork = new ForkCoroutine(monoBehaviour, coroutine);
            if (!(callback is null)) fork.callback += callback;
            return fork;
        }

        /// <summary>
        ///     Stops all coroutines that created in this mono behaviour.
        ///     This function won't stop unity's original coroutines or not started coroutines.
        /// </summary>
        /// <param name="monoBehaviour"></param>
        public static void StopAllDinnerCoroutines(this MonoBehaviour monoBehaviour)
        {
            Daemon.Instance.StopAll(monoBehaviour);
        }

        /// <summary>
        ///     Stops all coroutines that created by coroutine name.
        ///     This function won't stop unity's original coroutines or not started coroutines.
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="coroutine">Name of the coroutine function.</param>
        public static void StopAllDinnerCoroutines(this MonoBehaviour monoBehaviour, string coroutine)
        {
            Daemon.Instance.StopAll(monoBehaviour, coroutine);
        }
    }
}